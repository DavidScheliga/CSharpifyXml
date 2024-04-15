namespace CSharpifyXml.Core.Abstractions;

public interface IXmlItemDescriptor
{
    string? ElementName { get; set; }
    
    /// <summary>
    /// The (potential) type name of the element.
    /// </summary>
    string TypeName { get; set; }
}