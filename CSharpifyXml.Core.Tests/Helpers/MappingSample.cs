namespace CSharpifyXml.Core.Tests.Helpers;

/// <summary>
/// A sample for testing the mapping of a XML file to a <see cref="XmlElementMap"/>.
/// </summary>
/// <param name="xmlFilepath">The filepath of the sample xml.</param>
/// <param name="expectedMap">The expected map.</param>
/// <param name="testCaseName"></param>
public class MappingSample(string xmlFilepath, XmlElementMap expectedMap, string testCaseName)
{
    public string XmlFilepath { get; } = xmlFilepath;

    /// <summary>
    /// The name of the file stem.
    /// </summary>
    public string TestCaseName { get; set; } = testCaseName;

    /// <summary>
    /// The expected map of the XML file.
    /// </summary>
    public XmlElementMap ExpectedMap { get; set; } = expectedMap;

    public StreamReader OpenXmlStream()
    {
        return new StreamReader(xmlFilepath);
    }
}