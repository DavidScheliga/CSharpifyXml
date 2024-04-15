using System.Diagnostics;
using System.Reflection;
using CSharpifyXml.Core.Mapping;
using Newtonsoft.Json;
using Xunit.Sdk;

namespace CSharpifyXml.Core.Tests.Helpers;

/// <summary>
/// Provides the main tests.
/// </summary>
/// <param name="testCasesFolderPath"></param>
public class MainTestCaseAttribute(string testCasesFolderPath) 
    : TestCaseAttribute<List<XmlClassDescriptor>>(testCasesFolderPath, ".ExpectedClasses.json");

/// <summary>
/// Provides the first step's result.
/// </summary>
/// <param name="testCasesFolderPath"></param>
public class MappingTestCaseAttribute(string testCasesFolderPath) 
    : TestCaseAttribute<List<XmlElementDescriptor>>(testCasesFolderPath, ".ExpectedMap.json");

/// <summary>
/// Loads test cases from a local folder.
/// The test cases are expected to be in the form of XML and json files
/// </summary>
public abstract class TestCaseAttribute<T>(string testCasesFolderPath, string resultFileExtension) : DataAttribute
{
    private const string TestFileExtension = ".xml";

    /// <summary>
    /// Returns the test cases for the test method, by finding all test file xml files in
    /// the test cases folder and their corresponding expected json files. 
    /// </summary>
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
                expectedResult:expectedMap!
            );
        }
    }

    // ReSharper disable once ReturnTypeCanBeEnumerable.Local, because of interfering design rules
    private static string[] GetTestFiles(string testCasesFolderPath)
    {
        var testFiles = Directory.GetFiles(testCasesFolderPath, $"*{TestFileExtension}");
        return testFiles;
    }

    private TM GetExpectedResult<TM>(string testFile)
    {
        var expectedFile = testFile.Replace(TestFileExtension, resultFileExtension);
        var expectedMap = JsonConvert.DeserializeObject<TM>(File.ReadAllText(expectedFile));
        Debug.Assert(expectedMap != null, nameof(expectedMap) + " != null");
        return expectedMap;
    }

    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        return GetTestCases().Select(couple => new object[] { couple });
    }
}