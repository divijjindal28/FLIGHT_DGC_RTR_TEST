using System;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WhisperRealtimeRecorder : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button recordButton;
    [SerializeField] private TMP_Text transcriptText;

    [Header("Recording")]
    [SerializeField] private int sampleRate = 16000;
    [SerializeField] private int maxRecordingLengthSeconds = 120;

    [Header("Button Colors")]
    [SerializeField] private Color normalColor = Color.blue;
    [SerializeField] private Color recordingColor = Color.red;

    private AudioClip microphoneClip;
    private string microphoneDevice;

    private bool modelLoaded;
    private bool isRecording;
    private bool transcriptionInProgress;

    private Task<string> transcriptionTask;

    private UIProceduralRoundedRect recordButtonBackground;

    private void Start()
    {
        LoadWhisper();

        if (!modelLoaded)
            return;

        SetupButton();

        if (transcriptText != null)
            transcriptText.text = "";
    }

    // =========================================================
    // WHISPER
    // =========================================================

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

    // =========================================================
    // BUTTON
    // =========================================================

    private void SetupButton()
    {
        if (recordButton == null)
        {
            Debug.LogError(
                "WhisperRealtimeRecorder: Record Button is not assigned."
            );

            return;
        }

        recordButton.onClick.AddListener(
            ToggleRecording
        );

        // Get first child of the button
        if (recordButton.transform.childCount > 0)
        {
            Transform background =
                recordButton.transform.GetChild(0);

            recordButtonBackground =
                background.GetComponent<UIProceduralRoundedRect>();

            if (recordButtonBackground != null)
            {
                recordButtonBackground.color =
                    normalColor;
            }
        }
        else
        {
            Debug.LogWarning(
                "WhisperRealtimeRecorder: Record Button has no child Background."
            );
        }
    }

    public void ToggleRecording()
    {
        if (transcriptionInProgress)
        {
            Debug.Log(
                "Whisper is still transcribing. Please wait."
            );

            return;
        }

        if (isRecording)
        {
            StopRecording();
        }
        else
        {
            StartRecording();
        }
    }

    // =========================================================
    // START RECORDING
    // =========================================================

    private void StartRecording()
    {
        if (!modelLoaded)
        {
            Debug.LogError(
                "Cannot start recording because Whisper is not loaded."
            );

            return;
        }

        if (Microphone.devices.Length == 0)
        {
            Debug.LogError(
                "No microphone detected."
            );

            return;
        }

        microphoneDevice = Microphone.devices[0];

        Debug.Log(
            "Starting microphone: " +
            microphoneDevice
        );

        microphoneClip = Microphone.Start(
            microphoneDevice,
            false,
            maxRecordingLengthSeconds,
            sampleRate
        );

        if (microphoneClip == null)
        {
            Debug.LogError(
                "Failed to start microphone."
            );

            return;
        }

        Debug.Log(
            "Microphone started. Frequency: " +
            microphoneClip.frequency +
            " Hz, Channels: " +
            microphoneClip.channels
        );

        isRecording = true;

        // Change button color when recording starts
        if (recordButtonBackground != null)
        {
            recordButtonBackground.color =
                recordingColor;
        }

        if (transcriptText != null)
            transcriptText.text = "";

        Debug.Log(
            "RECORDING STARTED."
        );
    }

    // =========================================================
    // STOP RECORDING
    // =========================================================

    private void StopRecording()
    {
        if (!isRecording)
            return;

        isRecording = false;

        // Change button color back when recording stops
        if (recordButtonBackground != null)
        {
            recordButtonBackground.color =
                normalColor;
        }

        Debug.Log(
            "Stopping microphone..."
        );

        int recordedPosition =
            Microphone.GetPosition(
                microphoneDevice
            );

        Debug.Log(
            "Recorded microphone position: " +
            recordedPosition
        );

        if (Microphone.IsRecording(microphoneDevice))
        {
            Microphone.End(
                microphoneDevice
            );
        }

        Debug.Log(
            "Microphone stopped."
        );

        if (recordedPosition <= 0)
        {
            Debug.LogWarning(
                "No audio was recorded."
            );

            return;
        }

        ProcessRecording(
            recordedPosition
        );
    }

    // =========================================================
    // PROCESS COMPLETE RECORDING
    // =========================================================

    private void ProcessRecording(
        int recordedPosition
    )
    {
        if (microphoneClip == null)
        {
            Debug.LogError(
                "Microphone clip is null."
            );

            return;
        }

        int channels =
            microphoneClip.channels;

        int totalSamples =
            recordedPosition * channels;

        Debug.Log(
            "Total recorded samples: " +
            totalSamples
        );

        if (totalSamples <= 0)
        {
            Debug.LogWarning(
                "No samples available."
            );

            return;
        }

        float[] samples =
            new float[totalSamples];

        microphoneClip.GetData(
            samples,
            0
        );

        Debug.Log(
            "Audio data extracted: " +
            samples.Length +
            " samples"
        );

        // Convert stereo → mono if necessary.
        float[] monoSamples =
            ConvertToMono(
                samples,
                channels
            );

        Debug.Log(
            "Mono audio samples: " +
            monoSamples.Length
        );

        StartTranscription(
            monoSamples
        );
    }

    // =========================================================
    // TRANSCRIPTION
    // =========================================================

    private void StartTranscription(
        float[] samples
    )
    {
        if (transcriptionInProgress)
        {
            Debug.LogWarning(
                "A transcription is already running."
            );

            return;
        }

        transcriptionInProgress = true;

        Debug.Log(
            "Sending complete recording to Whisper..."
        );

        Debug.Log(
            "Samples sent to Whisper: " +
            samples.Length
        );

        transcriptionTask = Task.Run(
            () =>
            {
                try
                {
                    return WhisperNative.Transcribe(
                        samples
                    );
                }
                catch (Exception exception)
                {
                    Debug.LogError(
                        "Whisper transcription exception: " +
                        exception
                    );

                    return string.Empty;
                }
            }
        );
    }

    // =========================================================
    // CHECK TRANSCRIPTION
    // =========================================================

    private void Update()
    {
        CheckTranscriptionTask();
    }

    private void CheckTranscriptionTask()
    {
        if (transcriptionTask == null)
            return;

        if (!transcriptionTask.IsCompleted)
            return;

        if (transcriptionTask.IsFaulted)
        {
            Debug.LogError(
                "Whisper transcription task failed: " +
                transcriptionTask.Exception
            );

            transcriptionInProgress = false;
            transcriptionTask = null;

            return;
        }

        string result =
            transcriptionTask.Result;

        transcriptionInProgress = false;
        transcriptionTask = null;

        if (string.IsNullOrWhiteSpace(result))
        {
            Debug.LogWarning(
                "Whisper returned an empty transcription."
            );

            return;
        }

        result = result.Trim();

        Debug.Log(
            "FINAL WHISPER RESULT: " +
            result
        );

        if (transcriptText != null)
        {
            transcriptText.text = result;
        }
    }

    // =========================================================
    // AUDIO CONVERSION
    // =========================================================

    private float[] ConvertToMono(
        float[] samples,
        int channels
    )
    {
        if (channels <= 1)
            return samples;

        int monoLength =
            samples.Length / channels;

        float[] mono =
            new float[monoLength];

        for (int i = 0; i < monoLength; i++)
        {
            float sum = 0f;

            for (int channel = 0;
                 channel < channels;
                 channel++)
            {
                sum += samples[
                    i * channels + channel
                ];
            }

            mono[i] =
                sum / channels;
        }

        return mono;
    }

    // =========================================================
    // CLEANUP
    // =========================================================

    private void OnDestroy()
    {
        if (recordButton != null)
        {
            recordButton.onClick.RemoveListener(
                ToggleRecording
            );
        }

        if (!string.IsNullOrEmpty(microphoneDevice))
        {
            if (Microphone.IsRecording(
                microphoneDevice))
            {
                Microphone.End(
                    microphoneDevice
                );
            }
        }

        WhisperNative.Whisper_UnloadModel();
    }
}