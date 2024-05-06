namespace CSharpifyXml;

public class ClassFileContent(string proposedFilename, string content)
{
    public string ProposedFilename { get; set; } = proposedFilename;
    public string Content { get; set; } = content;
}

