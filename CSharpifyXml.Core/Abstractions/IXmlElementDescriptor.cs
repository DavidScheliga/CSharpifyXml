namespace CSharpifyXml.Core.Abstractions;

public interface IXmlElementDescriptor
{
    /// <summary>
    /// The name of the element.
    /// </summary>
    string? ElementName { get; set; }

    /// <summary>
    /// The attributes of the element.
    /// </summary>
    List<string>? AttributeNames { get; set; }

    /// <summary>
    /// The child elements of the element.
    /// </summary>
    List<XmlChildElementDescriptor>? Children { get; set; }
}