namespace CSharpifyXml.Core;

public class XmlClassDescriptor
{
    public string ElementName { get; set; } = string.Empty;
    public List<XmlPropertyDescriptor> FromAttributes { get; set; } = [];
    public List<XmlPropertyDescriptor> FromElements { get; set; } = [];
}