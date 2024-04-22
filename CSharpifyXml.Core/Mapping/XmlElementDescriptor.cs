using CSharpifyXml.Core.Abstractions;

namespace CSharpifyXml.Core.Mapping;

/// <summary>
/// Describes an XML element, which its attributes and child elements.
/// </summary>
public class XmlElementDescriptor : XmlLightElementDescriptor, IXmlElementDescriptor 
{
    /// <summary>
    /// States if the element is the root element of the XML given.
    /// </summary>
    public bool IsRoot { get; set; }

    public bool IsLeaf => Children.Count == 0 && Attributes.Count == 0;

    /// <summary>
    /// The attributes of the element.
    /// </summary>
    public List<XmlAttributeDescriptor> Attributes { get; set; } = [];

    /// <summary>
    /// The child elements of the element.
    /// </summary>
    public List<XmlLightElementDescriptor> Children { get; set; } = [];
}