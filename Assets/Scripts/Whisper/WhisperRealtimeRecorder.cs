using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public enum WhisperModel
{
    BaseEnglish,
    SmallEnglish,
    MediumEnglish
}

public enum SpeechRecognitionMode
{
    OfflineWhisper,
    OnlineElevenLabs
}

public class WhisperRealtimeRecorder : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button recordButton;
    [SerializeField] private TMP_Text transcriptText;
    [SerializeField] private TMP_InputField transcriptInputField;
    [SerializeField] private Button sendButton;

    [Header("Whisper Model")]
    [SerializeField]
    private WhisperModel whisperModel =
        WhisperModel.BaseEnglish;

    [Header("Speech Recognition Mode")]
    [SerializeField]
    private SpeechRecognitionMode recognitionMode =
        SpeechRecognitionMode.OfflineWhisper;

    [Header("ElevenLabs (Online Mode)")]
    [Tooltip("Stored in the scene if entered. Do not commit this key or ship it in a public client build.")]
    [SerializeField] private string elevenLabsApiKey;
    [SerializeField] private string elevenLabsModelId = "scribe_v2";
    [SerializeField] private string elevenLabsLanguageCode = "en";
    [SerializeField] private RTRApiCostTracker apiCostTracker;

    private const double ElevenLabsUsdPerAudioHour = 0.22;

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
    private Coroutine onlineTranscriptionCoroutine;
    private UnityWebRequest activeRequest;

    private UIProceduralRoundedRect recordButtonBackground;

    private void Start()
    {
        if (recognitionMode == SpeechRecognitionMode.OfflineWhisper)
            LoadWhisper();

        SetupButton();

        if (transcriptText != null)
            transcriptText.text = "";
    }

    // =========================================================
    // WHISPER
    // =========================================================

    private void LoadWhisper()
    {
        string modelFileName;

        switch (whisperModel)
        {
            case WhisperModel.BaseEnglish:
                modelFileName = "ggml-base.en.bin";
                break;

            case WhisperModel.SmallEnglish:
                modelFileName = "ggml-small.en.bin";
                break;

            case WhisperModel.MediumEnglish:
                modelFileName = "ggml-medium.en.bin";
                break;

            default:
                Debug.LogError(
                    "WhisperRealtimeRecorder: " +
                    "Unknown Whisper model selected."
                );
                return;
        }

        string modelPath = Path.Combine(
            Application.streamingAssetsPath,
            "Whisper",
            modelFileName
        );

        Debug.Log(
            "Selected Whisper model: " +
            whisperModel
        );

        Debug.Log(
            "Whisper model path:\n" +
            modelPath
        );

        if (!File.Exists(modelPath))
        {
            Debug.LogError(
                "Whisper model not found:\n" +
                modelPath
            );

            return;
        }

        Debug.Log(
            "Whisper model found:\n" +
            modelFileName
        );

        modelLoaded = WhisperNative.Whisper_LoadModel(
            modelPath
        );

        if (modelLoaded)
        {
            Debug.Log(
                "SUCCESS: Whisper model loaded.\n" +
                "Model: " +
                whisperModel
            );
        }
        else
        {
            Debug.LogError(
                "FAILED: Whisper model could not be loaded.\n" +
                "Model: " +
                whisperModel
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
        if (recognitionMode == SpeechRecognitionMode.OfflineWhisper && !modelLoaded)
        {
            Debug.LogError(
                "Cannot start recording because Whisper is not loaded."
            );

            return;
        }

        if (recognitionMode == SpeechRecognitionMode.OnlineElevenLabs &&
            string.IsNullOrWhiteSpace(elevenLabsApiKey))
        {
            Debug.LogError(
                "Cannot start recording because the ElevenLabs API key is empty. " +
                "Set it in the WhisperRealtimeRecorder Inspector."
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

        if (recognitionMode == SpeechRecognitionMode.OnlineElevenLabs)
        {
            onlineTranscriptionCoroutine = StartCoroutine(
                TranscribeWithElevenLabs(monoSamples, microphoneClip.frequency)
            );
        }
        else
        {
            StartWhisperTranscription(monoSamples);
        }
    }

    // =========================================================
    // TRANSCRIPTION
    // =========================================================

    private void StartWhisperTranscription(
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

        HandleTranscriptionResult(result, "Whisper");
    }

    private IEnumerator TranscribeWithElevenLabs(float[] samples, int audioSampleRate)
    {
        transcriptionInProgress = true;

        if (samples == null || samples.Length == 0 || audioSampleRate <= 0)
        {
            transcriptionInProgress = false;
            onlineTranscriptionCoroutine = null;
            Debug.LogError("ElevenLabs transcription received invalid audio data.");
            yield break;
        }

        byte[] wavData = EncodeMonoPcm16Wav(samples, audioSampleRate);
        List<IMultipartFormSection> form = new List<IMultipartFormSection>
        {
            new MultipartFormFileSection("file", wavData, "recording.wav", "audio/wav"),
            new MultipartFormDataSection("model_id", elevenLabsModelId),
            new MultipartFormDataSection("tag_audio_events", "false"),
            new MultipartFormDataSection(
                "transcript_edit",
                "For aviation radiotelephony, write spoken numbers as English words rather than numeric digits. " +
                "Preserve aviation phraseology exactly. For example, write 'heading zero niner zero', not 'heading 090'. " +
                "Do not convert spoken digit sequences into numeric digits. Do not change the meaning or add words."
            )
        };

        if (!string.IsNullOrWhiteSpace(elevenLabsLanguageCode))
        {
            form.Add(new MultipartFormDataSection(
                "language_code",
                elevenLabsLanguageCode.Trim()
            ));
        }

        string result = string.Empty;
        bool requestSucceeded = false;
        using (UnityWebRequest request = UnityWebRequest.Post(
                   "https://api.elevenlabs.io/v1/speech-to-text",
                   form))
        {
            activeRequest = request;
            request.SetRequestHeader("xi-api-key", elevenLabsApiKey.Trim());
            request.timeout = 120;

            Debug.Log(
                "Sending recorded audio to ElevenLabs (" +
                elevenLabsModelId + ", " + audioSampleRate + " Hz)."
            );

            yield return request.SendWebRequest();
            activeRequest = null;

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(
                    "ElevenLabs transcription failed (HTTP " +
                    request.responseCode + "): " +
                    request.error + "\n" +
                    request.downloadHandler.text
                );
            }
            else
            {
                if (apiCostTracker != null)
                {
                    double audioSeconds =
                        samples.Length / (double)audioSampleRate;

                    apiCostTracker.RecordElevenLabsAudioCost(
                        audioSeconds,
                        ElevenLabsUsdPerAudioHour
                    );
                }
                try
                {
                    ElevenLabsTranscriptResponse response =
                        JsonUtility.FromJson<ElevenLabsTranscriptResponse>(
                            request.downloadHandler.text
                        );

                    if (response != null)
                    {
                        // Use the edited transcript when ElevenLabs returns one.
                        // Fall back to the original transcript if editing is unavailable.
                        result = response.text;

                        if (response.edited_transcript != null &&
                            response.edited_transcript.kind == "transcript" &&
                            !string.IsNullOrWhiteSpace(response.edited_transcript.text))
                        {
                            result = response.edited_transcript.text;
                        }

                        requestSucceeded = true;
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogError(
                        "Could not parse ElevenLabs transcription response: " +
                        exception.Message + "\n" +
                        request.downloadHandler.text
                    );
                }
            }
        }

        transcriptionInProgress = false;
        onlineTranscriptionCoroutine = null;

        if (requestSucceeded)
            HandleTranscriptionResult(result, "ElevenLabs");
    }

    private void HandleTranscriptionResult(string result, string providerName)
    {
        if (string.IsNullOrWhiteSpace(result))
        {
            Debug.LogWarning(providerName + " returned an empty transcription.");
            return;
        }

        result = result.Trim();
        Debug.Log("FINAL " + providerName.ToUpperInvariant() + " RESULT: " + result);

        if (transcriptText != null)
            transcriptText.text = result;

        if (transcriptInputField != null)
            transcriptInputField.text = result;

        if (sendButton != null)
            sendButton.onClick.Invoke();
    }

    [Serializable]
    private class ElevenLabsTranscriptResponse
    {
        public string text;
        public EditedTranscript edited_transcript;
    }

    [Serializable]
    private class EditedTranscript
    {
        public string kind;
        public string text;
        public string error_type;
        public string message;
    }

    private static byte[] EncodeMonoPcm16Wav(float[] samples, int audioSampleRate)
    {
        const int headerSize = 44;
        const short bitsPerSample = 16;
        const short channels = 1;
        int dataSize = samples.Length * sizeof(short);
        byte[] wav = new byte[headerSize + dataSize];

        using (MemoryStream stream = new MemoryStream(wav))
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + dataSize);
            writer.Write(Encoding.ASCII.GetBytes("WAVE"));
            writer.Write(Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((short)1);
            writer.Write(channels);
            writer.Write(audioSampleRate);
            writer.Write(audioSampleRate * channels * bitsPerSample / 8);
            writer.Write((short)(channels * bitsPerSample / 8));
            writer.Write(bitsPerSample);
            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(dataSize);

            for (int i = 0; i < samples.Length; i++)
            {
                float clampedSample = Mathf.Clamp(samples[i], -1f, 1f);
                short pcmSample = (short)(clampedSample < 0f
                    ? clampedSample * 32768f
                    : clampedSample * 32767f);
                writer.Write(pcmSample);
            }
        }

        return wav;
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

            for (
                int channel = 0;
                channel < channels;
                channel++
            )
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

        if (activeRequest != null)
            activeRequest.Abort();

        if (onlineTranscriptionCoroutine != null)
            StopCoroutine(onlineTranscriptionCoroutine);

        if (modelLoaded)
            WhisperNative.Whisper_UnloadModel();
    }
}
