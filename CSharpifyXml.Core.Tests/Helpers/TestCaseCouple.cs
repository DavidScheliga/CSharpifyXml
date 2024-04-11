namespace CSharpifyXml.Core.Tests.Helpers;

public class TestCaseCouple
{
    /// <summary>
    /// The name of the file stem.
    /// </summary>
    public string TestCaseName { get; set; }
    
    /// <summary>
    /// The input XML stream.
    /// </summary>
    public StreamReader XmlStream { get; set; }
    /// <summary>
    /// The expected map of the XML file.
    /// </summary>
    public XmlElementMap ExpectedMap { get; set; }
}