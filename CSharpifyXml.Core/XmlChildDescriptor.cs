namespace CSharpifyXml.Core;

public class XmlChildDescriptor
{
    public string? ElementName { get; set; }
    public string TypeName { get; set; } = GlobalConstants.UnknownTypeName;
    public bool HadChildren { get; set; }
    public bool HadAttributes { get; set; }
    public int GroupCount { get; set; }
}