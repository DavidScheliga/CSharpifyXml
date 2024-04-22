using CSharpifyXml.Core.Mapping;

namespace CSharpifyXml.Core.Abstractions;

public interface IXmlElementMap
{
    /// <summary>
    /// Practically all found xml elements by their name.
    /// </summary>
    Dictionary<RelationKey, XmlElementDescriptor> Descriptors { get; }
}