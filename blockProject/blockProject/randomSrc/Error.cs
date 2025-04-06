namespace blockProject.randomSrc
{
    public class Error
    {
        public string Message { get; }  // Używamy Message zamiast error

        public Error(string message)
        {
            Message = message;
        }
    }
}