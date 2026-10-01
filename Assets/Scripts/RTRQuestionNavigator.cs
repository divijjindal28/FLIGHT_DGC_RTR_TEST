using UnityEngine;
using UnityEngine.UI;

public class RTRQuestionNavigator : MonoBehaviour
{
    [Header("UI Controller")]
    [SerializeField] private RTRDataSetUIController uiController;

    [Header("Navigation Buttons")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;

    private int currentPhaseIndex = 0;
    private int currentQuestionIndex = 0;

    private RTRDataSet DataSet
    {
        get
        {
            if (uiController == null)
                return null;

            return uiController.DataSet;
        }
    }


    // =========================================================
    // UNITY
    // =========================================================

    private void Start()
    {
        SetupButtons();
        StartTest();
    }


    // =========================================================
    // BUTTON SETUP
    // =========================================================

    private void SetupButtons()
    {
        if (nextButton != null)
            nextButton.onClick.AddListener(NextQuestion);

        if (previousButton != null)
            previousButton.onClick.AddListener(PreviousQuestion);
    }


    // =========================================================
    // START TEST
    // =========================================================

    private void StartTest()
    {
        if (uiController == null)
        {
            Debug.LogError(
                "RTRQuestionNavigator: UI Controller is not assigned."
            );

            return;
        }

        if (DataSet == null)
        {
            Debug.LogError(
                "RTRQuestionNavigator: RTR Data Set could not be obtained from UI Controller."
            );

            return;
        }

        if (DataSet.phase == null ||
            DataSet.phase.Count == 0)
        {
            Debug.LogError(
                "RTRQuestionNavigator: RTR Data Set contains no phases."
            );

            return;
        }

        currentPhaseIndex = 0;
        currentQuestionIndex = 0;

        DisplayCurrentQuestion();
    }


    // =========================================================
    // NEXT QUESTION
    // =========================================================

    public void NextQuestion()
    {
        if (!HasValidPhase())
            return;

        PhaseElement currentPhaseElement =
            DataSet.phase[currentPhaseIndex];

        FlightPhaseData currentPhaseData =
            currentPhaseElement.flightPhaseData;

        if (currentPhaseData == null)
        {
            Debug.LogError(
                "RTRQuestionNavigator: Current Phase Element has no FlightPhaseData."
            );

            return;
        }

        if (currentPhaseData.questions == null ||
            currentPhaseData.questions.Count == 0)
        {
            MoveToNextPhase();
            return;
        }


        // -----------------------------------------------------
        // Move to next question inside current phase
        // -----------------------------------------------------

        if (currentQuestionIndex <
            currentPhaseData.questions.Count - 1)
        {
            currentQuestionIndex++;

            DisplayCurrentQuestion();
            return;
        }


        // -----------------------------------------------------
        // Current phase finished
        // Move to next phase
        // -----------------------------------------------------

        MoveToNextPhase();
    }


    // =========================================================
    // PREVIOUS QUESTION
    // =========================================================

    public void PreviousQuestion()
    {
        if (!HasValidPhase())
            return;


        // -----------------------------------------------------
        // Move to previous question inside current phase
        // -----------------------------------------------------

        if (currentQuestionIndex > 0)
        {
            currentQuestionIndex--;

            DisplayCurrentQuestion();
            return;
        }


        // -----------------------------------------------------
        // First question of current phase
        // Move to previous phase
        // -----------------------------------------------------

        MoveToPreviousPhase();
    }


    // =========================================================
    // NEXT PHASE
    // =========================================================

    private void MoveToNextPhase()
    {
        if (currentPhaseIndex <
            DataSet.phase.Count - 1)
        {
            currentPhaseIndex++;
            currentQuestionIndex = 0;

            DisplayCurrentQuestion();

            return;
        }

        Debug.Log(
            "RTRQuestionNavigator: All phases and questions are complete."
        );
    }


    // =========================================================
    // PREVIOUS PHASE
    // =========================================================

    private void MoveToPreviousPhase()
    {
        if (currentPhaseIndex > 0)
        {
            currentPhaseIndex--;

            PhaseElement previousPhaseElement =
                DataSet.phase[currentPhaseIndex];

            if (previousPhaseElement.flightPhaseData == null)
            {
                Debug.LogError(
                    "RTRQuestionNavigator: Previous phase has no FlightPhaseData."
                );

                return;
            }

            FlightPhaseData previousPhaseData =
                previousPhaseElement.flightPhaseData;

            if (previousPhaseData.questions == null ||
                previousPhaseData.questions.Count == 0)
            {
                currentQuestionIndex = 0;

                DisplayCurrentQuestion();

                return;
            }


            // Go to last question of previous phase
            currentQuestionIndex =
                previousPhaseData.questions.Count - 1;

            DisplayCurrentQuestion();

            return;
        }

        Debug.Log(
            "RTRQuestionNavigator: Already at the first question."
        );
    }


    // =========================================================
    // DISPLAY CURRENT QUESTION
    // =========================================================

    private void DisplayCurrentQuestion()
    {
        if (!HasValidPhase())
            return;

        PhaseElement phaseElement =
            DataSet.phase[currentPhaseIndex];

        if (phaseElement.flightPhaseData == null)
        {
            Debug.LogError(
                "RTRQuestionNavigator: Phase Element has no FlightPhaseData."
            );

            return;
        }

        FlightPhaseData phaseData =
            phaseElement.flightPhaseData;

        if (phaseData.questions == null ||
            phaseData.questions.Count == 0)
        {
            Debug.LogWarning(
                "RTRQuestionNavigator: Current phase has no questions."
            );

            return;
        }

        if (currentQuestionIndex < 0 ||
            currentQuestionIndex >= phaseData.questions.Count)
        {
            Debug.LogError(
                "RTRQuestionNavigator: Question index is out of range."
            );

            return;
        }

        QuestionData question =
            phaseData.questions[currentQuestionIndex];

        if (question == null)
        {
            Debug.LogError(
                "RTRQuestionNavigator: Current QuestionData is null."
            );

            return;
        }


        // -----------------------------------------------------
        // GLOBAL QUESTION NUMBER
        // -----------------------------------------------------

        int globalQuestionNumber =
            GetGlobalQuestionNumber();


        // -----------------------------------------------------
        // TOTAL QUESTIONS
        // -----------------------------------------------------

        int totalQuestionCount =
            GetTotalQuestionCount();


        // -----------------------------------------------------
        // SEND CURRENT QUESTION TO UI
        // -----------------------------------------------------

        uiController.DisplayQuestion(
            phaseElement,
            question,
            globalQuestionNumber,
            totalQuestionCount
        );
    }


    // =========================================================
    // GLOBAL QUESTION NUMBER
    // =========================================================

    private int GetGlobalQuestionNumber()
    {
        int questionNumber = 0;


        // Count all questions in previous phases
        for (int i = 0; i < currentPhaseIndex; i++)
        {
            PhaseElement phaseElement =
                DataSet.phase[i];

            if (phaseElement == null ||
                phaseElement.flightPhaseData == null)
            {
                continue;
            }

            if (phaseElement.flightPhaseData.questions == null)
                continue;

            questionNumber +=
                phaseElement.flightPhaseData.questions.Count;
        }


        // Add current question index
        questionNumber += currentQuestionIndex + 1;

        return questionNumber;
    }


    // =========================================================
    // TOTAL QUESTIONS
    // =========================================================

    private int GetTotalQuestionCount()
    {
        int totalQuestions = 0;

        foreach (PhaseElement phaseElement in DataSet.phase)
        {
            if (phaseElement == null ||
                phaseElement.flightPhaseData == null)
            {
                continue;
            }

            if (phaseElement.flightPhaseData.questions == null)
                continue;

            totalQuestions +=
                phaseElement.flightPhaseData.questions.Count;
        }

        return totalQuestions;
    }


    // =========================================================
    // VALIDATION
    // =========================================================

    private bool HasValidPhase()
    {
        if (DataSet == null)
            return false;

        if (DataSet.phase == null ||
            DataSet.phase.Count == 0)
        {
            return false;
        }

        if (currentPhaseIndex < 0 ||
            currentPhaseIndex >= DataSet.phase.Count)
        {
            return false;
        }

        return true;
    }


    // =========================================================
    // CLEANUP
    // =========================================================

    private void OnDestroy()
    {
        if (nextButton != null)
            nextButton.onClick.RemoveListener(NextQuestion);

        if (previousButton != null)
            previousButton.onClick.RemoveListener(PreviousQuestion);
    }
}