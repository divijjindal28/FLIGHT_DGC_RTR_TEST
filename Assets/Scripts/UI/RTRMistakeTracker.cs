
using TMPro;
using UnityEngine;

public class RTRMistakeTracker : MonoBehaviour
{
    [Header("Mistake Settings")]
    [SerializeField, Min(0)]
    private int maxAllowedMistakes = 7;

    [Header("UI")]
    [SerializeField]
    private TMP_Text mistakesRemainingText;

    private int mistakesMade;
    private bool examEnded;

    public int MistakesMade => mistakesMade;

    public int MistakesRemaining =>
        Mathf.Max(0, maxAllowedMistakes - mistakesMade);

    public bool ExamEnded => examEnded;


    private void Start()
    {
        mistakesMade = 0;
        examEnded = false;

        UpdateMistakesRemainingUI();
    }

    public void RegisterMistake()
    {
        // Do not count additional mistakes after the exam ends.
        if (examEnded)
            return;

        mistakesMade++;

        UpdateMistakesRemainingUI();

        Debug.Log(
            $"RTR Mistake: {mistakesMade}/{maxAllowedMistakes}. " +
            $"Remaining: {MistakesRemaining}"
        );

        // The student is allowed the configured number of mistakes.
        // The next mistake ends the exam.
        if (mistakesMade > maxAllowedMistakes)
        {
            EndExam();
        }
    }

    private void UpdateMistakesRemainingUI()
    {
        if (mistakesRemainingText != null)
        {
            mistakesRemainingText.text =
                $"{MistakesRemaining} mistakes remaining";
        }
    }

    private void EndExam()
    {
        if (examEnded)
            return;

        examEnded = true;

        if (mistakesRemainingText != null)
        {
            mistakesRemainingText.text = "Exam failed";
        }

        Debug.LogError(
            "RTR Exam ended: maximum mistake allowance exceeded."
        );

#if UNITY_EDITOR
        // Stop Play Mode when testing in the Unity Editor.
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Quit the built application.
        Application.Quit();
#endif
    }

    public void ResetMistakes()
    {
        mistakesMade = 0;
        examEnded = false;

        UpdateMistakesRemainingUI();
    }
}
