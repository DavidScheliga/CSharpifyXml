using System.Diagnostics;
using CSharpifyXml.Core.Abstractions;
using CSharpifyXml.Core.Mapping;

namespace CSharpifyXml.Core;

public class XmlClassIdentifier(IXmlElementMapper mapper, ISequenceFormatter sequenceFormatter) : IXmlClassIdentifier
{
    public IEnumerable<XmlClassDescriptor> Identify(StreamReader textReader)
    {
        // Split the descriptors into classes and potential sequences, which will be
        // properties of the classes.
        var map = mapper.Map(textReader);

        var futureClasses = map.Descriptors
            .Where(kv => !ThisIsASequence(kv.Value));
        var sequences = map.Descriptors
            .Where(kv => ThisIsASequence(kv.Value));

        IdentifyMissingTypeNamesOfSequences(map.Descriptors);
        var resultingClasses = CreateFutureClasses(futureClasses);
        SortInSequences(sequences, resultingClasses);

        return resultingClasses.Values;
    }

    private void IdentifyMissingTypeNamesOfSequences(Dictionary<RelationKey, XmlElementDescriptor> mapDescriptors)
    {
        ArgumentNullException.ThrowIfNull(mapDescriptors);

        var allChildrenWithParentKey = mapDescriptors
            .Where(kv => ThisIsASequence(kv.Value));

        foreach (var (parentKey, potentialSequence) in allChildrenWithParentKey)
        {
            if (potentialSequence.TypeName != GlobalConstants.UnknownTypeName) continue;
            
            var child = potentialSequence.Children[0];
            var childIsPotentialClass = child.TypeName == GlobalConstants.UnknownTypeName;
            Debug.Assert(child.ElementName != null);
            var newTypeName = childIsPotentialClass ? child.ElementName : child.TypeName; 
            potentialSequence.TypeName = newTypeName;
            child.TypeName = newTypeName;
        }
    }

    private void SortInSequences(
        IEnumerable<KeyValuePair<RelationKey, XmlElementDescriptor>> sequences,
        Dictionary<RelationKey, XmlClassDescriptor> resultingClasses
    )
    {
        ArgumentNullException.ThrowIfNull(resultingClasses);

        foreach (var (key, descriptor) in sequences)
        {
            var targetParentKey = key.GetParent();
            Debug.Assert(resultingClasses.ContainsKey(targetParentKey),
                $"The key {targetParentKey} should be present in the dictionary");

            var newProperty = new XmlPropertyDescriptor()
            {
                Name = key.ElementName,
                TypeName = sequenceFormatter.FormatSequence(descriptor.Children[0].TypeName)
            };

            // Drop existing element with same name.
            resultingClasses[targetParentKey].FromElements.RemoveAll(x => x.Name == newProperty.Name);
            resultingClasses[targetParentKey].FromElements.Add(newProperty);
        }
    }

    private static Dictionary<RelationKey, XmlClassDescriptor> CreateFutureClasses(
        IEnumerable<KeyValuePair<RelationKey, XmlElementDescriptor>> futureClasses
    )
    {
        var resultingClasses = new Dictionary<RelationKey, XmlClassDescriptor>();
        foreach (var (key, descriptor) in futureClasses)
        {
            Debug.Assert(descriptor.ElementName != null);

            var fromAttributes = TransformToPropertyDescriptor(descriptor.Attributes);
            var fromElements = TransformToPropertyDescriptor(descriptor.Children);

            var namesToDropInAttributes = fromElements
                .Select(x => x.Name)
                .Except(fromAttributes.Select(x => x.Name));

            var remainingAttributes = fromAttributes
                .Where(x => !namesToDropInAttributes.Contains(x.Name))
                .ToList();

            var classDescriptor = new XmlClassDescriptor()
            {
                ElementName = descriptor.ElementName,
                FromAttributes = remainingAttributes,
                FromElements = fromElements
            };

            resultingClasses.Add(key, classDescriptor);
        }

        return resultingClasses;
    }

    private static bool ThisIsASequence(IXmlElementDescriptor descriptor)
    {
        var elementHasJustOneChild = descriptor.Children.Count == 1;
        var theChildHasMultipleOccurrence = descriptor.Children[0].GroupCount > 1;
        // A sequence 
        return elementHasJustOneChild && theChildHasMultipleOccurrence;
    }

    private static List<XmlPropertyDescriptor> TransformToPropertyDescriptor(IEnumerable<IXmlItemDescriptor> attributes)
    {
        Debug.Assert(attributes != null, $"{nameof(attributes)} != null");
        return attributes
            .Select(a =>
                new XmlPropertyDescriptor()
                {
                    Name = a.ElementName ?? throw new InvalidOperationException("The element name should not be null"),
                    TypeName = a.TypeName
                }
            ).ToList();
    }
}