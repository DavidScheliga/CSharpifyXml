using CSharpifyXml.Core.Tests.Helpers;
using FluentAssertions;

namespace CSharpifyXml.Core.Tests;

public class HelpersTests
{
    /// <summary>
    /// This test aims to run through the test files and assert
    /// the test cases are deserialized correctly.
    /// </summary>
    /// <remarks>For main test cases.</remarks>
    [Theory]
    [MainTestCase(@".\TestAssets\TestCases")]
    [MappingTestCase(@".\TestAssets\TestCases")]
    public void LocalMainSamplesRunThrough(TestSample sample)
    {
        // Assert
        File.Exists(sample.XmlFilepath);
        sample.Should().NotBeNull();
        sample.ExpectedResult.Should().NotBeNull();
    }
}