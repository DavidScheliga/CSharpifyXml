using CSharpifyXml.Core.Abstractions;
using CSharpifyXml.Core.Mapping;
using CSharpifyXml.Core.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpifyXml.Core.Tests;

/// <summary>
/// The major goal of the <see cref="CSharpifyXml.Core"/> package is to deconstruct XML into representations
/// of their elements, attributes, and child elements. These should be used to generate C# classes that
/// can be used to deserialize the XML. Serialization of the XML is not the goal of this package, as for this
/// the .NET xsd.exe of Microsoft is the choice to take.
/// </summary>
public class MainTests : ATestClass
{
    /// <summary>
    /// This test represents the main usage case of the <see cref="CSharpifyXml.Core"/> package.
    /// </summary>
    /// <param name="sample">A test sample of the xml sample file and the expected result.</param>
    [Theory]
    [MainTestCase(@".\TestAssets\TestCases")]
    public void RunningTheMainUsageCase(TestSample sample)
    {
        // Arrange
        // This is the user's dependency injection container.
        var sutProvider = CreateTestServiceProvider();
        
        // Within this test we are only interested in the XmlClassIdentifier
        // because this is our main entry point.
        var sut = sutProvider.GetRequiredService<IXmlClassIdentifier>();

        // Act
        List<XmlClassDescriptor> result;
        using (var sampleStream = sample.OpenXmlStream())
        {
            result = sut.Identify(sampleStream).ToList();
        }

        // Assert
        result.Should().NotBeNull(because: "The sample steam should contain a valid XML file. Check the test case.");
        var expectedMap = (List<XmlClassDescriptor>)sample.ExpectedResult;
        expectedMap.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedMap);
    }

    [Theory]
    [MappingTestCase(@".\TestAssets\TestCases")]
    public void SuccessfulMapTestCases(TestSample sample)
    {
        // Arrange
        var sutProvider = CreateTestServiceProvider(); 
        var elementMapperSut = sutProvider.GetRequiredService<IXmlElementMapper>();
        
        // Act
        XmlElementMap elementMap;
        using (var sampleStream = sample.OpenXmlStream())
        {
            elementMap = elementMapperSut.Map(sampleStream);
        }

        // Assert
        elementMap.Should().NotBeNull(because: "The sample steam should contain a valid XML file. Check the test case.");
        var descriptors = elementMap.Descriptors.Values.ToList();
        var expectedMap = (List<XmlElementDescriptor>)sample.ExpectedResult;
        descriptors.Should().NotBeNull();
        descriptors.Should().BeEquivalentTo(expectedMap);
    }
}