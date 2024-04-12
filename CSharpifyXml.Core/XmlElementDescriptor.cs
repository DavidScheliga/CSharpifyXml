using CSharpifyXml.Core.Abstractions;

namespace CSharpifyXml.Core;

/// <summary>
/// Describes an XML element, which its attributes and child elements.
/// </summary>
public class XmlElementDescriptor : IXmlElementDescriptor
{
    /// <summary>
    /// States if the element is the root element of the XML given.
    /// </summary>
    public bool IsRoot { get; set; }

    /// <summary>
    /// The name of the element.
    /// </summary>
    public string? ElementName { get; set; }

    /// <summary>
    /// The (potential) type name of the element.
    /// </summary>
    public string TypeName { get; set; } = GlobalConstants.UnknownTypeName;

    /// <summary>
    /// The count this element appearing within the parent element. 
    /// </summary>
    public int GroupCount { get; set; } = 1;

    /// <summary>
    /// The attributes of the element.
    /// </summary>
    public List<XmlAttributeDescriptor> Attributes { get; set; } = [];

    /// <summary>
    /// The child elements of the element.
    /// </summary>
    public List<XmlChildDescriptor> Children { get; set; } = [];

}