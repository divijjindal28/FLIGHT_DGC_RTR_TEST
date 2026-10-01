using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RTRDataSetUIController : MonoBehaviour
{
    [Header("RTR Data")]
    [SerializeField] private RTRDataSet rtrDataSet;

    // Public access for other systems
    public RTRDataSet DataSet => rtrDataSet;


    // =========================================================
    // TEST DEFINITION
    // =========================================================

    [Header("Test Definition")]
    [SerializeField] private TMP_Text testHeadingText;


    // =========================================================
    // QUESTION PANEL
    // =========================================================

    [Header("Question Panel")]
    [SerializeField] private GameObject questionPanel;

    [SerializeField] private TMP_Text questionNumberText;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text questionDescriptionText;


    // =========================================================
    // TEST MAP
    // =========================================================

    [Header("Test Map")]
    [SerializeField] private Image testImage;


    // =========================================================
    // ANSWER PANEL
    // =========================================================

    [Header("Answer Panel")]
    [SerializeField] private GameObject answerPanel;
    [SerializeField] private TMP_Text answerText;


    // =========================================================
    // AIRCRAFT INFORMATION
    // =========================================================

    [Header("Aircraft Information")]
    [SerializeField] private TMP_Text callSignText;
    [SerializeField] private TMP_Text typeText;
    [SerializeField] private TMP_Text operatorText;
    [SerializeField] private TMP_Text registrationText;

    [SerializeField] private TMP_Text departureText;
    [SerializeField] private TMP_Text destinationText;
    [SerializeField] private TMP_Text routeText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text flightRulesText;

    [SerializeField] private TMP_Text squawkText;
    [SerializeField] private TMP_Text aircraftOtherInfoText;


    // =========================================================
    // BASE FREQUENCY
    // =========================================================

    [Header("Base Frequency")]
    [SerializeField] private TMP_Text baseSMCText;
    [SerializeField] private TMP_Text baseTowerText;
    [SerializeField] private TMP_Text baseApproachText;
    [SerializeField] private TMP_Text baseAreaText;
    [SerializeField] private TMP_Text baseOtherText;


    // =========================================================
    // DESTINATION FREQUENCY
    // =========================================================

    [Header("Destination Frequency")]
    [SerializeField] private TMP_Text destinationSMCText;
    [SerializeField] private TMP_Text destinationTowerText;
    [SerializeField] private TMP_Text destinationApproachText;
    [SerializeField] private TMP_Text destinationAreaText;
    [SerializeField] private TMP_Text destinationOtherText;


    // =========================================================
    // METAR
    // =========================================================

    [Header("METAR Information")]
    [SerializeField] private TMP_Text metarStationText;
    [SerializeField] private TMP_Text metarTimeText;

    [SerializeField] private TMP_Text metarWindText;

    [SerializeField] private TMP_Text metarVisibilityText;
    [SerializeField] private TMP_Text metarTemperatureText;
    [SerializeField] private TMP_Text metarDewpointText;
    [SerializeField] private TMP_Text metarQNHText;

    [SerializeField] private TMP_Text metarWeatherText;
    [SerializeField] private TMP_Text metarTrendText;
    [SerializeField] private TMP_Text metarOtherInfoText;


    // =========================================================
    // UNITY
    // =========================================================

    private void Start()
    {
        PopulateUI();

        // Start with question panel visible
        ShowQuestion();
    }


    // =========================================================
    // MAIN POPULATION FUNCTION
    // =========================================================

    public void PopulateUI()
    {
        if (rtrDataSet == null)
        {
            Debug.LogError(
                "RTRDataSetUIController: RTR Data Set is not assigned."
            );

            return;
        }

        PopulateTestDefinition();
        PopulateAircraftInformation();
        PopulateBaseFrequency();
        PopulateDestinationFrequency();
        PopulateMETAR();

        Debug.Log(
            "RTRDataSetUIController: UI populated successfully."
        );
    }


    // =========================================================
    // TEST DEFINITION
    // =========================================================

    private void PopulateTestDefinition()
    {
        SetText(
            testHeadingText,
            rtrDataSet.testHeading
        );
    }


    // =========================================================
    // QUESTION DISPLAY
    // =========================================================

    public void DisplayQuestion(
        PhaseElement phaseElement,
        QuestionData question,
        int questionNumber,
        int totalQuestions
    )
    {
        if (phaseElement == null)
        {
            Debug.LogError(
                "RTRDataSetUIController: PhaseElement is null."
            );

            return;
        }

        if (question == null)
        {
            Debug.LogError(
                "RTRDataSetUIController: QuestionData is null."
            );

            return;
        }


        // -----------------------------------------------------
        // MAP
        // -----------------------------------------------------

        if (testImage != null)
        {
            testImage.sprite = phaseElement.map;

            testImage.gameObject.SetActive(
                phaseElement.map != null
            );
        }


        // -----------------------------------------------------
        // QUESTION NUMBER
        // -----------------------------------------------------

        SetText(
            questionNumberText,
            $"{questionNumber}"
        );


        // -----------------------------------------------------
        // QUESTION
        // -----------------------------------------------------

        SetText(
            questionText,
            question.question
        );


        // -----------------------------------------------------
        // DESCRIPTION / SUBTEXT
        // -----------------------------------------------------

        SetText(
            questionDescriptionText,
            question.description
        );


        // -----------------------------------------------------
        // ANSWER
        // -----------------------------------------------------

        SetText(
            answerText,
            question.answer
        );


        // -----------------------------------------------------
        // SHOW QUESTION
        // -----------------------------------------------------

        ShowQuestion();
    }


    // =========================================================
    // SHOW ANSWER
    // =========================================================

    public void ShowAnswer()
    {
        // Hide question panel
        if (questionPanel != null)
            questionPanel.SetActive(false);

        // Show answer panel
        if (answerPanel != null)
            answerPanel.SetActive(true);
    }


    // =========================================================
    // SHOW QUESTION
    // =========================================================

    public void ShowQuestion()
    {
        // Show question panel
        if (questionPanel != null)
            questionPanel.SetActive(true);

        // Hide answer panel
        if (answerPanel != null)
            answerPanel.SetActive(false);
    }


    // =========================================================
    // AIRCRAFT INFORMATION
    // =========================================================

    private void PopulateAircraftInformation()
    {
        AircraftInformationData data =
            rtrDataSet.aircraftInformation;

        if (data == null)
        {
            Debug.LogWarning(
                "RTRDataSetUIController: Aircraft Information is not assigned."
            );

            return;
        }

        SetText(callSignText, data.callSign);
        SetText(typeText, data.type);
        SetText(operatorText, data.operatorName);
        SetText(registrationText, data.registration);

        SetText(departureText, data.departure);
        SetText(destinationText, data.destination);
        SetText(routeText, data.route);
        SetText(levelText, data.level);
        SetText(flightRulesText, data.flightRules);

        SetText(squawkText, data.squawk.ToString());
        SetText(aircraftOtherInfoText, data.otherInfo);
    }


    // =========================================================
    // BASE FREQUENCY
    // =========================================================

    private void PopulateBaseFrequency()
    {
        AircraftFrequencyData data =
            rtrDataSet.baseFrequency;

        if (data == null)
        {
            Debug.LogWarning(
                "RTRDataSetUIController: Base Frequency is not assigned."
            );

            return;
        }

        SetText(baseSMCText, data.smc.ToString("0.0"));
        SetText(baseTowerText, data.tower.ToString("0.00"));
        SetText(baseApproachText, data.approach.ToString("0.0"));
        SetText(baseAreaText, data.area.ToString("0.0"));
        SetText(baseOtherText, data.other.ToString("0.0"));
    }


    // =========================================================
    // DESTINATION FREQUENCY
    // =========================================================

    private void PopulateDestinationFrequency()
    {
        AircraftFrequencyData data =
            rtrDataSet.destinationFrequency;

        if (data == null)
        {
            Debug.LogWarning(
                "RTRDataSetUIController: Destination Frequency is not assigned."
            );

            return;
        }

        SetText(
            destinationSMCText,
            data.smc.ToString("0.0")
        );

        SetText(
            destinationTowerText,
            data.tower.ToString("0.00")
        );

        SetText(
            destinationApproachText,
            data.approach.ToString("0.0")
        );

        SetText(
            destinationAreaText,
            data.area.ToString("0.0")
        );

        SetText(
            destinationOtherText,
            data.other.ToString("0.0")
        );
    }


    // =========================================================
    // METAR
    // =========================================================

    private void PopulateMETAR()
    {
        METARInfoData data =
            rtrDataSet.metarInformation;

        if (data == null)
        {
            Debug.LogWarning(
                "RTRDataSetUIController: METAR Information is not assigned."
            );

            return;
        }

        SetText(metarStationText, data.station);
        SetText(metarTimeText, data.time);

        SetText(metarWindText, data.wind);

        SetText(metarVisibilityText, data.visibility);

        SetText(
            metarTemperatureText,
            data.temperature.ToString("0")
        );

        SetText(
            metarDewpointText,
            data.dewpoint.ToString("0")
        );

        SetText(
            metarQNHText,
            data.qnh.ToString("0")
        );

        SetText(metarWeatherText, data.weather);
        SetText(metarTrendText, data.trend);
        SetText(metarOtherInfoText, data.otherInfo);
    }


    // =========================================================
    // HELPER
    // =========================================================

    private void SetText(
        TMP_Text textField,
        string value
    )
    {
        if (textField == null)
            return;

        textField.text = value ?? string.Empty;
    }
}