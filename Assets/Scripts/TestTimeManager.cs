using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestTimeManager : MonoBehaviour
{
    [Header("Timer Display")]
    [SerializeField] private TMP_Text timerText;

    [Header("Test Duration")]
    [SerializeField] private int durationInMinutes = 30;

    [Header("Buttons")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resetButton;

    private float remainingTime;
    private bool isPaused = false;
    private bool timerFinished = false;


    // =========================================================
    // UNITY
    // =========================================================

    private void Start()
    {
        SetupButtons();
        ResetTimer();
    }

    private void Update()
    {
        if (timerFinished || isPaused)
            return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            timerFinished = true;

            UpdateTimerDisplay();
            QuitApplication();

            return;
        }

        UpdateTimerDisplay();
    }


    // =========================================================
    // BUTTON SETUP
    // =========================================================

    private void SetupButtons()
    {
        if (pauseButton != null)
            pauseButton.onClick.AddListener(TogglePause);

        if (resetButton != null)
            resetButton.onClick.AddListener(ResetTimer);
    }


    // =========================================================
    // PAUSE / RESUME
    // =========================================================

    public void TogglePause()
    {
        if (timerFinished)
            return;

        isPaused = !isPaused;
    }


    // =========================================================
    // RESET
    // =========================================================

    public void ResetTimer()
    {
        if (durationInMinutes < 0)
            durationInMinutes = 0;

        remainingTime = durationInMinutes * 60f;

        isPaused = false;
        timerFinished = false;

        UpdateTimerDisplay();
    }


    // =========================================================
    // TIMER DISPLAY
    // =========================================================

    private void UpdateTimerDisplay()
    {
        if (timerText == null)
            return;

        int totalSeconds = Mathf.CeilToInt(remainingTime);

        int hours = totalSeconds / 3600;

        int minutes = (totalSeconds % 3600) / 60;

        int seconds = totalSeconds % 60;

        timerText.text =
            $"{hours:00}:{minutes:00}:{seconds:00}";
    }


    // =========================================================
    // QUIT APPLICATION
    // =========================================================

    private void QuitApplication()
    {
        Debug.Log(
            "TestTimeManager: Test time has expired. Closing application."
        );

        Application.Quit();
    }


    // =========================================================
    // CLEANUP
    // =========================================================

    private void OnDestroy()
    {
        if (pauseButton != null)
            pauseButton.onClick.RemoveListener(TogglePause);

        if (resetButton != null)
            resetButton.onClick.RemoveListener(ResetTimer);
    }
}