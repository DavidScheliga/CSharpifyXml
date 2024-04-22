namespace CSharpifyXml;

public interface ITemplateCodeBuilder
{
    /// <summary>
    /// Generates a class from the given XML stream.
    /// </summary>
    /// <param name="request">The information for creating class files.</param>
    /// <returns></returns>
    IEnumerable<ClassFileContent> RenderClasses(ScribanGenerationRequest request);
}