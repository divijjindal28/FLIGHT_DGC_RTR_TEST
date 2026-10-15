
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RTRQuestionStatusTracker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RTRDataSetUIController uiController;
    [SerializeField] private GameObject questionSquarePrefab;
    [SerializeField] private Transform squareContainer;

    [Header("Colors")]
    [SerializeField] private Color unansweredColor = Color.white;
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color incorrectColor = Color.red;

    // Global question number -> square Image.
    private readonly Dictionary<int, Image> questionSquares =
        new Dictionary<int, Image>();

    private void Start()
    {
        CreateQuestionSquares();
    }

    private void CreateQuestionSquares()
    {
        questionSquares.Clear();

        if (uiController == null)
        {
            Debug.LogError(
                "[RTRQuestionStatusTracker] UI Controller is not assigned."
            );
            return;
        }

        if (questionSquarePrefab == null)
        {
            Debug.LogError(
                "[RTRQuestionStatusTracker] Question square prefab is not assigned."
            );
            return;
        }

        if (squareContainer == null)
        {
            Debug.LogError(
                "[RTRQuestionStatusTracker] Square container is not assigned."
            );
            return;
        }

        RTRDataSet dataSet = uiController.DataSet;

        if (dataSet == null || dataSet.phase == null)
        {
            Debug.LogError(
                "[RTRQuestionStatusTracker] RTR DataSet or phase list is missing."
            );
            return;
        }

        int globalQuestionNumber = 0;

        foreach (PhaseElement phaseElement in dataSet.phase)
        {
            if (phaseElement == null ||
                phaseElement.flightPhaseData == null ||
                phaseElement.flightPhaseData.questions == null)
            {
                Debug.LogWarning(
                    "[RTRQuestionStatusTracker] Skipping a phase with missing question data."
                );
                continue;
            }

            foreach (QuestionData question in
                     phaseElement.flightPhaseData.questions)
            {
                if (question == null)
                {
                    Debug.LogWarning(
                        "[RTRQuestionStatusTracker] Skipping a null question."
                    );
                    continue;
                }

                globalQuestionNumber++;

                GameObject squareObject = Instantiate(
                    questionSquarePrefab,
                    squareContainer
                );

                squareObject.name =
                    "QuestionStatus_" + globalQuestionNumber;

                Image squareImage =
                    squareObject.GetComponent<Image>();

                if (squareImage == null)
                {
                    Debug.LogError(
                        "[RTRQuestionStatusTracker] Prefab root needs an Image component."
                    );

                    Destroy(squareObject);
                    continue;
                }

                squareImage.color = unansweredColor;

                TMP_Text numberText =
                    squareObject.GetComponentInChildren<TMP_Text>(true);

                if (numberText != null)
                {
                    numberText.text =
                        globalQuestionNumber.ToString();
                }
                else
                {
                    Debug.LogWarning(
                        "[RTRQuestionStatusTracker] No TMP_Text found in square " +
                        globalQuestionNumber
                    );
                }

                questionSquares[globalQuestionNumber] =
                    squareImage;
            }
        }

        Debug.Log(
            "[RTRQuestionStatusTracker] Created " +
            globalQuestionNumber + " question squares."
        );
    }

    public void SetQuestionResult(
        int globalQuestionNumber,
        bool isCorrect)
    {
        if (!questionSquares.TryGetValue(
                globalQuestionNumber,
                out Image squareImage))
        {
            Debug.LogWarning(
                "[RTRQuestionStatusTracker] No square found for question " +
                globalQuestionNumber
            );
            return;
        }

        squareImage.color =
            isCorrect ? correctColor : incorrectColor;

        Debug.Log(
            "[RTRQuestionStatusTracker] Question " +
            globalQuestionNumber + " marked " +
            (isCorrect ? "CORRECT" : "INCORRECT") + "."
        );
    }
}
