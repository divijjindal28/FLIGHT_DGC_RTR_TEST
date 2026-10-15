using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Calls Gemini's generateContent REST API and converts its structured JSON
/// response into the evaluator result used by the RTR UI.
/// </summary>
public class GeminiAnswerEvaluator : IAnswerEvaluator
{
    private const string ApiUrlPrefix =
        "https://generativelanguage.googleapis.com/v1beta/models/";

    private readonly string apiKey;
    private readonly string model;
    private readonly string parentPrompt;

    [Serializable]
    private class GeminiPart
    {
        public string text;
    }

    [Serializable]
    private class GeminiSystemInstruction
    {
        public GeminiPart[] parts;
    }

    [Serializable]
    private class GeminiContent
    {
        public string role;
        public GeminiPart[] parts;
    }

    [Serializable]
    private class GeminiRequest
    {
        public GeminiSystemInstruction systemInstruction;
        public GeminiContent[] contents;
        public GeminiGenerationConfig generationConfig;
    }

    [Serializable]
    private class GeminiGenerationConfig
    {
        public string responseMimeType = "application/json";
        public GeminiResponseSchema responseSchema;
        public GeminiThinkingConfig thinkingConfig;
        public int maxOutputTokens = 1500;
    }

    [Serializable]
    private class GeminiThinkingConfig
    {
        public string thinkingLevel = "low";
    }

    [Serializable]
    private class GeminiResponseSchema
    {
        public string type = "OBJECT";
        public GeminiSchemaProperties properties;
        public string[] required =
        {
            "score",
            "correctness",
            "severity",
            "reason"
        };
    }

    [Serializable]
    private class GeminiSchemaProperties
    {
        public GeminiFieldSchema score = new GeminiFieldSchema("INTEGER");
        public GeminiFieldSchema correctness = new GeminiFieldSchema("BOOLEAN");
        public GeminiFieldSchema severity = new GeminiFieldSchema(
            "STRING",
            new[] { "none", "minor", "major", "critical" });
        public GeminiFieldSchema reason = new GeminiFieldSchema("STRING");
    }

    [Serializable]
    private class GeminiFieldSchema
    {
        public string type;
        public string[] @enum;

        public GeminiFieldSchema(string fieldType, string[] allowedValues = null)
        {
            type = fieldType;
            @enum = allowedValues;
        }
    }

    [Serializable]
    private class GeminiResponse
    {
        public GeminiCandidate[] candidates;
        public GeminiApiError error;
    }

    [Serializable]
    private class GeminiCandidate
    {
        public GeminiContent content;
        public string finishReason;
    }

    [Serializable]
    private class GeminiApiError
    {
        public int code;
        public string message;
        public string status;
    }

    public GeminiAnswerEvaluator(string apiKey, string model, string parentPrompt)
    {
        this.apiKey = apiKey;
        this.model = model;
        this.parentPrompt = parentPrompt;
    }

    public async Task<RTRValidationResult> Evaluate(
        QuestionData question,
        string studentAnswer,
        string expectedAnswer)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException(
                "Gemini API key is empty. Enter it in the RTR Answer Evaluator Tester Inspector.");

        if (string.IsNullOrWhiteSpace(model))
            throw new InvalidOperationException(
                "Gemini model ID is empty. Enter a supported Gemini model in the Inspector.");

        if (string.IsNullOrWhiteSpace(parentPrompt))
            throw new InvalidOperationException(
                "Gemini parent prompt is empty. Enter it in the RTR Answer Evaluator Tester Inspector.");

        if (question == null)
            throw new ArgumentNullException(nameof(question));

        string userPrompt =
            "Evaluate the student's response using the evaluation rules.\n\n" +
            "QUESTION:\n" + (question.question ?? string.Empty) + "\n\n" +
            "SCENARIO DESCRIPTION:\n" + (question.description ?? string.Empty) + "\n\n" +
            "EXPECTED ANSWER:\n" + (expectedAnswer ?? string.Empty) + "\n\n" +
            "STUDENT ANSWER:\n" + (studentAnswer ?? string.Empty);

