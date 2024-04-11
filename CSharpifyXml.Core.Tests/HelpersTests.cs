using CSharpifyXml.Core.Tests.Helpers;
using FluentAssertions;

namespace CSharpifyXml.Core.Tests;

public class HelpersTests
{
    /// <summary>
    /// This test aims to run through the test files and assert the
    /// the test cases are deserialized correctly.
    /// </summary>
    [Theory]
    [MappingTestCase(@".\TestAssets\TestCases")]
    public void LocalResultProviderRunsThrough(MappingSample sample)
    {
        // Assert
        File.Exists(sample.XmlFilepath);
        sample.ExpectedMap.Should().NotBeNull();
        sample.ExpectedMap.Descriptors.Should().NotBeNull();
    }
}