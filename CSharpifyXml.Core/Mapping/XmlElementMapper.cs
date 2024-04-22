using System.Diagnostics;
using System.Text;
using System.Xml;
using CSharpifyXml.Core.Abstractions;

namespace CSharpifyXml.Core.Mapping;

/// <summary>
/// Runs through the xml content and maps the elements, their attributes and values to
/// the <see cref="XmlElementMap"/> by invoking its methods <see cref="XmlElementMap.OnElementFound"/>,
/// <see cref="XmlElementMap.OnAttributesFound"/> and <see cref="XmlElementMap.OnElementValueFound"/>.
/// </summary>
/// <param name="typeIdentifier"></param>
public class XmlElementMapper(ITypeIdentifier typeIdentifier) : IXmlElementMapper
{
    private sealed class ElementCounter(RelationKey parentKey)
    {
        private readonly Dictionary<RelationKey, int> _counts = new();

        public void AddCount(string elementName)
        {
            var key = parentKey.CreateKeyForChild(elementName);
            if (_counts.TryAdd(key, 1)) return;

            _counts[key]++;
        }

        public IEnumerable<KeyValuePair<RelationKey, int>> GetAllWhereMoreThanOne()
        {
            return _counts.Where(kv => kv.Value > 1);
        }
    }

    /// <summary>
    /// Starts the parsing of the xml content to the target map.
    /// </summary>
    /// <param name="map"></param>
    /// <param name="xmlStream"></param>
    private void StartParsingXmlToMap(ref XmlElementMap map, StreamReader xmlStream)
    {
        var settings = new XmlReaderSettings
        {
            IgnoreWhitespace = true // Ignore whitespace nodes
        };
        using var reader = XmlReader.Create(xmlStream, settings);
        MoveToTheFirstElement(reader);
        ParseElement(ref map, reader, RelationKey.Root);
    }

    /// <summary>
    /// For moving the reader to the first element, because <see cref="ParseElement"/>
    /// needs to start at an element.
    /// </summary>
    /// <param name="reader"></param>
    private static void MoveToTheFirstElement(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                return;
            }
        }
    }

    private void ParseElement(ref XmlElementMap map, XmlReader reader, RelationKey parentElementPath)
    {
        Debug.Assert(reader.NodeType == XmlNodeType.Element, "The reader must be on an element node.");
        var currentElementKey = parentElementPath.CreateKeyForChild(reader.Name);
        map.OnElementFound(currentElementKey);
        // Because the reader will point on the last attribute using MapAttributesOfElement
        // the reason to leave this function must be saved here.
        var theElementDoesNotContainAnythingDoLeaveHere = reader.IsEmptyElement;
        map.OnAttributesFound(currentElementKey, MapAttributesOfElement(reader).ToList());

        if (theElementDoesNotContainAnythingDoLeaveHere)
        {
            return;
        }

        var textCache = new StringBuilder();
        var subElementCounts = new ElementCounter(currentElementKey);
        // Because we want the inner text of the element, we need to read the next elements.
        // We hope for a text node and an end element node to finish the element.
        var childFound = false;
        while (reader.Read())
        {
            // Because the next element can only be a child of the current element,
            // we need to jump into the next recursive call of ParseElement.
            if (reader.NodeType == XmlNodeType.Element)
            {
                childFound = true;
                subElementCounts.AddCount(reader.Name);
                ParseElement(ref map, reader, parentElementPath: currentElementKey);
            }
            else if (reader.NodeType == XmlNodeType.Text)
            {
                textCache.Append(reader.Value);
            }
            else if (reader.NodeType == XmlNodeType.EndElement)
            {
                break;
            }
        }

        foreach (var relevantCount in subElementCounts.GetAllWhereMoreThanOne())
        {
            map.OnElementCount(relevantCount.Key, relevantCount.Value);
        }

        var theCurrentNodeIsALeafStoreTheText = !childFound;
        if (theCurrentNodeIsALeafStoreTheText)
        {
            map.OnElementValueFound(currentElementKey, textCache.ToString());
        }
        else
        {
            // Because we are not interested in the text of a non-leaf element.
            textCache.Clear();
        }
    }

    /// <summary>
    /// Maps the xml content into the <see cref="XmlElementMap">xml element map</see>.
    /// </summary>
    /// <param name="coupleXmlStream">The xml content as pure text.</param>
    /// <returns>The map of the xml elements.</returns>
    public XmlElementMap Map(StreamReader coupleXmlStream)
    {
        // We will start with an empty map.
        var freshMap = new XmlElementMap(typeIdentifier);
        // Because we are only summarizing the xml elements like a book index,
        // we will take the element names, their attributes and either
        // the inner text, if it is a leaf, or continue with the children.
        StartParsingXmlToMap(ref freshMap, coupleXmlStream);
        freshMap.Finish();
        return freshMap;
    }

    private IEnumerable<XmlAttributeDescriptor> MapAttributesOfElement(XmlReader xmlReader)
    {
        if (!xmlReader.HasAttributes) yield break;

        while (xmlReader.MoveToNextAttribute())
        {
            yield return new XmlAttributeDescriptor
            (
                xmlReader.Name,
                typeIdentifier.IdentifyTypeRepresentation(xmlReader.Value)
            );
        }
    }
}