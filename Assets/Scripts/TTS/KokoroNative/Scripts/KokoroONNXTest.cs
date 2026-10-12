using System;
using System.IO;
using UnityEngine;
using Microsoft.ML.OnnxRuntime;

public class KokoroONNXTest : MonoBehaviour
{
    [Header("ONNX Model")]
    [SerializeField]
    private string modelPath;

    private InferenceSession session;

    private void Start()
    {
        Debug.Log("[Kokoro ONNX] Starting model load test...");

        if (string.IsNullOrWhiteSpace(modelPath))
        {
            Debug.LogError("[Kokoro ONNX] Model path is not assigned.");
            return;
        }

        Debug.Log("[Kokoro ONNX] Model path:");
        Debug.Log(modelPath);

        if (!File.Exists(modelPath))
        {
            Debug.LogError(
                "[Kokoro ONNX] Model file does not exist at:\n" +
                modelPath
            );
            return;
        }

        try
        {
            Debug.Log("[Kokoro ONNX] Creating InferenceSession...");

            session = new InferenceSession(modelPath);

            Debug.Log(
                "[Kokoro ONNX] SUCCESS: InferenceSession created."
            );

            Debug.Log(
                "[Kokoro ONNX] Inputs: " +
                session.InputMetadata.Count
            );

            foreach (var input in session.InputMetadata)
            {
                Debug.Log(
                    $"[Kokoro ONNX] INPUT: {input.Key} | " +
                    $"Type: {input.Value.ElementType} | " +
                    $"Shape: {string.Join(", ", input.Value.Dimensions)}"
                );
            }

            Debug.Log(
                "[Kokoro ONNX] Outputs: " +
                session.OutputMetadata.Count
            );

            foreach (var output in session.OutputMetadata)
            {
                Debug.Log(
                    $"[Kokoro ONNX] OUTPUT: {output.Key} | " +
                    $"Type: {output.Value.ElementType} | " +
                    $"Shape: {string.Join(", ", output.Value.Dimensions)}"
                );
            }
        }
        catch (Exception e)
        {
            Debug.LogError(
                "[Kokoro ONNX] FAILED to create InferenceSession:\n" +
                e
            );
        }
    }

    private void OnDestroy()
    {
        if (session != null)
        {
            session.Dispose();
            session = null;
        }
    }
}