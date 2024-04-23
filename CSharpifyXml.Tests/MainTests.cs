using CSharpifyXml.Core;
using CSharpifyXml.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpifyXml.Tests;

public class MainTests
{
    private static ServiceProvider CreateTestServiceProvider()
    {
        var services = new ServiceCollection();
        services.ConfigureCSharpifyServices();
        services.AddSingleton(MappingConfiguration.Default());
        services.AddTransient<ITemplateCodeBuilder, ScribanTemplateCodeBuilder>();
        return services.BuildServiceProvider();
    }

    [Theory]
    [GenerationTestSamples("./TestAssets", "./TestAssets/Default.template.scriban")]
    public void DefaultGenerationOfClassFilesWorks(GenerationSample sample)
    {
        // Arrange
        var serviceProvider = CreateTestServiceProvider();
        var generator = serviceProvider.GetRequiredService<ITemplateCodeBuilder>();
        var sampleRequest = sample.CreateSampleRequest();

        // Act
        var generatedCode = generator.RenderClasses(sampleRequest);

        // Assert
        foreach (var codeResult in generatedCode)
        {
            AssertFileForContent(codeResult, sample.ClassDescriptions);
        }
    }

    private static void AssertFileForContent(ClassFileContent result, List<XmlClassDescriptor> descriptors)
    {
        var neededElements = descriptors.First(x => result.ProposedFilename.Contains(x.ElementName));
        neededElements.Should().NotBeNull();

        var code = result.Content;
        code.Should().Contain(neededElements.ElementName);

        var allProperties = neededElements.FromAttributes.Concat(neededElements.FromElements);
        foreach (var neededPropertyName in allProperties.Select(x => x.Name))
        {
            code.Should().Contain(neededPropertyName);
        }
    }
    

}