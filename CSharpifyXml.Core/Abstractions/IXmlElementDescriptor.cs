using CSharpifyXml.Core.Mapping;

namespace CSharpifyXml.Core.Abstractions;

public interface IXmlElementDescriptor : IXmlLightElementDescriptor
{
    /// <summary>
    /// States if the element is the root element of the XML given.
    /// </summary>
    bool IsRoot { get; set; }
    
    bool IsLeaf { get; }

    /// <summary>
    /// The attributes of the element.
    /// </summary>
    List<XmlAttributeDescriptor> Attributes { get; set; }

    /// <summary>
    /// The child elements of the element.
    /// </summary>
    List<XmlLightElementDescriptor> Children { get; set; }
}