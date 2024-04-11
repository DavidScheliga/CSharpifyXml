namespace CSharpifyXml.Core;

/// <summary>
/// A map of element names to their descriptors.
/// </summary>
public class XmlElementMap
{
    /// <summary>
    /// Practically all found xml elements by their name.
    /// </summary>
    public Dictionary<string, XmlElementDescriptor>? Descriptors { get; set; } 
}