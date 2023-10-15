namespace TestFramework.Code
{
    namespace Test
    {
        public class TestError
        {
            public string TestName { get; }
            public string TestCaseName { get; }
            public string TestStepName { get; }
            public string ErrorID { get; }
            public HashSet<(string, object)> ExtraFieldsList { get; }

            public TestError(string testName, string testCaseName, string testStepName, string errorID)
            {
                TestName = testName;
                TestCaseName = testCaseName;
                TestStepName = testStepName;
                ErrorID = errorID;
                ExtraFieldsList = new();
            }

            public TestError AddExtraField((string, object) extraField)
            {
                ExtraFieldsList.Add(extraField);
                return this;
            }

            public override bool Equals(object? obj)
            {
                if(obj == null || !(obj is TestError)) return false;

                var castedObj = (TestError)obj;

                if (TestName != castedObj.TestName) return false;
                if (TestCaseName != castedObj.TestCaseName) return false;
                if (ErrorID != castedObj.ErrorID) return false;

                foreach ((string, object) extraField in ExtraFieldsList)
                {
                    if(!castedObj.ExtraFieldsList.Contains(extraField)) return false;
                }

                return true;
            }

            public override int GetHashCode()
            {
                int hashCode = 17;

                hashCode = hashCode * 23 + (TestName?.GetHashCode() ?? 0);
                hashCode = hashCode * 23 + (TestCaseName?.GetHashCode() ?? 0);
                hashCode = hashCode * 23 + (ErrorID?.GetHashCode() ?? 0);

                foreach ((string, object) extraField in ExtraFieldsList)
                {
                    hashCode = hashCode * 23 + extraField.GetHashCode();
                }

                return hashCode;
            }

            public override string ToString()
            {
                string resultString = ErrorID + ";" + TestName + ";" + TestCaseName + ";" + TestStepName;

                if (ExtraFieldsList.Count > 0)
                {
                    resultString += " > ";
                    foreach ((string, object) extraField in ExtraFieldsList)
                    {
                        resultString += extraField.Item1 + ": " + extraField.Item2 + ";";
                    }
                    resultString = resultString[..^1];
                }

                return resultString;
            }
        }
    }
}