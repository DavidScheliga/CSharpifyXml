using CSharpifyXml.Core;
using CSharpifyXml.Core.Abstractions;

namespace CSharpifyXml;

public class MappingConfiguration : IMappingConfiguration
{
    public string SequenceTemplate { get; set; } = null!;

    public static IMappingConfiguration Default()
    {
        return new MappingConfiguration() { SequenceTemplate = GlobalConstants.DefaultSequenceTemplate };
    }
}