using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class RTRRapidFuzzComparerWindow : EditorWindow
{
    private enum Scorer
    {
        SimpleRatio,
        PartialRatio,
        TokenSortRatio,
        TokenSetRatio,
        WeightedRatio,
        QuickRatio
    }

    [Serializable]
    private class CompareRequest
    {
        public string string_a;
        public string string_b;
        public string scorer;
    }

    [Serializable]
    private class CompareResponse
    {
        public string scorer;
        public float score;
        public string error;
    }

    private const string Endpoint = "http://127.0.0.1:5000/rapidfuzz_compare";

    private string stringA = "";
    private string stringB = "";
    private Scorer scorer = Scorer.SimpleRatio;
    private string result = "Enter two strings and choose a scorer.";
    private bool isComparing;
    private UnityWebRequest request;
    private UnityWebRequestAsyncOperation operation;

    [MenuItem("DGCA RTR/RapidFuzz Comparer")]
    private static void ShowWindow()
    {
        GetWindow<RTRRapidFuzzComparerWindow>("RapidFuzz Comparer");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("RapidFuzz String Comparer", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Runs RapidFuzz in the local evaluator service. Start it from DGCA RTR/Local AI Server → START RAPIDFUZZ ONLY.",
            MessageType.Info);

        EditorGUILayout.LabelField("String A");
        stringA = EditorGUILayout.TextArea(stringA, GUILayout.MinHeight(55));
        EditorGUILayout.LabelField("String B");
        stringB = EditorGUILayout.TextArea(stringB, GUILayout.MinHeight(55));

        scorer = (Scorer)EditorGUILayout.EnumPopup("Scorer", scorer);
        using (new EditorGUI.DisabledScope(isComparing))
        {
            if (GUILayout.Button(isComparing ? "Comparing..." : "Compare"))
                Compare();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Result", EditorStyles.boldLabel);
        EditorGUILayout.SelectableLabel(result, EditorStyles.textArea, GUILayout.MinHeight(38));
    }

    private void Compare()
    {
        CompareRequest payload = new CompareRequest
        {
            string_a = stringA,
            string_b = stringB,
            scorer = GetScorerName(scorer)
        };

        request = new UnityWebRequest(Endpoint, UnityWebRequest.kHttpVerbPOST)
        {
            uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(payload))),
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Content-Type", "application/json");
        operation = request.SendWebRequest();
        isComparing = true;
        result = "Waiting for RapidFuzz...";
        EditorApplication.update += PollRequest;
    }

    private void PollRequest()
    {
        if (operation == null || !operation.isDone)
            return;

        EditorApplication.update -= PollRequest;
        isComparing = false;

        if (request.result == UnityWebRequest.Result.Success)
        {
            CompareResponse response = JsonUtility.FromJson<CompareResponse>(request.downloadHandler.text);
            result = response != null && string.IsNullOrEmpty(response.error)
                ? $"{GetDisplayName(scorer)}: {response.score:0.##}%"
                : response?.error ?? "RapidFuzz returned an invalid response.";
        }
        else
        {
            result = "Could not reach the RapidFuzz service. Start it from DGCA RTR/Local AI Server.\n" + request.error;
        }

        request.Dispose();
        request = null;
        operation = null;
        Repaint();
    }

    private void OnDisable()
    {
        EditorApplication.update -= PollRequest;
        request?.Abort();
        request?.Dispose();
        request = null;
        operation = null;
        isComparing = false;
    }

    private static string GetScorerName(Scorer value)
    {
        switch (value)
        {
            case Scorer.PartialRatio: return "partial_ratio";
            case Scorer.TokenSortRatio: return "token_sort_ratio";
            case Scorer.TokenSetRatio: return "token_set_ratio";
            case Scorer.WeightedRatio: return "weighted_ratio";
            case Scorer.QuickRatio: return "quick_ratio";
            default: return "ratio";
        }
    }

    private static string GetDisplayName(Scorer value)
    {
        switch (value)
        {
            case Scorer.PartialRatio: return "Partial Ratio";
            case Scorer.TokenSortRatio: return "Token Sort Ratio";
            case Scorer.TokenSetRatio: return "Token Set Ratio";
            case Scorer.WeightedRatio: return "Weighted Ratio";
            case Scorer.QuickRatio: return "Quick Ratio";
            default: return "Simple Ratio";
        }
    }
}
