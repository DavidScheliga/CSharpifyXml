using System.Data;
using CSharpifyXml.Core.Abstractions;

namespace CSharpifyXml.Core.Mapping;

public class XmlAttributeDescriptor(
    string attributeElementName,
    string attributeTypeName = GlobalConstants.UnknownTypeName
) : IXmlItemDescriptor
{
    public string? ElementName { get; set; } = attributeElementName;
    public string TypeName { get; set; } = attributeTypeName;
}