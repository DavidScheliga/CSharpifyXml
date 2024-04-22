using CSharpifyXml.Core.Abstractions;
using CSharpifyXml.Core.Mapping;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpifyXml.Core;

public static class CSharpifyPackageExtension
{
    public static void ConfigureCSharpifyServices(this IServiceCollection services)
    {
        services.AddTransient<ISequenceFormatter, SequenceFormatter>();
        services.AddTransient<ITypeIdentifier, TypeIdentifier>();
        services.AddTransient<IXmlElementMap, XmlElementMap>();
        services.AddTransient<IXmlElementMapper, XmlElementMapper>();
        services.AddTransient<IXmlClassIdentifier, XmlClassIdentifier>();
    }
}