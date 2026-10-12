
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AutoScrollToBottom : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;

    public void ScrollToBottom()
    {
        StartCoroutine(ScrollAfterLayoutUpdate());
    }

    private IEnumerator ScrollAfterLayoutUpdate()
    {
        // Wait for the new message to be added.
        yield return null;

        // Update the UI layout before scrolling.
        Canvas.ForceUpdateCanvases();

        // 0 = bottom, 1 = top.
        scrollRect.verticalNormalizedPosition = 0f;

        Canvas.ForceUpdateCanvases();
    }
}
