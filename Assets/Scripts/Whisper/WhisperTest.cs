using System.IO;
using UnityEngine;

public class WhisperTest : MonoBehaviour
{
    [Header("Microphone")]
    [SerializeField] private int recordingLength = 5;
    [SerializeField] private int sampleRate = 16000;

    private AudioClip microphoneClip;
    private string microphoneDevice;
    private bool modelLoaded;

    private void Start()
    {
        LoadWhisper();

        if (!modelLoaded)
            return;

        StartMicrophone();
    }

    private void LoadWhisper()
    {
        string modelPath = Path.Combine(
            Application.streamingAssetsPath,
            "Whisper",
            "ggml-base.en.bin"
        );

        Debug.Log("Whisper model path:");
        Debug.Log(modelPath);

        if (!File.Exists(modelPath))
        {
            Debug.LogError(
                "Whisper model not found: " + modelPath
            );

            return;
        }

        Debug.Log("Whisper model found.");

        modelLoaded = WhisperNative.Whisper_LoadModel(
            modelPath
        );

        if (modelLoaded)
        {
            Debug.Log("SUCCESS: Whisper model loaded.");
        }
        else
        {
            Debug.LogError(
                "FAILED: Whisper model could not be loaded."
            );
        }
    }

    private void StartMicrophone()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("No microphone detected.");
            return;
        }

        microphoneDevice = Microphone.devices[0];

        Debug.Log(
            "Using microphone: " + microphoneDevice
        );

        microphoneClip = Microphone.Start(
            microphoneDevice,
            false,
            recordingLength,
            sampleRate
        );

        Debug.Log(
            "Recording for " +
            recordingLength +
            " seconds..."
        );

        Invoke(
            nameof(StopRecordingAndTranscribe),
            recordingLength
        );
    }

    private void StopRecordingAndTranscribe()
    {
        if (microphoneClip == null)
        {
            Debug.LogError("Microphone clip is null.");
            return;
        }

        Microphone.End(microphoneDevice);

        Debug.Log("Recording finished.");

        float[] samples = new float[
            microphoneClip.samples *
            microphoneClip.channels
        ];

        microphoneClip.GetData(samples, 0);

        Debug.Log(
            "Audio samples: " +
            samples.Length
        );

        Debug.Log(
            "Sending audio to Whisper..."
        );

        string transcription =
            WhisperNative.Transcribe(samples);

        Debug.Log(
            "WHISPER RESULT: " +
            transcription
        );
    }

    private void OnDestroy()
    {
        if (!string.IsNullOrEmpty(microphoneDevice))
        {
            if (Microphone.IsRecording(microphoneDevice))
                Microphone.End(microphoneDevice);
        }

        WhisperNative.Whisper_UnloadModel();
    }
}