using System.Diagnostics;
using System.Reflection;
using Newtonsoft.Json;
using Xunit.Sdk;

namespace CSharpifyXml.Core.Tests.Helpers;

/// <summary>
/// Loads test cases from a local folder.
/// The test cases are expected to be in the form of XML and json files
/// </summary>
public class MappingTestCaseAttribute(string testCasesFolderPath) : DataAttribute
{
    private const string TestFileExtension = ".xml";
    private const string ExpectedFileExtension = ".map.json";

    /// <summary>
    /// Returns the test cases for the test method, by finding all test file xml files in
    /// the test cases folder and their corresponding expected json files. 
    /// </summary>
    /// <param name="testMethod"></param>
    /// <returns>The test cases as <see cref="MappingSample"/></returns>
    /// <exception cref="NotImplementedException"></exception>
    private IEnumerable<MappingSample> GetTestCases()
    {
        var testFiles = GetTestFiles(testCasesFolderPath);
        foreach (var testFile in testFiles)
        {
            var expectedMap = GetExpectedResult(testFile);

            yield return new MappingSample
            (
                testCaseName:Path.GetFileNameWithoutExtension(testFile),
                xmlFilepath:testFile,
                expectedMap:expectedMap
            );
        }
    }

    // ReSharper disable once ReturnTypeCanBeEnumerable.Local, because of interfering design rules
    private static string[] GetTestFiles(string testCasesFolderPath)
    {
        var testFiles = Directory.GetFiles(testCasesFolderPath, $"*{TestFileExtension}");
        return testFiles;
    }

    private static XmlElementMap GetExpectedResult(string testFile)
    {
        var expectedFile = testFile.Replace(TestFileExtension, ExpectedFileExtension);
        var expectedMap = JsonConvert.DeserializeObject<XmlElementMap>(File.ReadAllText(expectedFile));
        Debug.Assert(expectedMap != null, nameof(expectedMap) + " != null");
        return expectedMap;
    }

    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        return GetTestCases().Select(couple => new object[] { couple });
    }
}