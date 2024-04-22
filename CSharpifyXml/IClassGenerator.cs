namespace CSharpifyXml;

public interface IClassGenerator
{
    /// <summary>
    /// Generates a class from the given XML stream.
    /// </summary>
    /// <param name="xmlStream">The XML stream to generate the class from.</param>
    /// <returns>The generated class.</returns>
    object GenerateClasses(StreamReader xmlStream);
}