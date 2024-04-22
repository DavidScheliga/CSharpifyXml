namespace CSharpifyXml.Tests.Helpers;

public class TestSample(
    List<string> expectedClassFileContents,
    string templateContent,
    string xmlContent,
    string testCaseName
)
{
    public string TestCaseName { get; } = testCaseName;

    /// <summary>
    /// The content of the blazor template file.
    /// </summary>
    public string TemplateContent { get; set; } = templateContent;

    /// <summary>
    /// The content of the sample XML file.
    /// </summary>
    public string XmlContent { get; set; } = xmlContent;

    /// <summary>
    /// The expected content of the generated c# class file.
    /// </summary>
    public List<string> ExpectedClassFileContents { get; set; } = expectedClassFileContents;
}