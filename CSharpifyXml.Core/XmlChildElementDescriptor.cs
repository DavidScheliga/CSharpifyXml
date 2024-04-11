namespace CSharpifyXml.Core;

public class XmlChildElementDescriptor
{
    /// <summary>
    /// The name of the element.
    /// </summary>
    public string ElementName { get; set; }

    /// <summary>
    /// The child element's count within its parent.
    /// <summary>
    public int Count { get; set; } = 1;

    public bool HasChildren { get; set; }
    public bool HasAttributes { get; set; }
}