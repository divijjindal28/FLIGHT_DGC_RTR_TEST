
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public class PhaseElement
{
    public FlightPhase flightPhase;
    public Sprite map;
    public FlightPhaseData flightPhaseData;
}


[CreateAssetMenu(
    fileName = "RTRDataSet",
    menuName = "DGCA/RTR Part 2/RTR Data Set"
)]
public class RTRDataSet : ScriptableObject
{
    [Header("Test Definition")]
    public string testHeading;

    [Header("Maps")]
    public List<PhaseElement> phase;

    [Header("Aircraft Information")]
    public AircraftInformationData aircraftInformation;

    [Header("Base Frequency")]
    public AircraftFrequencyData baseFrequency;

    [Header("Destination Frequency")]
    public AircraftFrequencyData destinationFrequency;

    [Header("METAR Information")]
    public METARInfoData metarInformation;
}
