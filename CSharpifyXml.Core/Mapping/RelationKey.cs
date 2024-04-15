namespace CSharpifyXml.Core.Mapping;

/// <summary>
/// The relation key is used to identify the relation between the parent and child element.
/// </summary>
/// <remarks>
/// RelationKeys are equal if the element name and the parent name are equal.
/// </remarks>
/// <param name="ElementName">The elements name.</param>
/// <param name="ParentPath">The parent path of the key. Only the parent name is taken into account.</param>
public record RelationKey(string ElementName, string ParentPath)
{
    private readonly string _parentName = Path.GetFileName(ParentPath);

    private static readonly string RootPath = Path.DirectorySeparatorChar.ToString();
    private string ElementPath => Path.Join(ParentPath, ElementName);

    public RelationKey CreateKeyForChild(string childName) => new(childName, ElementPath);
    public RelationKey CreateKeyForChild(string childName, int index) => new(childName, ElementPath);

    public static readonly RelationKey Root = new(string.Empty, RootPath);

    /// <summary>
    /// Checks against the element name and parent name (last element in the path).
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual bool Equals(RelationKey? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return ElementName == other.ElementName && _parentName == other._parentName;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ElementName, _parentName);
    }

    public RelationKey GetParent()
    {
        return new RelationKey(_parentName, Path.GetDirectoryName(ParentPath) ?? RootPath);
    }
}