namespace TestFramework.Code.FrameworkModules
{
    public static class LogExtensions
    {
        public static void LogFatalError(string message)
        {
            LogManager.LogFatalError(message);
        }

        public static void LogError(string message)
        {
            LogManager.LogError(message);
        }

        public static void LogOK(string message)
        {
            LogManager.LogOK(message);
        }

        public static void LogWarning(string message)
        {
            LogManager.LogWarning(message);
        }

        public static void LogDebug(string message)
        {
            LogManager.LogDebug(message);
        }

        public static void LogTestFatalError(string message)
        {
            LogManager.LogTestFatalError(message);
        }

        public static void LogTestError(string message)
        {
            LogManager.LogTestError(message);
        }

        public static void LogTestOK(string message)
        {
            LogManager.LogTestOK(message);
        }

        public static void LogTestWarning(string message)
        {
            LogManager.LogTestWarning(message);
        }

        public static void LogTestDebug(string message)
        {
            LogManager.LogTestDebug(message);
        }
    }
}
