namespace blockProject.randomSrc;

public class Error
{
    public Error(string message)
    {
        Message = message;
    }

    public string Message { get; } // Używamy Message zamiast error
}