using CSharpifyXml.Core;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpifyXml.Tests.Helpers;

public abstract class BaseTestClass
{
    protected static ServiceProvider CreateTestServiceProvider()
    {
        var services = new ServiceCollection();
        services.ConfigureCSharpifyServices();
        services.AddSingleton(MappingConfiguration.Default());
        services.AddTransient<ITemplateCodeBuilder, ScribanTemplateCodeBuilder>();
        services.AddSingleton<ISharpifyCommandManager, SharpifyCommandManager>();
        return services.BuildServiceProvider();
    }
}