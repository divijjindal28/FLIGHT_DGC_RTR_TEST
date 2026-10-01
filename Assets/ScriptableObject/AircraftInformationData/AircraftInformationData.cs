using UnityEngine;

[CreateAssetMenu(
    fileName = "AircraftInformation",
    menuName = "DGCA/RTR Part 2/Aircraft Information"
)]
public class AircraftInformationData : ScriptableObject
{
    [Header("Aircraft")]
    public string callSign;
    public string type;
    public string operatorName;
    public string registration;

    [Header("Flight")]
    public string departure;
    public string destination;
    public string route;
    public string level;
    public string flightRules;

    [Header("Operational")]
    public int squawk;
    public string otherInfo;
}