using CSharpifyXml.Core;

namespace CSharpifyXml.Tests.Helpers;

public class GenerationSample(
    string templateContent,
    List<XmlClassDescriptor> classDescriptions,
    string testCaseName
)
{
    private const string TargetNamespace = "SampleNamespace";
    
    private string TestCaseName { get; } = testCaseName;

    /// <summary>
    /// The content of the blazor template file.
    /// </summary>
    private string TemplateContent { get; } = templateContent;

    /// <summary>
    /// The content of the sample XML file.
    /// </summary>
    public List<XmlClassDescriptor> ClassDescriptions { get; } = classDescriptions;

    public ScribanGenerationRequest CreateSampleRequest()
    {
        return new ScribanGenerationRequest(TargetNamespace, TemplateContent, ClassDescriptions);
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