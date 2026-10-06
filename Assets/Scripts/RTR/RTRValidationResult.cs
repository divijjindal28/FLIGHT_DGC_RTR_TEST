using System;

[Serializable]
public class RTRValidationResult
{
    public int score;
    public bool correctness;
    public string severity;
    public string reason;
}