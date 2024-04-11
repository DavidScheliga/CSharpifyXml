using CSharpifyXml.Core.Abstractions;

namespace CSharpifyXml.Core;

public class XmlClassIdentifier : IXmlClassIdentifier
{
    public XmlClassIdentifier() { }
    
    public IEnumerable<XmlClassDescriptor> Identify(StreamReader textReader)
    {
        throw new NotImplementedException();
    }
}