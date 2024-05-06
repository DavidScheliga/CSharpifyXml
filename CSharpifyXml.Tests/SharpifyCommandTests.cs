using CSharpifyXml.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpifyXml.Tests;

public class SharpifyCommandTests : BaseTestClass
{
    [Fact]
    public void FilenameIsDetectedAndCorrectlyReplaced()
    {
        // Assign
        var sample = new ClassFileContent(
            "Sample.cs",
            """
            //csharpify:outputfilename=ExpectedFilename.cs
            AnotherLine
            """
        );
        var sut = CreateTestServiceProvider().GetRequiredService<ISharpifyCommandManager>();

        // Act
        sut.EvaluateCommandsInContent(sample);

        // Assert
        sample.ProposedFilename.Should().Be("ExpectedFilename.cs");
        sample.Content.Should().Be("AnotherLine");
    }
}