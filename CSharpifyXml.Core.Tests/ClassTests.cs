using CSharpifyXml.Core.Mapping;
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
}