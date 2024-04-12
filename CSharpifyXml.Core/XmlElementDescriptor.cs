using CSharpifyXml.Core.Abstractions;

namespace CSharpifyXml.Core;

/// <summary>
/// Describes an XML element, which its attributes and child elements.
/// </summary>
public class XmlElementDescriptor : IXmlElementDescriptor
{
    private string _typeName = GlobalConstants.UnknownTypeName;

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
    public string TypeName {
        get
        {
            return _typeName;
        }
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                value = GlobalConstants.UnknownTypeName;
            }
            _typeName = value;
        }
    }
    
    /// <summary>
    /// The child element's count within its parent.
    /// <summary>
    public int Count { get; set; } = 1;

    /// <summary>
    /// The attributes of the element.
    /// </summary>
    public List<XmlAttributeDescriptor>? Attributes { get; set; } = [];

    /// <summary>
    /// The child elements of the element.
    /// </summary>
    public List<XmlElementDescriptor>? Children { get; set; } = [];
}