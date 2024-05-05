using CSharpifyXml.Core;

namespace CSharpifyXml;

public class ScribanGenerationRequest(
    string targetNamespace,
    string templateContent,
    List<XmlClassDescriptor> xmlDescriptions,
    string outputPath = "./"
)
{
    public string TargetNamespace { get; set; } = targetNamespace;

    /// <summary>
    /// The razor template. 
    /// </summary>
    public string TemplateContent { get; set; } = templateContent;

    /// <summary>
    /// The class descriptions containing the needed information to generate
    /// a class file for deserialization. 
    /// </summary>
    public List<XmlClassDescriptor> XmlDescriptions { get; set; } = xmlDescriptions;

    /// <summary>
    /// The output folder for the classs files. 
    /// </summary>
    public string OutputPath { get; set; } = outputPath;
}