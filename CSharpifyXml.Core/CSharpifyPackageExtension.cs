using CSharpifyXml.Core.Abstractions;
using CSharpifyXml.Core.Mapping;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpifyXml.Core;

public static class CSharpifyPackageExtension
{
    public static void ConfigureCSharpifyServices(this IServiceCollection services)
    {
        services.AddTransient<IXmlElementMapper, XmlElementMapper>();
        services.AddTransient<IXmlClassIdentifier, XmlClassIdentifier>();
        services.AddTransient<ITypeIdentifier, TypeIdentifier>();
        services.AddTransient<IXmlClassIdentifier, XmlClassIdentifier>();
        services.AddTransient<ISequenceFormatter, SequenceFormatter>();
    }
}