using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class RTRLocalTTS : MonoBehaviour
{
    [Header("TTS Server")]
    [SerializeField]
    private string ttsUrl = "http://127.0.0.1:5001/tts";

    [Header("Audio")]
    [SerializeField]
    private AudioSource audioSource;

    public void Speak(string text)
    {
        Debug.Log(
            "[TTS] Speak() called."
        );

        if (string.IsNullOrWhiteSpace(text))
        {
            Debug.LogWarning(
                "[TTS] Cannot speak empty text."
            );

            return;
        }

        Debug.Log(
            "[TTS] Text to speak: " +
            text
        );

        Debug.Log(
            "[TTS] Starting TTS coroutine."
        );

        StartCoroutine(
            RequestTTS(text)
        );
    }

    private IEnumerator RequestTTS(string text)
    {
        float requestStartTime =
            Time.realtimeSinceStartup;

        Debug.Log(
            "[TTS] ----------------------------------------"
        );

        Debug.Log(
            "[TTS] Request started."
        );

        Debug.Log(
            "[TTS] URL: " +
            ttsUrl
        );

        Debug.Log(
            "[TTS] Text: " +
            text
        );

        string json =
            JsonUtility.ToJson(
                new TTSRequest
                {
                    text = text
                }
            );

        Debug.Log(
            "[TTS] JSON created: " +
            json
        );

        byte[] bodyRaw =
            System.Text.Encoding.UTF8.GetBytes(
                json
            );

        Debug.Log(
            "[TTS] Request body created."
        );

        using UnityWebRequest request =
            new UnityWebRequest(
                ttsUrl,
                UnityWebRequest.kHttpVerbPOST
            );

        request.uploadHandler =
            new UploadHandlerRaw(bodyRaw);

        request.downloadHandler =
            new DownloadHandlerAudioClip(
                ttsUrl,
                AudioType.WAV
            );

        request.SetRequestHeader(
            "Content-Type",
            "application/json"
        );

        Debug.Log(
            "[TTS] UnityWebRequest configured."
        );

        float beforeSendTime =
            Time.realtimeSinceStartup;

        Debug.Log(
            "[TTS] Sending request to local TTS server..."
        );

        yield return request.SendWebRequest();

        float responseTime =
            Time.realtimeSinceStartup -
            beforeSendTime;

        Debug.Log(
            "[TTS] HTTP request completed in: " +
            responseTime.ToString("F2") +
            " seconds."
        );

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(
                "[TTS] TTS request FAILED."
            );

            Debug.LogError(
                "[TTS] Error: " +
                request.error
            );

            Debug.LogError(
                "[TTS] HTTP response code: " +
                request.responseCode
            );

            yield break;
        }

        Debug.Log(
            "[TTS] HTTP request successful."
        );

        Debug.Log(
            "[TTS] HTTP response code: " +
            request.responseCode
        );

        Debug.Log(
            "[TTS] Downloaded bytes: " +
            request.downloadedBytes
        );

        float beforeAudioClipTime =
            Time.realtimeSinceStartup;

        Debug.Log(
            "[TTS] Converting downloaded WAV to AudioClip..."
        );

        AudioClip clip =
            DownloadHandlerAudioClip.GetContent(
                request
            );

        float audioClipCreationTime =
            Time.realtimeSinceStartup -
            beforeAudioClipTime;

        Debug.Log(
            "[TTS] AudioClip conversion completed in: " +
            audioClipCreationTime.ToString("F2") +
            " seconds."
        );

        if (clip == null)
        {
            Debug.LogError(
                "[TTS] Received AudioClip is NULL."
            );

            yield break;
        }

        Debug.Log(
            "[TTS] AudioClip successfully created."
        );

        Debug.Log(
            "[TTS] AudioClip length: " +
            clip.length.ToString("F2") +
            " seconds."
        );

        Debug.Log(
            "[TTS] AudioClip frequency: " +
            clip.frequency +
            " Hz."
        );

        Debug.Log(
            "[TTS] AudioClip channels: " +
            clip.channels
        );

        if (audioSource == null)
        {
            Debug.LogError(
                "[TTS] AudioSource is NOT assigned."
            );

            yield break;
        }

        Debug.Log(
            "[TTS] AudioSource found."
        );

        audioSource.clip = clip;

        Debug.Log(
            "[TTS] AudioClip assigned to AudioSource."
        );

        float totalTime =
            Time.realtimeSinceStartup -
            requestStartTime;

        Debug.Log(
            "[TTS] TOTAL Unity TTS processing time: " +
            totalTime.ToString("F2") +
            " seconds."
        );

        Debug.Log(
            "[TTS] Starting audio playback."
        );

        audioSource.Play();

        Debug.Log(
            "[TTS] Audio playback started."
        );

        Debug.Log(
            "[TTS] ----------------------------------------"
        );
    }

    [System.Serializable]
    private class TTSRequest
    {
        public string text;
    }
}