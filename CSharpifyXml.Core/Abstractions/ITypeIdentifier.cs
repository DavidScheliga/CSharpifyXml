namespace CSharpifyXml.Core.Abstractions;

public interface ITypeIdentifier
{
    /// <summary>
    /// Identifies the type representation of the xml content, which can be either
    /// the attribute value or the element inner text.
    /// </summary>
    /// <param name="content">Attribute value or element inner text of leafs.</param>
    /// <returns>The return value should represent a valid target language syntax representation.</returns>
    public string IdentifyTypeRepresentation(string content);

    /// <summary>
    /// Determines the dominant data type between two given types. 
    /// </summary>
    /// <example>
    /// The following examples demonstrate the usage of this method.
    /// <code>
    /// var typeName = DetermineDominandDataType("int", "string", "UNKOWN");
    /// // typeName == "string"
    /// var typeName = DetermineDominandDataType("double", "int", "float");
    /// // typeName == "double"
    /// var typeName = DetermineDominandDataType("Foo", "UNKNOWN", "string");
    /// // typeName == "Foo"
    /// </code>
    /// <remarks>
    /// Keep in mind during implementation, that the start type <see cref="GlobalConstants.UnknownTypeName"/>.
    /// </remarks>
    /// </example>
    /// <param name="typeNames"></param>
    /// <returns></returns>
    public string DetermineDominantDataType(params string[] typeNames);
}