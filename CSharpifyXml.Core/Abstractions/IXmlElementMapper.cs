using CSharpifyXml.Core.Mapping;

namespace CSharpifyXml.Core.Abstractions;

public interface IXmlElementMapper
{
    XmlElementMap Map(StreamReader coupleXmlStream); 
}