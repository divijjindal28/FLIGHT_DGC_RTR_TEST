using UnityEngine;

[CreateAssetMenu(
    fileName = "RTRInteractionData",
    menuName = "DGCA/RTR Part 2/Interaction Data"
)]
public class RTRInteractionData : ScriptableObject
{
    [Header("ATC")]
    [TextArea(2, 5)]
    public string atcQuestion;

    public AudioClip atcAudio;

    [Header("Expected Pilot Response")]
    [TextArea(3, 8)]
    public string expectedAnswer;

    [Header("Wrong Answer Response")]
    [TextArea(2, 5)]
    public string atcWrongAnswerResponse;

    public AudioClip atcWrongAnswerAudio;
}