namespace CSharpifyXml;

public interface ISharpifyCommandManager
{
    ClassFileContent EvaluateCommandsInContent(ClassFileContent generatedResult);
}