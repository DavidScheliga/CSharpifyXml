using CSharpifyXml.Core.Abstractions;

namespace CSharpifyXml.Core;

public class MappingSetting : IMappingSetting
{
    public static MappingSetting Default => new();
}