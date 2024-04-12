using System.Diagnostics;

namespace CSharpifyXml.Core;

/// <summary>
/// A map of element names to their descriptors.
/// </summary>
public class XmlElementMap
{
    /// <summary>
    /// Practically all found xml elements by their name.
    /// </summary>
    public Dictionary<string, XmlElementDescriptor>? Descriptors { get; set; } = new();

    public void AddDescriptor(XmlElementDescriptor descriptor)
    {
        Debug.Assert(descriptor.ElementName != null, "Element name is null");
        Debug.Assert(Descriptors != null, "Descriptors is null");
        
        if (Descriptors.TryGetValue(descriptor.ElementName, out var existingDescriptor))
        {
            existingDescriptor.Count++;
        }
        else
        {
            Descriptors.Add(descriptor.ElementName, descriptor);
        }
    }
}