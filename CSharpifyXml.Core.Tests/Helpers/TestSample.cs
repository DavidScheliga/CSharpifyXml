using CSharpifyXml.Core.Mapping;

namespace CSharpifyXml.Core.Tests.Helpers;

/// <summary>
/// A sample for testing the mapping of a XML file to a <see cref="XmlElementMap"/>.
/// </summary>
/// <param name="xmlFilepath">The filepath of the sample xml.</param>
/// <param name="expectedResult">The expected result of this test.</param>
/// <param name="testCaseName"></param>
public class TestSample(string xmlFilepath, object expectedResult, string testCaseName)
{
    public string XmlFilepath { get; } = xmlFilepath;

    /// <summary>
    /// The name of the file stem.
    /// </summary>
    private string TestCaseName { get; } = testCaseName;

    /// <summary>
    /// The expected map of the XML file.
    /// </summary>
    public object ExpectedResult { get; } = expectedResult;

    public StreamReader OpenXmlStream()
    {
        return new StreamReader(XmlFilepath);
    }
    
    /// <summary>
    /// Overriden to return a distinguishable name in the test runner theory list.
    /// </summary>
    /// <returns>The test case name is the best indicator.</returns>
    public override string ToString()
    {
        return TestCaseName;
    }
}