        GeminiRequest payload = new GeminiRequest
        {
            systemInstruction = new GeminiSystemInstruction
            {
                parts = new[] { new GeminiPart { text = parentPrompt } }
            },
            contents = new[]
            {
                new GeminiContent
                {
                    role = "user",
                    parts = new[] { new GeminiPart { text = userPrompt } }
                }
            },
            generationConfig = new GeminiGenerationConfig
            {
                responseSchema = new GeminiResponseSchema
                {
                    properties = new GeminiSchemaProperties()
                },
                thinkingConfig = new GeminiThinkingConfig()
            }
        };

        string requestUrl = ApiUrlPrefix + model.Trim() + ":generateContent";
        byte[] requestBody = Encoding.UTF8.GetBytes(JsonUtility.ToJson(payload));

        using (UnityWebRequest request = new UnityWebRequest(
            requestUrl,
            UnityWebRequest.kHttpVerbPOST))
        {
            request.uploadHandler = new UploadHandlerRaw(requestBody);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = 90;
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("x-goog-api-key", apiKey.Trim());

            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield();

            string responseBody = request.downloadHandler.text;
            if (request.result != UnityWebRequest.Result.Success)
                throw new Exception(FormatApiError(request, responseBody));

            return ParseEvaluation(responseBody);
        }
    }

    private static RTRValidationResult ParseEvaluation(string responseBody)
    {
        GeminiResponse response;
        try
        {
            response = JsonUtility.FromJson<GeminiResponse>(responseBody);
        }
        catch (Exception exception)
        {
            throw new Exception("Could not parse Gemini's API response.", exception);
        }

        if (response == null)
            throw new Exception("Gemini returned an empty API response.");

        if (response.error != null && !string.IsNullOrEmpty(response.error.message))
            throw new Exception(
                "Gemini API error " + response.error.code + ": " + response.error.message);

        if (response.candidates == null || response.candidates.Length == 0 ||
            response.candidates[0].content == null ||
            response.candidates[0].content.parts == null ||
            response.candidates[0].content.parts.Length == 0)
        {
            throw new Exception("Gemini returned no evaluation text.");
        }

        string resultJson = response.candidates[0].content.parts[0].text;
        if (string.IsNullOrWhiteSpace(resultJson))
            throw new Exception("Gemini returned an empty evaluation.");

        resultJson = RemoveMarkdownCodeFence(resultJson.Trim());

        RTRValidationResult result;
        try
        {
            result = JsonUtility.FromJson<RTRValidationResult>(resultJson);
        }
        catch (Exception exception)
        {
            throw new Exception(
                "Gemini returned invalid evaluation JSON:\n" + resultJson,
                exception);
        }

        if (result == null || result.score < 0 || result.score > 100 ||
            string.IsNullOrWhiteSpace(result.severity) ||
            string.IsNullOrWhiteSpace(result.reason))
        {
            throw new Exception(
                "Gemini evaluation JSON is missing required fields or has an invalid score:\n" +
                resultJson);
        }

        if (result.severity != "none" && result.severity != "minor" &&
            result.severity != "major" && result.severity != "critical")
        {
            throw new Exception("Gemini returned an unsupported severity: " + result.severity);
        }

        return result;
    }

    private static string RemoveMarkdownCodeFence(string value)
    {
        if (!value.StartsWith("```", StringComparison.Ordinal))
            return value;

        int firstNewline = value.IndexOf('\n');
        if (firstNewline < 0)
            return value;

        value = value.Substring(firstNewline + 1).Trim();
        if (value.EndsWith("```", StringComparison.Ordinal))
            value = value.Substring(0, value.Length - 3).Trim();

        return value;
    }

    private static string FormatApiError(UnityWebRequest request, string responseBody)
    {
        string message = request.error;
        try
        {
            GeminiResponse response = JsonUtility.FromJson<GeminiResponse>(responseBody);
            if (response != null && response.error != null &&
                !string.IsNullOrEmpty(response.error.message))
            {
                message += "\nGemini: " + response.error.message;
            }
        }
        catch
        {
            // Keep the UnityWebRequest error when the API body isn't Gemini JSON.
        }

        return "Gemini evaluation failed (HTTP " + request.responseCode + "): " +
            message + "\nResponse: " + responseBody;
    }
}
