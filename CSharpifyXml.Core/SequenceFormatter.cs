using System.Text.RegularExpressions;
using CSharpifyXml.Core.Abstractions;

namespace CSharpifyXml.Core;

public partial class SequenceFormatter(IMappingConfiguration configuration) : ISequenceFormatter
{
    private readonly string _template = configuration.SequenceTemplate;

    public string FormatSequence(string typeName)
    {
        return ReplacesTemplatePattern().Replace(_template, typeName);
    }

    [GeneratedRegex(@"\{\{\s?typename\s?\}\}", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex ReplacesTemplatePattern();
}