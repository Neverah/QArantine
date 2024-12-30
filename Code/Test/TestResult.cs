namespace QArantine.Code.Test
{
    public class TestResult
    (
        string testName,
        bool success,
        int testedTestCases,
        int foundErrorsAmmount,
        string completionTime,
        long completionTimeAsSeconds,
        string startTimestamp,
        string endTimestamp,
        string outputDirectoryPath
    ) : EventArgs
    {    
        public string TestName { get; set; } = testName;
        public bool Success { get; set; } = success;
        public int TestedTestCases { get; set; } = testedTestCases;
        public int FoundErrorsAmmount { get; set; } = foundErrorsAmmount;
        public string CompletionTime { get; set; } = completionTime;
        public long CompletionTimeAsSeconds { get; set; } = completionTimeAsSeconds;
        public string StartTimestamp { get; set; } = startTimestamp;
        public string EndTimestamp { get; set; } = endTimestamp;
        public string OutputDirectoryPath { get; set; } = outputDirectoryPath;
    }
}