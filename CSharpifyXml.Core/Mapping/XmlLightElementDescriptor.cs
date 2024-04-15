using CSharpifyXml.Core.Abstractions;

namespace CSharpifyXml.Core.Mapping;

/// <summary>
/// This is a lightweight variant of an <see cref="IXmlElementDescriptor"/> as this
/// only caries the information, which would be needed to construct a property. 
/// </summary>
public class XmlLightElementDescriptor : IXmlLightElementDescriptor
{
    public string? ElementName { get; set; }
    
    /// <summary>
    /// The (potential) type name of the element.
    /// </summary>
    public string TypeName { get; set; } = GlobalConstants.UnknownTypeName;
    
    /// <summary>
    /// The number of elements this element name was within a parent element.
    /// </summary>
    /// <remarks>Starts with 1 by default, because if this element only occurs on existance.</remarks>
    public int GroupCount { get; set; } = 1;
}