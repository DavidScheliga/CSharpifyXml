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
}