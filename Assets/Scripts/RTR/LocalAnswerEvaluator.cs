using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class LocalAnswerEvaluator : IAnswerEvaluator
{
    private const string LocalEvaluatorUrl =
        "http://127.0.0.1:5000/evaluate";

    public async Task<RTRValidationResult> Evaluate(
        QuestionData question,
        string studentAnswer)
    {
        if (question == null)
        {
            Debug.LogError(
                "LocalAnswerEvaluator: Question is null."
            );

            return null;
        }

        LocalEvaluationRequest requestData =
            new LocalEvaluationRequest
            {
                question = question.question,
                description = question.description,
                expected_answer = question.answer,
                student_answer = studentAnswer
            };

        string json =
            JsonUtility.ToJson(requestData);

        Debug.Log(
            "Local RTR Evaluation Request:\n" +
            json
        );

        using (UnityWebRequest request =
               new UnityWebRequest(
                   LocalEvaluatorUrl,
                   "POST"))
        {
            byte[] body =
                Encoding.UTF8.GetBytes(json);

            request.uploadHandler =
                new UploadHandlerRaw(body);

            request.downloadHandler =
                new DownloadHandlerBuffer();

            request.SetRequestHeader(
                "Content-Type",
                "application/json"
            );

            var operation =
                request.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (request.result !=
                UnityWebRequest.Result.Success)
            {
                Debug.LogError(
                    "Local RTR Evaluation Error:\n" +
                    request.error +
                    "\n\nResponse:\n" +
                    request.downloadHandler.text
                );

                return null;
            }

            string response =
                request.downloadHandler.text;

            Debug.Log(
                "Local RTR Evaluation Response:\n" +
                response
            );

            try
            {
                RTRValidationResult result =
                    JsonUtility.FromJson<RTRValidationResult>(
                        response
                    );

                return result;
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "LocalAnswerEvaluator: " +
                    "Could not parse evaluation result.\n" +
                    exception.Message +
                    "\n\nResponse:\n" +
                    response
                );

                return null;
            }
        }
    }

    [Serializable]
    private class LocalEvaluationRequest
    {
        public string question;
        public string description;
        public string expected_answer;
        public string student_answer;
    }
}