using System.Diagnostics;
using CSharpifyXml.Core.Abstractions;

namespace CSharpifyXml.Core.Mapping;

/// <summary>
/// A map of element names to their descriptors. On creation the map is under construction
/// and <see cref="XmlElementMap.Finish"/> must be called before accessing its items.
/// </summary>
public class XmlElementMap(ITypeIdentifier typeIdentifier)
{
    private bool _underConstruction = true;

    private readonly Dictionary<RelationKey, XmlElementDescriptor> _descriptors = new();

    /// <summary>
    /// Practically all found xml elements by their name.
    /// </summary>
    public Dictionary<RelationKey, XmlElementDescriptor> Descriptors
    {
        get
        {
            CheckConstruction();
            return _descriptors;
        }
    }

    private void CheckConstruction()
    {
        if (_underConstruction)
        {
            throw new InvalidOperationException("The element map is under construction. Please call 'Finish' first.");
        }
    }

    private void AddDescriptorIfNotExist(RelationKey key, XmlElementDescriptor descriptor)
    {
        Debug.Assert(descriptor.ElementName != null, "Element name is null");
        _descriptors.TryAdd(key, descriptor);
    }

    private bool TryGetDescriptor(RelationKey key, out XmlElementDescriptor? descriptor)
    {
        Debug.Assert(key != null, "Element key is null");
        Debug.Assert(_descriptors != null, "_descriptors is null");

        return _descriptors.TryGetValue(key, out descriptor);
    }

    public void Finish()
    {
        SortChildrenIntoParent();
        DropLeafs();
        _underConstruction = false;
    }

    private void DropLeafs()
    {
        var leafs = _descriptors.Where(x => x.Value.IsLeaf).ToList();
        foreach (var (key, _) in leafs)
        {
            _descriptors.Remove(key);
        }
    }

    private void SortChildrenIntoParent()
    {
        foreach (var (key, currentDescriptor) in _descriptors)
        {
            var parentKey = key.GetParent();
            if (TryGetDescriptor(parentKey, out var parentDescriptor))
            {
                var newChild = new XmlLightElementDescriptor()
                {
                    ElementName = currentDescriptor.ElementName,
                    TypeName = currentDescriptor.TypeName,
                    GroupCount = currentDescriptor.GroupCount,
                };
                parentDescriptor!.Children.Add(newChild);
            }
            else if (parentKey == RelationKey.Root)
            {
                currentDescriptor.IsRoot = true;
            }
            else
            {
                throw new InvalidOperationException("Element not found in descriptors");
            }
        }
    }

    public void OnElementFound(RelationKey elementPathKey)
    {
        var newElement = new XmlElementDescriptor
        {
            ElementName = elementPathKey.ElementName,
        };
        AddDescriptorIfNotExist(elementPathKey, newElement);
    }

    public void OnAttributesFound(RelationKey keyOfAttributeHolder, IEnumerable<XmlAttributeDescriptor> attributes)
    {
        if (TryGetDescriptor(keyOfAttributeHolder, out var descriptor))
        {
            descriptor!.Attributes = MergeAttributes(descriptor.Attributes, attributes).ToList();
        }
        else
        {
            throw new InvalidOperationException("Element not found in descriptors");
        }
    }

    private IEnumerable<XmlAttributeDescriptor> MergeAttributes(
        IEnumerable<XmlAttributeDescriptor> oldAttributes,
        IEnumerable<XmlAttributeDescriptor> newAttributes
    )
    {
        var allAttributes = new List<XmlAttributeDescriptor>();
        allAttributes.AddRange(oldAttributes);
        allAttributes.AddRange(newAttributes);

        foreach (var group in allAttributes.GroupBy(x => x.ElementName))
        {
            var attribute = group.First();
            if (group.Count() == 1)
            {
                Debug.Assert(attribute.ElementName != null, "Element name is null");
                yield return new XmlAttributeDescriptor(attribute.ElementName, attribute.TypeName);
                continue;
            }

            var dominantTypeName = typeIdentifier.DetermineDominantDataType(group.Select(x => x.TypeName).ToArray());
            Debug.Assert(attribute.ElementName != null, "Element name is null");
            yield return new XmlAttributeDescriptor(attribute.ElementName, dominantTypeName);
        }
    }

    public void OnElementValueFound(RelationKey elementPathKey, string text)
    {
        if (TryGetDescriptor(elementPathKey, out var descriptor))
        {
            var newTypeName = typeIdentifier.IdentifyTypeRepresentation(text);
            descriptor!.TypeName = typeIdentifier.DetermineDominantDataType(descriptor.TypeName, newTypeName);
        }
        else
        {
            throw new InvalidOperationException("Element not found in descriptors");
        }
    }

    public void OnElementCount(RelationKey relevantCountKey, int newCount)
    {
        if (TryGetDescriptor(relevantCountKey, out var descriptor))
        {
            if (descriptor!.GroupCount < newCount)
            {
                descriptor.GroupCount = newCount;
            }
        }
        else
        {
            throw new InvalidOperationException("Element not found in descriptors");
        }
    }
}