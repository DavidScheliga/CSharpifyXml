using System.Text.RegularExpressions;

namespace CSharpifyXml;

public partial class SharpifyFilenameCommand : ISharpifyCommand
{
    private static readonly Regex FilenameRegex = MyRegex();

    public void EvaluateCommandInContent(
        IEnumerable<string> commands,
        ref ClassFileContent generatedResult
    )
    {
        foreach (var command in commands)
        {
            var match = FilenameRegex.Match(command);
            var foundMyCommand = match.Success;
            if (!foundMyCommand) continue;

            var newProposedFilename = match.Groups[1].Value;
            generatedResult.ProposedFilename = newProposedFilename;
            generatedResult.Content = FilenameRegex.Replace(generatedResult.Content, "");
            return;
        }
    }

    [GeneratedRegex(@"//csharpify:outputfilename=\s*([\w\.]*)\s*\r?\n?", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline)]
    private static partial Regex MyRegex();
}