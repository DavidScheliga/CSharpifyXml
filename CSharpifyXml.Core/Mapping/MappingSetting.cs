using CSharpifyXml.Core.Abstractions;

namespace CSharpifyXml.Core.Mapping;

public class MappingSetting : IMappingSetting
{
    public static MappingSetting Default => new();
}