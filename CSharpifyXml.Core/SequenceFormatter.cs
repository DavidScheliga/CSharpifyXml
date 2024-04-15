using CSharpifyXml.Core.Abstractions;

namespace CSharpifyXml.Core;

public class SequenceFormatter : ISequenceFormatter
{
    public string FormatSequence(string typeName)
    {
        return $"{typeName}[]";
    }
}