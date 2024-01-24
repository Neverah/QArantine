using System.Collections.Generic;

using TestFramework.Code.FrameworkUtils;

namespace TestFramework.Code
{
    namespace Test
    {
        public class TestError
        {
            private Dictionary<string, object> ExtraFields { get; }

            public string TestName { get; }
            public string TestCaseName { get; }
            public string TestStepName { get; }
            public string ErrorID { get; }
            public string ErrorCategory { get; }
            public string ErrorDescription { get; }

            public TestError(string testName, string testCaseName, string testStepName, string errorID, string errorCategory)
            {
                TestName = testName;
                TestCaseName = testCaseName;
                TestStepName = testStepName;
                ErrorID = errorID;
                ErrorCategory = errorCategory;
                ErrorDescription = "No description provided";
                ExtraFields = new();
            }

            public TestError(string testName, string testCaseName, string testStepName, string errorID, string errorCategory, string errorDescription)
            {
                TestName = testName;
                TestCaseName = testCaseName;
                TestStepName = testStepName;
                ErrorID = errorID;
                ErrorCategory = errorCategory;
                ErrorDescription = errorDescription;
                ExtraFields = new();
            }

            public object? GetExtraField(string extraFieldID)
            {
                ExtraFields.TryGetValue(extraFieldID, out object? foundValue);
                return foundValue;
            }

            public bool TryGetExtraField(string extraFieldID, out object value)
            {
                if(ExtraFields.TryGetValue(extraFieldID, out value!))
                {
                    return true;
                }
                else
                {
                    value = null!;
                    return false;
                }
            }

            public TestError AddExtraField(string extraFieldID, object extraFieldValue)
            {
                ExtraFields.Add(extraFieldID, extraFieldValue);
                return this;
            }

            public override bool Equals(object? obj)
            {
                if(obj == null || obj is not TestError) return false;

                var castedObj = (TestError)obj;

                if (TestName != castedObj.TestName) return false;
                if (TestCaseName != castedObj.TestCaseName) return false;
                if (ErrorID != castedObj.ErrorID) return false;

                if(!ExtraFields.DictionaryEquals(castedObj.ExtraFields)) return false;

                return true;
            }

            public override int GetHashCode()
            {
                int hashCode = 17;

                hashCode = hashCode * 23 + (TestName?.GetHashCode() ?? 0);
                hashCode = hashCode * 23 + (TestCaseName?.GetHashCode() ?? 0);
                hashCode = hashCode * 23 + (ErrorID?.GetHashCode() ?? 0);

                foreach (string extraFieldKey in ExtraFields.Keys)
                {
                    hashCode = hashCode * 23 + extraFieldKey.GetHashCode();
                    hashCode = hashCode * 23 + (ExtraFields.TryGetValue(extraFieldKey, out var extraFieldValue) ? extraFieldValue.GetHashCode() : 0);
                }

                return hashCode;
            }

            public override string ToString()
            {
                string resultString = ErrorID + ";" + TestName + ";" + TestCaseName + ";" + TestStepName;

                if (ExtraFields.Count > 0)
                {
                    resultString += " > ";
                    foreach (string extraFieldKey in ExtraFields.Keys)
                    {
                        resultString += extraFieldKey + ": " + (ExtraFields.TryGetValue(extraFieldKey, out var extraFieldValue) ? extraFieldValue.ToString() : "") + ";";
                    }
                    resultString = resultString[..^1];
                }

                return resultString;
            }
        }
    }
}