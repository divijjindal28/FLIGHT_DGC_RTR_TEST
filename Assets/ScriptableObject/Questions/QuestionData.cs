using UnityEngine;


public enum ScenarioType
{
    PushbackStartUp,
    ATCClearance,
    Taxi,
    Departure,
    Traffic,
    Relay,
    PositionReport,
    Turbulence,
    Distress
}


[CreateAssetMenu(
    fileName = "QuestionData",
    menuName = "DGCA/RTR Part 2/Question Data"
)]


public class QuestionData : ScriptableObject
{
    [Header("Question")]
    [TextArea(2, 5)]
    public string question;

    [Header("Description")]
    [TextArea(3, 8)]
    public string description;

    [Header("Scenario")]
    public ScenarioType scenarioType;

}