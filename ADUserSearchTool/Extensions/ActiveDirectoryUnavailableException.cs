namespace ADUserSearchTool.Exceptions
{
    public class ActiveDirectoryUnavailableException : Exception
    {
        public ActiveDirectoryUnavailableException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
