using UnityEngine;

public enum FrequencyType
{
    Base,
    Destination
}

[CreateAssetMenu(
    fileName = "FrequencyData",
    menuName = "DGCA/RTR Part 2/Frequency Data"
)]
public class AircraftFrequencyData : ScriptableObject
{
    [Header("Frequency Type")]
    public FrequencyType frequencyType;

    [Header("Frequencies")]
    public float smc;
    public float tower;
    public float approach;
    public float area;
    public float other;
}