using System.Threading.Tasks;

public interface IAnswerEvaluator
{
    Task<RTRValidationResult> Evaluate(
        QuestionData question,
        string studentAnswer
    );
}