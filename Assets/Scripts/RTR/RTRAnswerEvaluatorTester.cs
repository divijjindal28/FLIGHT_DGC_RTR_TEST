using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


public enum EvaluationMode
{
    Online,
    Local
}


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


    // --------------------------------------------------
    // References
    // --------------------------------------------------

    [Header("References")]
    [SerializeField] private RTRQuestionNavigator questionNavigator;

    [SerializeField] private TMP_InputField studentAnswerInput;

    [SerializeField] private Button sendButton;


    // --------------------------------------------------
    // Result UI
    // --------------------------------------------------

    [Header("Result UI")]
    [SerializeField] private TMP_Text reasonText;

    [SerializeField] private TMP_Text correctnessText;


    // --------------------------------------------------
    // Evaluation Mode
    // --------------------------------------------------

    [Header("Evaluation Mode")]
    [SerializeField]
    private EvaluationMode evaluationMode =
        EvaluationMode.Online;


    // --------------------------------------------------
    // Hugging Face
    // --------------------------------------------------

    [Header("Hugging Face")]
    [SerializeField]
    private string spaceUrl =
        "https://divij28-dgca-rtr-evaluator.hf.space";

    [Header("Development Token")]
    [SerializeField]
    private string hfToken;


    // --------------------------------------------------
    // Current Question
    // --------------------------------------------------

    private QuestionData currentQuestion;


    // --------------------------------------------------
    // Evaluator
    // --------------------------------------------------

    private IAnswerEvaluator answerEvaluator;


    // --------------------------------------------------
    // Start
    // --------------------------------------------------

    private void Start()
    {
        SetupEvaluator();

        if (sendButton != null)
            sendButton.onClick.AddListener(SendAnswer);

        RefreshCurrentQuestion();
    }


    // --------------------------------------------------
    // Setup Evaluator
    // --------------------------------------------------

    private void SetupEvaluator()
    {
        switch (evaluationMode)
        {
            case EvaluationMode.Online:

                answerEvaluator =
                    new OnlineAnswerEvaluator(
                        spaceUrl,
                        hfToken
                    );

                Debug.Log(
                    "RTR Evaluator Mode: ONLINE"
                );

                break;


            case EvaluationMode.Local:

                answerEvaluator =
                    new LocalAnswerEvaluator();

                Debug.Log(
                    "RTR Evaluator Mode: LOCAL"
                );

                break;
        }
    }


    // --------------------------------------------------
    // Update
    // --------------------------------------------------

    private void Update()
    {
        RefreshCurrentQuestion();
    }


    // --------------------------------------------------
    // Question Refresh
    // --------------------------------------------------

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


    // --------------------------------------------------
    // Send Answer
    // --------------------------------------------------

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
                "RTRAnswerEvaluatorTester: " +
                "Student Answer InputField is not assigned."
            );

            return;
        }


        if (answerEvaluator == null)
        {
            Debug.LogWarning(
                "RTRAnswerEvaluatorTester: " +
                "No evaluator has been initialized."
            );

            return;
        }


        StartCoroutine(
            EvaluateCurrentAnswer()
        );
    }


    // --------------------------------------------------
    // Evaluate Current Answer
    // --------------------------------------------------

    private IEnumerator EvaluateCurrentAnswer()
    {
        if (sendButton != null)
            sendButton.interactable = false;


        if (reasonText != null)
            reasonText.text = "Evaluating...";


        if (correctnessText != null)
            correctnessText.text = "";


        string studentAnswer =
            studentAnswerInput.text;


        Task<RTRValidationResult> evaluationTask =
            answerEvaluator.Evaluate(
                currentQuestion,
                studentAnswer
            );


        while (!evaluationTask.IsCompleted)
            yield return null;


        if (evaluationTask.IsFaulted)
        {
            Debug.LogError(
                "RTR Evaluation Exception:\n" +
                evaluationTask.Exception
            );


            if (reasonText != null)
            {
                reasonText.text =
                    "Evaluation failed.";
            }


            if (correctnessText != null)
                correctnessText.text = "";


            if (sendButton != null)
                sendButton.interactable = true;


            yield break;
        }


        RTRValidationResult result =
            evaluationTask.Result;


        if (result == null)
        {
            Debug.LogError(
                "RTRAnswerEvaluatorTester: " +
                "Evaluator returned null."
            );


            if (reasonText != null)
            {
                reasonText.text =
                    "No evaluation result received.";
            }


            if (correctnessText != null)
                correctnessText.text = "";


            if (sendButton != null)
                sendButton.interactable = true;


            yield break;
        }


        // --------------------------------------------------
        // Display Correctness
        // --------------------------------------------------

        if (correctnessText != null)
        {
            correctnessText.text =
                result.correctness
                    ? "Correct"
                    : "Incorrect";
        }


        // --------------------------------------------------
        // Display Reason
        // --------------------------------------------------

        if (reasonText != null)
        {
            reasonText.text =
                result.reason;
        }


        // --------------------------------------------------
        // Debug
        // --------------------------------------------------

        Debug.Log(
            "RTR Evaluation Result\n" +
            "Score: " + result.score +
            "\nCorrectness: " +
            result.correctness +
            "\nSeverity: " +
            result.severity +
            "\nReason: " +
            result.reason
        );


        if (sendButton != null)
            sendButton.interactable = true;
    }


    // ==================================================
    // ONLINE ANSWER EVALUATOR
    // ==================================================

    private class OnlineAnswerEvaluator : IAnswerEvaluator
    {
        private readonly string spaceUrl;
        private readonly string hfToken;


        public OnlineAnswerEvaluator(
            string spaceUrl,
            string hfToken
        )
        {
            this.spaceUrl =
                spaceUrl;

            this.hfToken =
                hfToken;
        }


        public async Task<RTRValidationResult> Evaluate(
            QuestionData question,
            string studentAnswer
        )
        {
            OnlineEvaluationRequest requestData =
                new OnlineEvaluationRequest
                {
                    question =
                        question.question,

                    description =
                        question.description,

                    expected_answer =
                        question.answer,

                    student_answer =
                        studentAnswer
                };


            string requestJson =
                JsonUtility.ToJson(
                    new GradioRequest
                    {
                        data = new string[]
                        {
                            requestData.question,
                            requestData.description,
                            requestData.expected_answer,
                            requestData.student_answer
                        }
                    }
                );


            Debug.Log(
                "Sending ONLINE RTR evaluation request:\n" +
                requestJson
            );


            string submitUrl =
                spaceUrl +
                "/gradio_api/call/evaluate_answer";


            using (UnityWebRequest request =
                   new UnityWebRequest(
                       submitUrl,
                       "POST"))
            {
                byte[] body =
                    Encoding.UTF8.GetBytes(
                        requestJson
                    );


                request.uploadHandler =
                    new UploadHandlerRaw(body);


                request.downloadHandler =
                    new DownloadHandlerBuffer();


                request.SetRequestHeader(
                    "Content-Type",
                    "application/json"
                );


                if (!string.IsNullOrEmpty(hfToken))
                {
                    request.SetRequestHeader(
                        "Authorization",
                        "Bearer " + hfToken
                    );
                }


                var operation =
                    request.SendWebRequest();


                while (!operation.isDone)
                    await Task.Yield();


                if (request.result !=
                    UnityWebRequest.Result.Success)
                {
                    throw new Exception(
                        "Online RTR API Error:\n" +
                        request.error +
                        "\n\nResponse:\n" +
                        request.downloadHandler.text
                    );
                }


                string response =
                    request.downloadHandler.text;


                Debug.Log(
                    "ONLINE RTR Submission Response:\n" +
                    response
                );


                GradioEventResponse eventResponse =
                    JsonUtility.FromJson<
                        GradioEventResponse
                    >(response);


                if (string.IsNullOrEmpty(
                    eventResponse.event_id))
                {
                    throw new Exception(
                        "Gradio did not return an event ID."
                    );
                }


                return await GetOnlineResult(
                    eventResponse.event_id
                );
            }
        }


        private async Task<RTRValidationResult>
            GetOnlineResult(
                string eventId
            )
        {
            string resultUrl =
                spaceUrl +
                "/gradio_api/call/evaluate_answer/" +
                eventId;


            using (UnityWebRequest request =
                   UnityWebRequest.Get(resultUrl))
            {
                if (!string.IsNullOrEmpty(hfToken))
                {
                    request.SetRequestHeader(
                        "Authorization",
                        "Bearer " + hfToken
                    );
                }


                var operation =
                    request.SendWebRequest();


                while (!operation.isDone)
                    await Task.Yield();


                if (request.result !=
                    UnityWebRequest.Result.Success)
                {
                    throw new Exception(
                        "Online RTR Result Error:\n" +
                        request.error +
                        "\n\nResponse:\n" +
                        request.downloadHandler.text
                    );
                }


                string response =
                    request.downloadHandler.text;


                Debug.Log(
                    "ONLINE RTR Final Response:\n" +
                    response
                );


                string jsonResult =
                    ExtractResultJson(
                        response
                    );


                if (string.IsNullOrEmpty(
                    jsonResult))
                {
                    throw new Exception(
                        "Could not extract evaluation JSON."
                    );
                }


                RTRValidationResult result =
                    JsonUtility.FromJson<
                        RTRValidationResult
                    >(jsonResult);


                return result;
            }
        }


        private string ExtractResultJson(
            string response
        )
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
                    StringSplitOptions
                        .RemoveEmptyEntries
                );


            foreach (string line in lines)
            {
                string trimmedLine =
                    line.Trim();


                if (!trimmedLine.StartsWith(
                    "data:"))
                {
                    continue;
                }


                string data =
                    trimmedLine
                        .Substring(5)
                        .Trim();


                if (string.IsNullOrEmpty(data))
                    continue;


                try
                {
                    string[] dataArray =
                        JsonHelper.FromJson<string>(
                            data
                        );


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
    }


    // ==================================================
    // ONLINE REQUEST DATA
    // ==================================================

    [Serializable]
    private class OnlineEvaluationRequest
    {
        public string question;
        public string description;
        public string expected_answer;
        public string student_answer;
    }


    // ==================================================
    // JSON HELPER
    // ==================================================

    private static class JsonHelper
    {
        [Serializable]
        private class Wrapper<T>
        {
            public T[] Items;
        }


        public static T[] FromJson<T>(
            string json
        )
        {
            string wrappedJson =
                "{\"Items\":" +
                json +
                "}";


            Wrapper<T> wrapper =
                JsonUtility.FromJson<
                    Wrapper<T>
                >(wrappedJson);


            return wrapper.Items;
        }
    }


    // ==================================================
    // CLEANUP
    // ==================================================

    private void OnDestroy()
    {
        if (sendButton != null)
            sendButton.onClick.RemoveListener(
                SendAnswer
            );
    }
}