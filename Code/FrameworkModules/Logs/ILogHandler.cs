namespace TestFramework.Code.FrameworkModules.Logs
{
    public interface ILogHandler
    {
        void LogFatalError(string message);
        void LogError(string message);
        void LogOK(string message);
        void LogWarning(string message);
        void LogDebug(string message);

        void LogTestFatalError(string message);
        void LogTestError(string message);
        void LogTestOK(string message);
        void LogTestWarning(string message);
        void LogTestDebug(string message);
    }
}