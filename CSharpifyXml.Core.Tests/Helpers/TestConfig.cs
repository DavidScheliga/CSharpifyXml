using CSharpifyXml.Core.Abstractions;

namespace CSharpifyXml.Core.Tests.Helpers;

public class TestConfig : IMappingConfiguration
{
    public string SequenceTemplate { get; set; } = GlobalConstants.DefaultSequenceTemplate;
}