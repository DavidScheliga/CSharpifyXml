namespace CSharpifyXml.Core.Tests.Helpers;

public class MappingTestCaseAttribute(string testCasesFolderPath) 
    : TestCaseAttribute<XmlElementMap>(testCasesFolderPath, ".ExpectedMap.json");