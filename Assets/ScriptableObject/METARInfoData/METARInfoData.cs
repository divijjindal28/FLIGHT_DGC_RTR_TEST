using UnityEngine;

[CreateAssetMenu(
    fileName = "METARInfo",
    menuName = "DGCA/RTR Part 2/METAR Info"
)]
public class METARInfoData : ScriptableObject
{
    [Header("Station")]
    public string station;
    public string time;

    [Header("Wind")]
    public string wind;

    [Header("Weather")]
    public string visibility;
    public float temperature;
    public float dewpoint;
    public float qnh;

    public string weather;
    public string trend;
    public string otherInfo;
}