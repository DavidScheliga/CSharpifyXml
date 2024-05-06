using System.Text.RegularExpressions;

namespace CSharpifyXml;

public partial class SharpifyCommandManager : ISharpifyCommandManager
{
    private const int MaximumCommandHeadlineCount = 1;

    private static readonly ISharpifyCommand[] KnownCommands = [new SharpifyFilenameCommand()];

    public ClassFileContent EvaluateCommandsInContent(ClassFileContent generatedResult)
    {
        using var generatedCodeReader = new StringReader(generatedResult.Content);
        var commandLines = new List<string>();
        for (var i = 0; i < MaximumCommandHeadlineCount; i++)
        {
            var commandLine = generatedCodeReader.ReadLine();
            if (string.IsNullOrEmpty(commandLine)) continue;
            if (!IsACommandLine(commandLine)) continue;

            commandLines.Add(commandLine);
        }

        foreach (var command in KnownCommands)
        {
            command.EvaluateCommandInContent(commandLines, ref generatedResult);
        }

        return generatedResult;
    }

    private static bool IsACommandLine(string line) => MyRegex().Match(line).Success;

    [GeneratedRegex(@"\/\/csharpify.*", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}