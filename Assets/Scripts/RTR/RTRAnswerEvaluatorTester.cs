using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RTRAnswerEvaluatorTester : MonoBehaviour
{
    [Serializable]
    private class GradioRequest
    {
        public string[] data;
    }

    [Serializable]
    private class GradioEventResponse
    {
        public string event_id;
    }

    [Header("References")]
    [SerializeField] private RTRQuestionNavigator questionNavigator;

    [SerializeField] private TMP_InputField studentAnswerInput;

    [SerializeField] private Button sendButton;

    [Header("Result UI")]
    [SerializeField] private TMP_Text reasonText;

    [SerializeField] private TMP_Text correctnessText;

    [Header("Hugging Face")]
    [SerializeField]
    private string spaceUrl =
        "https://divij28-dgca-rtr-evaluator.hf.space";

    [Header("Development Token")]
    [SerializeField] private string hfToken;

    private QuestionData currentQuestion;

    private void Start()
    {
        if (sendButton != null)
            sendButton.onClick.AddListener(SendAnswer);

        RefreshCurrentQuestion();
    }

    private void Update()
    {
        RefreshCurrentQuestion();
    }

    private void RefreshCurrentQuestion()
    {
        if (questionNavigator == null)
            return;

        QuestionData newQuestion =
            questionNavigator.CurrentQuestion;

        if (newQuestion == null)
            return;

        if (newQuestion == currentQuestion)
            return;

        currentQuestion = newQuestion;

        // Put the correct answer into the input field
        // whenever the question changes.
        if (studentAnswerInput != null)
        {
            studentAnswerInput.text =
                currentQuestion.answer;

            studentAnswerInput.caretPosition =
                studentAnswerInput.text.Length;
        }

        // Clear previous result.
        if (reasonText != null)
            reasonText.text = "";

        if (correctnessText != null)
            correctnessText.text = "";
    }

    public void SendAnswer()
    {
        if (currentQuestion == null)
        {
            Debug.LogWarning(
                "RTRAnswerEvaluatorTester: No current question."
            );

            return;
        }

        if (studentAnswerInput == null)
        {
            Debug.LogWarning(
                "RTRAnswerEvaluatorTester: Student Answer InputField is not assigned."
            );

            return;
        }

        StartCoroutine(SendEvaluationRequest());
    }

    private IEnumerator SendEvaluationRequest()
    {
        if (sendButton != null)
            sendButton.interactable = false;

        if (reasonText != null)
            reasonText.text = "Evaluating...";

        if (correctnessText != null)
            correctnessText.text = "";

        string studentAnswer =
            studentAnswerInput.text;

        // --------------------------------------------------
        // Build Gradio request
        // --------------------------------------------------

        string requestJson =
            JsonUtility.ToJson(
                new GradioRequest
                {
                    data = new string[]
                    {
                        currentQuestion.question,
                        currentQuestion.description,
                        currentQuestion.answer,
                        studentAnswer
                    }
                }
            );

        Debug.Log(
            "Sending RTR evaluation request:\n" +
            requestJson
        );

        // --------------------------------------------------
        // Gradio API endpoint
        // --------------------------------------------------

        string url =
            spaceUrl +
            "/gradio_api/call/evaluate_answer";

        using (UnityWebRequest request =
               new UnityWebRequest(url, "POST"))
        {
            byte[] body =
                Encoding.UTF8.GetBytes(requestJson);

            request.uploadHandler =
                new UploadHandlerRaw(body);

            request.downloadHandler =
                new DownloadHandlerBuffer();

            request.SetRequestHeader(
                "Content-Type",
                "application/json"
            );

            // Development authentication
            if (!string.IsNullOrEmpty(hfToken))
            {
                request.SetRequestHeader(
                    "Authorization",
                    "Bearer " + hfToken
                );
            }

            yield return request.SendWebRequest();

            // --------------------------------------------------
            // Handle API error
            // --------------------------------------------------

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(
                    "RTR API Error:\n" +
                    request.error +
                    "\n\nResponse:\n" +
                    request.downloadHandler.text
                );

                if (reasonText != null)
                {
                    reasonText.text =
                        "API ERROR\n\n" +
                        request.error;
                }

                if (correctnessText != null)
                    correctnessText.text = "";

                if (sendButton != null)
                    sendButton.interactable = true;

                yield break;
            }

            // --------------------------------------------------
            // Submission response
            // --------------------------------------------------

            string response =
                request.downloadHandler.text;

            Debug.Log(
                "RTR API Submission Response:\n" +
                response
            );

            GradioEventResponse eventResponse;

            try
            {
                eventResponse =
                    JsonUtility.FromJson<GradioEventResponse>(
                        response
                    );
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "Could not parse Gradio event response:\n" +
                    exception.Message
                );

                if (reasonText != null)
                    reasonText.text = response;

                if (sendButton != null)
                    sendButton.interactable = true;

                yield break;
            }

            // --------------------------------------------------
            // Check event ID
            // --------------------------------------------------

            if (string.IsNullOrEmpty(eventResponse.event_id))
            {
                Debug.LogError(
                    "Gradio did not return an event ID."
                );

                if (reasonText != null)
                {
                    reasonText.text =
                        "No event ID returned.\n\n" +
                        response;
                }

                if (sendButton != null)
                    sendButton.interactable = true;

                yield break;
            }

            Debug.Log(
                "Gradio Event ID: " +
                eventResponse.event_id
            );

            // --------------------------------------------------
            // Get final evaluation
            // --------------------------------------------------

            yield return StartCoroutine(
                GetEvaluationResult(
                    eventResponse.event_id
                )
            );
        }
    }

    private IEnumerator GetEvaluationResult(
        string eventId
    )
    {
        string url =
            spaceUrl +
            "/gradio_api/call/evaluate_answer/" +
            eventId;

        Debug.Log(
            "Requesting RTR evaluation result:\n" +
            url
        );

        using (UnityWebRequest request =
               UnityWebRequest.Get(url))
        {
            if (!string.IsNullOrEmpty(hfToken))
            {
                request.SetRequestHeader(
                    "Authorization",
                    "Bearer " + hfToken
                );
            }

            yield return request.SendWebRequest();

            // --------------------------------------------------
            // Handle result error
            // --------------------------------------------------

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(
                    "RTR API Result Error:\n" +
                    request.error +
                    "\n\nResponse:\n" +
                    request.downloadHandler.text
                );

                if (reasonText != null)
                {
                    reasonText.text =
                        "RESULT ERROR\n\n" +
                        request.error;
                }

                if (correctnessText != null)
                    correctnessText.text = "";

                if (sendButton != null)
                    sendButton.interactable = true;

                yield break;
            }

            string response =
                request.downloadHandler.text;

            Debug.Log(
                "RTR API Final Response:\n" +
                response
            );

            // --------------------------------------------------
            // Parse Gradio SSE response
            // --------------------------------------------------

            string jsonResult =
                ExtractResultJson(response);

            if (string.IsNullOrEmpty(jsonResult))
            {
                Debug.LogError(
                    "Could not extract evaluation JSON from Gradio response."
                );

                if (reasonText != null)
                {
                    reasonText.text =
                        "Could not parse evaluation result.";
                }

                if (sendButton != null)
                    sendButton.interactable = true;

                yield break;
            }

            Debug.Log(
                "Extracted Evaluation JSON:\n" +
                jsonResult
            );

            // --------------------------------------------------
            // Parse RTR result
            // --------------------------------------------------

            RTRValidationResult result;

            try
            {
                result =
                    JsonUtility.FromJson<RTRValidationResult>(
                        jsonResult
                    );
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "Could not parse RTRValidationResult:\n" +
                    exception.Message
                );

                if (reasonText != null)
                    reasonText.text =
                        "Could not parse evaluation JSON.";

                if (sendButton != null)
                    sendButton.interactable = true;

                yield break;
            }

            // --------------------------------------------------
            // Display correctness
            // --------------------------------------------------

            if (correctnessText != null)
            {
                correctnessText.text =
                    result.correctness
                        ? "Correct"
                        : "Incorrect";
            }

            // --------------------------------------------------
            // Display reason
            // --------------------------------------------------

            if (reasonText != null)
            {
                reasonText.text =
                    result.reason;
            }

            Debug.Log(
                "RTR Evaluation Result\n" +
                "Score: " + result.score +
                "\nCorrectness: " + result.correctness +
                "\nSeverity: " + result.severity +
                "\nReason: " + result.reason
            );

            if (sendButton != null)
                sendButton.interactable = true;
        }
    }

    // --------------------------------------------------
    // Extract JSON from Gradio SSE response
    // --------------------------------------------------

    private string ExtractResultJson(string response)
    {
        if (string.IsNullOrEmpty(response))
            return null;

        string[] lines =
            response.Split(
                new[]
                {
                    '\n',
                    '\r'
                },
                StringSplitOptions.RemoveEmptyEntries
            );

        foreach (string line in lines)
        {
            string trimmedLine =
                line.Trim();

            if (!trimmedLine.StartsWith("data:"))
                continue;

            string data =
                trimmedLine.Substring(5).Trim();

            if (string.IsNullOrEmpty(data))
                continue;

            // Gradio returns:
            //
            // data: ["{\"score\":100,...}"]
            //
            // So first parse the outer JSON array.

            try
            {
                string[] dataArray =
                    JsonHelper.FromJson<string>(data);

                if (dataArray != null &&
                    dataArray.Length > 0)
                {
                    return dataArray[0];
                }
            }
            catch
            {
                // Ignore non-result SSE data.
            }
        }

        return null;
    }

    // --------------------------------------------------
    // Unity JsonUtility does not directly support
    // top-level JSON arrays.
    // This helper wraps the array.
    // --------------------------------------------------

    private static class JsonHelper
    {
        [Serializable]
        private class Wrapper<T>
        {
            public T[] Items;
        }

        public static T[] FromJson<T>(string json)
        {
            string wrappedJson =
                "{\"Items\":" +
                json +
                "}";

            Wrapper<T> wrapper =
                JsonUtility.FromJson<Wrapper<T>>(
                    wrappedJson
                );

            return wrapper.Items;
        }
    }

    // --------------------------------------------------
    // Cleanup
    // --------------------------------------------------

    private void OnDestroy()
    {
        if (sendButton != null)
            sendButton.onClick.RemoveListener(SendAnswer);
    }
}