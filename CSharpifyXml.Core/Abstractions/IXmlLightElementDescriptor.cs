namespace CSharpifyXml.Core.Abstractions;

/// <summary>
/// The name of the element.
/// </summary>
public interface IXmlLightElementDescriptor : IXmlItemDescriptor
{
    /// <summary>
    /// The number of elements this element name was within a parent element.
    /// </summary>
    /// <remarks>Starts with 1 by default, because if this element only occurs on existance.</remarks>
    int GroupCount { get; set; }

    /// <summary>
    /// States if this element is a sequence.
    /// </summary>
    bool IsASequence { get; }
}