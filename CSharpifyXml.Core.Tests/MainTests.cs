using CSharpifyXml.Core.Tests.Helpers;
using FluentAssertions;

namespace CSharpifyXml.Core.Tests;

/// <summary>
/// The major goal of the <see cref="CSharpifyXml.Core"/> package is to deconstruct XML into representations
/// of their elements, attributes, and child elements. These should be used to generate C# classes that
/// can be used to deserialize the XML. Serialization of the XML is not the goal of this package, as for this
/// the .NET xsd.exe of Microsoft is the choice to take.
/// </summary>
public class MainTests
{
    [Theory]
    [MappingTestCase(@".\TestAssets\TestCases")]
    public void SuccessfulMapTestCases(MappingSample sample)
    {
        // Arrange
        var xmlElementMapper = new XmlElementMapper();
        
        // Act
        XmlElementMap result;
        using (var sampleStream = sample.OpenXmlStream())
        {
            result = xmlElementMapper.Map(sampleStream);
        }

        // Assert
        result.Should().NotBeNull(because:"The sample steam should contain a valid XML file. Check the test case.");
        sample.ExpectedMap.Should().NotBeNull();
        result.Should().BeEquivalentTo(sample.ExpectedMap);
    }
}