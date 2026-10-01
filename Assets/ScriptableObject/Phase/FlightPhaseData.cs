using System.Collections.Generic;
using UnityEngine;

public enum FlightPhase
{
    PreDeparture,
    EnRoute,
    Arrival
}

[CreateAssetMenu(
    fileName = "FlightPhaseData",
    menuName = "DGCA/RTR Part 2/Flight Phase Data"
)]
public class FlightPhaseData : ScriptableObject
{
    [Header("Flight Phase")]
    public FlightPhase flightPhase;

    [Header("Questions")]
    public List<QuestionData> questions;
}