using CSharpifyXml.Core.Mapping;

namespace CSharpifyXml.Core.Abstractions;

/// <summary>
/// The class identifier is responsible for identifying the classes within the content
/// of the <see cref="XmlElementMap">xml element map</see>. The result needs to be
/// the <see cref="XmlClassDescriptor">class description</see>, which can be then
/// transformed into a c# file containing the class.
/// </summary>
public interface IXmlClassIdentifier
{
    IXmlClassIdentifier Identify(StreamReader textReader);
    IEnumerable<XmlClassDescriptor> GetDescriptors();
}