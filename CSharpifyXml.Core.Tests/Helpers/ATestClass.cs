using Microsoft.Extensions.DependencyInjection;

namespace CSharpifyXml.Core.Tests.Helpers;

public abstract class ATestClass
{
    /// <summary>
    /// Creates a service provider for the tests.
    /// </summary>
    /// <returns></returns>
    protected static ServiceProvider CreateTestServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IMappingConfiguration, TestConfig>();
        services.ConfigureCSharpifyServices();
        return services.BuildServiceProvider();
    }
}