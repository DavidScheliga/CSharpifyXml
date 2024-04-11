namespace CSharpifyXml.Core.Abstractions;

public interface IXmlElementDescriptor
{
    /// <summary>
    /// States if the element is the root element of the XML given.
    /// </summary>
    bool IsRoot { get; set; }
    
    /// <summary>
    /// The name of the element.
    /// </summary>
    string? ElementName { get; set; }

    /// <summary>
    /// The attributes of the element.
    /// </summary>
    List<XmlAttributeDescriptor>? Attributes { get; set; }

    /// <summary>
    /// The child elements of the element.
    /// </summary>
    List<XmlChildElementDescriptor>? Children { get; set; }
}