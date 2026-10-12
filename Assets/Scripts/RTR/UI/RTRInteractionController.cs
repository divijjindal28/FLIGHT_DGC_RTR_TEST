using UnityEngine;

public class RTRInteractionController : MonoBehaviour
{
    [Header("Main Areas")]
    [SerializeField] private GameObject questionArea;
    [SerializeField] private GameObject answerArea;
    [SerializeField] private GameObject interactionArea;


    [Header("Interaction Prefabs")]
    [SerializeField] private RTRInteractionUnit pilotInteractionPrefab;
    [SerializeField] private RTRInteractionUnit atcInteractionPrefab;


    [Header("Interaction Parent")]
    [SerializeField] private Transform interactionContent;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;


    private void Start()
    {
        if (interactionArea != null)
        {
            interactionArea.SetActive(false);
        }
    }


    public void ShowPilotInteraction(
        string pilotText
    )
    {
        if (interactionArea != null)
        {
            interactionArea.SetActive(true);
        }

        //ClearInteractionArea();

        if (pilotInteractionPrefab == null)
        {
            Debug.LogError(
                "RTRInteractionController: " +
                "Pilot Interaction Prefab is not assigned."
            );

            return;
        }

        RTRInteractionUnit pilotInteraction =
            Instantiate(
                pilotInteractionPrefab,
                interactionContent
            );

        pilotInteraction.SetInteraction(
            "Pilot",
            pilotText
        );
    }


    public void ShowATCInteraction(
        string atcText
    )
    {
        if (interactionArea != null)
        {
            interactionArea.SetActive(true);
        }

        if (atcInteractionPrefab == null)
        {
            Debug.LogError(
                "RTRInteractionController: " +
                "ATC Interaction Prefab is not assigned."
            );

            return;
        }

        RTRInteractionUnit atcInteraction =
            Instantiate(
                atcInteractionPrefab,
                interactionContent
            );

        atcInteraction.SetInteraction(
            "ATC",
            atcText
        );
    }

    public void PlayInteractionAudio(AudioClip clip)
    {
        Debug.Log(
            $"[RTRInteractionController] Audio request received. " +
            $"Clip: {(clip != null ? clip.name : "NULL")}"
        );

        if (clip == null)
        {
            Debug.LogWarning(
                "[RTRInteractionController] No AudioClip assigned to this interaction."
            );
            return;
        }

        if (audioSource == null)
        {
            Debug.LogError(
                "[RTRInteractionController] AudioSource is not assigned in the Inspector."
            );
            return;
        }

        Debug.Log(
            $"[RTRInteractionController] Playing '{clip.name}'. " +
            $"AudioSource enabled: {audioSource.enabled}, " +
            $"GameObject active: {audioSource.gameObject.activeInHierarchy}, " +
            $"Volume: {audioSource.volume}, " +
            $"Mute: {audioSource.mute}, " +
            $"Clip length: {clip.length:F2}s"
        );

        audioSource.Stop();
        audioSource.PlayOneShot(clip);

        Debug.Log(
            $"[RTRInteractionController] PlayOneShot called for '{clip.name}'."
        );
    }

    public void ShowInteractionArea()
    {
        if (questionArea != null)
        {
            questionArea.SetActive(false);
        }

        if (answerArea != null)
        {
            answerArea.SetActive(false);
        }

        if (interactionArea != null)
        {
            interactionArea.SetActive(true);
        }
    }


    public void HideInteractionArea()
    {
        if (interactionArea != null)
        {
            interactionArea.SetActive(false);
        }
    }

    public void ClearInteractionArea()
    {
        if (interactionContent == null)
            return;

        for (
            int i = interactionContent.childCount - 1;
            i >= 0;
            i--
        )
        {
            Destroy(
                interactionContent.GetChild(i).gameObject
            );
        }
    }

    [ContextMenu("Test Interaction Accumulation")]
    private void TestInteractionAccumulation()
    {
        ShowATCInteraction("ATC: Akasa Air one five three two, say again your request.");

        ShowPilotInteraction("Pilot: Vijaywada clearance Akasa Air one five three two.");

        ShowATCInteraction("ATC: Cleared to Bangalore.");

        ShowPilotInteraction("Pilot: Cleared to Bangalore, Akasa Air one five three two.");
    }
}