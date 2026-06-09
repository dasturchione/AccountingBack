namespace SharedKernel.Exceptions
{
    public class DbCommandException : Exception
    {
        public DbCommandException(Exception innerException)
            : base ("Database command execution failed.", innerException)
        {
        }
    }
}
