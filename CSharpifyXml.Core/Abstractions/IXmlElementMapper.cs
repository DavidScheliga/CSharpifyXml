using CSharpifyXml.Core.Mapping;

namespace CSharpifyXml.Core.Abstractions;

public interface IXmlElementMapper
{
    void AddToMap(StreamReader coupleXmlStream);
    XmlElementMap CreateMap();
}