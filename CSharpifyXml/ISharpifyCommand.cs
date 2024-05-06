namespace CSharpifyXml;

public interface ISharpifyCommand
{
    public void EvaluateCommandInContent(IEnumerable<string> commands, ref ClassFileContent generatedResult);
}