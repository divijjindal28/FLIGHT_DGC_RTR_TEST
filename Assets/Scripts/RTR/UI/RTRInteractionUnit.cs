using TMPro;
using UnityEngine;

public class RTRInteractionUnit : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text headingText;
    [SerializeField] private TMP_Text interactionText;


    public void SetInteraction(
        string heading,
        string text
    )
    {
        if (headingText != null)
        {
            headingText.text = heading;
        }

        if (interactionText != null)
        {
            interactionText.text = text;
        }
    }
}