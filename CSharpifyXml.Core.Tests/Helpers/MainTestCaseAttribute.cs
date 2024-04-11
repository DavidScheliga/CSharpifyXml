namespace CSharpifyXml.Core.Tests.Helpers;

public class MainTestCaseAttribute(string testCasesFolderPath) 
    : TestCaseAttribute<List<XmlClassDescriptor>> (testCasesFolderPath, ".ExpectedClasses.json");