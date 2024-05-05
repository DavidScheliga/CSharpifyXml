using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using CSharpifyXml.Core.Abstractions;
using CSharpifyXml.Core.Mapping;
using Microsoft.Extensions.Logging;

namespace CSharpifyXml.Core;

public partial class XmlClassIdentifier(
    IXmlElementMapper mapper,
    ISequenceFormatter sequenceFormatter,
    ILogger<IXmlClassIdentifier>? logger = null)
    : IXmlClassIdentifier
{
    public IXmlClassIdentifier Identify(StreamReader textReader)
    {
        // Split the descriptors into classes and potential sequences, which will be
        // properties of the classes.
        mapper.AddToMap(textReader);
        return this;
    }

    public IEnumerable<XmlClassDescriptor> GetDescriptors()
    {
        var map = mapper.CreateMap();
        IdentifySequenceTypeNames(map.Descriptors, sequenceFormatter);
        DeclareRemainingTypeNamesAfterSequencesWereDone(map.Descriptors);
        LogDebugDescriptors(descriptors:map.Descriptors);
        var resultingClasses = CreateFutureClasses(map.Descriptors);
        return resultingClasses.Values;
    }

    private void LogDebugDescriptors(Dictionary<RelationKey, XmlElementDescriptor> descriptors)
    {
        if (logger is null) return;

        var jsonSettings = new JsonSerializerOptions()
        {
            WriteIndented = true,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        };

        foreach (var (key, elementDescription) in descriptors)
        {
            var jsonText = JsonSerializer.Serialize(elementDescription, jsonSettings);
            logger.LogDebug(@"Element {key} mapped\r\n{jsonText}", key.ToString(), jsonText);
        }
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

    private static Dictionary<RelationKey, XmlClassDescriptor> CreateFutureClasses(
        IEnumerable<KeyValuePair<RelationKey, XmlElementDescriptor>> futureClasses
    )
    {
        var resultingClasses = new Dictionary<RelationKey, XmlClassDescriptor>();
        foreach (var (key, descriptor) in futureClasses)
        {
            var classDescriptor = CreateClassDescriptor(descriptor);
            resultingClasses.Add(key, classDescriptor);
        }

        return resultingClasses;
    }

    private static XmlClassDescriptor CreateClassDescriptor(XmlElementDescriptor descriptor)
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
        return classDescriptor;
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