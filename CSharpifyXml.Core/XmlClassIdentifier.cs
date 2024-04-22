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
        DeclareRemainingTypeNamesAfterSequencesWereDone(map.Descriptors);
        var resultingClasses = CreateFutureClasses(futureClasses);
        SortInSequences(sequences, resultingClasses);

        return resultingClasses.Values;
    }

    private static void DeclareRemainingTypeNamesAfterSequencesWereDone(
        Dictionary<RelationKey, XmlElementDescriptor> mapDescriptors)
    {
        foreach (var (key, descriptor) in mapDescriptors)
        {
            if (descriptor.TypeName != GlobalConstants.UnknownTypeName) continue;

            var finalTypeName = GetFinalTypeName(descriptor);
            descriptor.TypeName = finalTypeName;

            foreach (
                var child
                in descriptor.Children.Where(child => child.TypeName == GlobalConstants.UnknownTypeName)
            )
            {
                Debug.Assert(child.ElementName != null);
                var keyOfChildAsClass = key.CreateKeyForChild(child.ElementName);

                // Because the child could still be a class, we need to check if it was identified as such.
                // If not, we will leave it as an object, as the last resort.
                child.TypeName = mapDescriptors.TryGetValue(keyOfChildAsClass, out var childAsClass)
                    ? GetFinalTypeName(childAsClass)
                    : "object";
            }
        }
    }

    private static string GetFinalTypeName(IXmlElementDescriptor descriptor)
    {
        if (descriptor.TypeName != GlobalConstants.UnknownTypeName)
        {
            return descriptor.TypeName;
        }

        // The descriptor has children and is not a sequence.
        // Therefore, the element name defines the type as a class.
        if (descriptor.Children.Count > 0)
        {
            return descriptor.ElementName
                   ?? throw new InvalidOperationException("The element name should not be null");
        }

        // The remaining case is the descriptor being a leaf,
        // with no type being identified before, therefore leaving it to be an object.
        return "object";
    }

    private static void IdentifyMissingTypeNamesOfSequences(
        Dictionary<RelationKey, XmlElementDescriptor> mapDescriptors)
    {
        ArgumentNullException.ThrowIfNull(mapDescriptors);

        var allChildrenWithParentKey = mapDescriptors
            .Where(kv => ThisIsASequence(kv.Value));

        foreach (var (_, potentialSequence) in allChildrenWithParentKey)
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
        IReadOnlyDictionary<RelationKey, XmlClassDescriptor> resultingClasses
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
                IsRoot = descriptor.IsRoot,
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
        // Because a sequence cannot be a collection of different elements, which would
        // resolve to an array of 'object' being quite ambiguous.
        if (descriptor.Children.Count != 1) return false;

        var thisElementHadOnlyOneChildName = descriptor.Children.Count == 1;
        var theChildHasMultipleOccurrence = descriptor.Children[0].GroupCount > 1;
        // A sequence 
        return thisElementHadOnlyOneChildName && theChildHasMultipleOccurrence;
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