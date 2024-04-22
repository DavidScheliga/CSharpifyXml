namespace CSharpifyXml.Core;

public interface IMappingConfiguration
{
    /// <summary>
    /// Template for representing a sequence in the output. The case-insensitive replacement token is <c>{{typeName}}</c>.
    /// </summary>
    /// <example>
    /// For example, if the template is set to <c>"{{typeName}}>[]"</c> and the type name is <c>"int"</c>,
    /// it will be formatted as <c>"int[]"</c>.
    /// </example>
    string SequenceTemplate { get; set; }
}