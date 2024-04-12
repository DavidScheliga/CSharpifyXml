using System.Diagnostics;
using System.Text.RegularExpressions;
using CSharpifyXml.Core.Abstractions;

namespace CSharpifyXml.Core;

public class TypeIdentifier : ITypeIdentifier
{
    private readonly struct RankingResult(int rank, string typeName)
    {
        public int Rank { get; } = rank;
        public string TypeName { get; } = typeName;
    }

    private readonly Dictionary<string, int> _primitiveTypeRanking = new()
    {
        // 0 is reserved for any name other than UNKWOWN and listed below.
        { "string", 1 }, // String always wins
        { "double", 2 }, // Double is more specific than int
        { "int", 3 }, // Int is more specific than unknown
        { GlobalConstants.UnknownTypeName, 4 },
        { "", 5 }
    };

    private readonly Regex _numberRegex = new Regex(@"^[+-]\d[,.]\d([eE][+-]\d)?$", RegexOptions.Compiled);
    private readonly Regex _integerRegex = new Regex(@"^\d+$", RegexOptions.Compiled);

    public string IdentifyTypeRepresentation(string content)
    {
        if (_numberRegex.IsMatch(content))
        {
            return "double";
        }

        return _integerRegex.IsMatch(content) ? "int" : "string";
    }

    public string DetermineDominantDataType(params string[] typeNames)
    {
        var mostDominantType = typeNames
            .Select(GetTypeRanking)
            .MinBy(x => x.Rank);
        return mostDominantType.TypeName;
    }

    private RankingResult GetTypeRanking(string typeName)
    {
        Debug.Assert(typeName != null, "Type name is null");
        var rankOfType = _primitiveTypeRanking.GetValueOrDefault(typeName, 0);
        return new RankingResult(rankOfType, typeName);
    }
}