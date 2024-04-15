namespace CSharpifyXml.Core.Abstractions;

public interface ISequenceFormatter
{
    /// <summary>
    /// Returns the target representation of the sequence's type.
    /// </summary>
    /// <param name="typeName">Target type's name.</param>
    /// <example>
    /// Depending on the desired type, the method 
    ///     <code>
    ///     FormatSequence("Foo")
    ///     // May return:
    ///     //      "List&lt;Foo&gt;"
    ///     //      "Foo[]"
    ///     //      "IEnumerable&lt;Foo&gt;"
    ///     </code>
    /// </example>
    /// <returns>The desired representation of the sequence.</returns>
    string FormatSequence(string typeName);
}