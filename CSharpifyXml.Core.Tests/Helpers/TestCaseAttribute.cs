using System.Diagnostics;
using System.Reflection;
using Newtonsoft.Json;
using Xunit.Sdk;

namespace CSharpifyXml.Core.Tests.Helpers;

/// <summary>
/// Loads test cases from a local folder.
/// The test cases are expected to be in the form of XML and json files
/// </summary>
public abstract class TestCaseAttribute<T>(string testCasesFolderPath, string resultFileExtension) : DataAttribute
{
    protected const string TestFileExtension = ".xml";

    /// <summary>
    /// Returns the test cases for the test method, by finding all test file xml files in
    /// the test cases folder and their corresponding expected json files. 
    /// </summary>
    /// <param name="testMethod"></param>
    /// <returns>The test cases as <see cref="TestSample"/></returns>
    /// <exception cref="NotImplementedException"></exception>
    private IEnumerable<TestSample> GetTestCases()
    {
        var testFiles = GetTestFiles(testCasesFolderPath);
        foreach (var testFile in testFiles)
        {
            var expectedMap = GetExpectedResult<T>(testFile);

            yield return new TestSample 
            (
                testCaseName:Path.GetFileNameWithoutExtension(testFile),
                xmlFilepath:testFile,
                expectedResult:expectedMap
            );
        }
    }

    // ReSharper disable once ReturnTypeCanBeEnumerable.Local, because of interfering design rules
    private static string[] GetTestFiles(string testCasesFolderPath)
    {
        var testFiles = Directory.GetFiles(testCasesFolderPath, $"*{TestFileExtension}");
        return testFiles;
    }

    private T GetExpectedResult<T>(string testFile)
    {
        var expectedFile = testFile.Replace(TestFileExtension, resultFileExtension);
        var expectedMap = JsonConvert.DeserializeObject<T>(File.ReadAllText(expectedFile));
        Debug.Assert(expectedMap != null, nameof(expectedMap) + " != null");
        return expectedMap;
    }

    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        return GetTestCases().Select(couple => new object[] { couple });
    }
}