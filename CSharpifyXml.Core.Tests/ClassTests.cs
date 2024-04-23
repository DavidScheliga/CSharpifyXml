using System.Text.RegularExpressions;
using CSharpifyXml.Core.Mapping;
using CSharpifyXml.Core.Tests.Helpers;
using FluentAssertions;

namespace CSharpifyXml.Core.Tests;

public class ClassTests
{
    [Fact]
    public void RelationKeysAreEqualByTheParentElement()
    {
        // Arrange
        var sampleKey = new RelationKey("A", ParentPath:"/C/B");
        var keyWithSameParent = new RelationKey("A", ParentPath:"/B");
        var hashSet = new HashSet<RelationKey> { sampleKey };

        // Assert
        hashSet.Contains(keyWithSameParent).Should().BeTrue();
    }
    
    
    [Theory]
    [InlineData("{{typeName}}[]","Foo", "Foo[]")]
    [InlineData(" {{ typename }} ", "Foo", " Foo ")]
    [InlineData("List<{{ TYPENAME }}>", "Foo", "List<Foo>")]
    public void FormatSequenceWorksProperly(string template, string sampleInput, string expectedRepresentation)
    {
        // Arrange
        var sut = new SequenceFormatter(new TestConfig() { SequenceTemplate = template});
        // Act
        var resultingSequenceRepresentation = sut.FormatSequence(sampleInput);

        // Assert
        resultingSequenceRepresentation.Should().Be(expectedRepresentation);
    }


    [Theory]
    [InlineData("{{typeName}}[]", "Foo[]")]
    [InlineData("List<{{ TYPENAME }}>", "List<Foo>")]
    public void FormatterCanBeUsedToCreateACheckingPattern(string template, string shouldMatchThis)
    {
        const string newPattern = @"\w+";
        var formatter = new SequenceFormatter(new TestConfig() { SequenceTemplate = template});
        var sut = new Regex(formatter.FormatSequence(newPattern).Replace("[", @"\[").Replace("]", @"\]"));

        var match = sut.Match(shouldMatchThis);
        
        match.Success.Should().BeTrue();
    }
}