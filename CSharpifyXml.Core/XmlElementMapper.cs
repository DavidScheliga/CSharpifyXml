using System.Diagnostics;
using System.Text;
using System.Xml;
using CSharpifyXml.Core.Abstractions;

namespace CSharpifyXml.Core;

public class XmlElementMapper : IXmlElementMapper
{
    private const string TheRootPath = @"\";
    private readonly ITypeIdentifier _typeIdentifier;
    private readonly Dictionary<XmlNodeHelper, XmlNodeHelper> _foundElements = new();

    private sealed class XmlNodeHelper : XmlElementDescriptor
    {
        public XmlNodeHelper(string elementName, string parentPath)
        {
            ParentPath = string.IsNullOrEmpty(parentPath) ? TheRootPath : parentPath;
            IsRoot = string.IsNullOrEmpty(parentPath);
            ElementName = elementName;
        }

        public string ParentPath { get; set; }
        public string ElementPath => Path.Join(ParentPath, ElementName);

        public override bool Equals(object? obj)
        {
            var other = obj as XmlNodeHelper;
            return other != null && other.ElementPath == ElementPath;
        }

        public override int GetHashCode()
        {
            return ElementPath.GetHashCode();
        }
    }

    public XmlElementMapper(ITypeIdentifier typeIdentifier)
    {
        _typeIdentifier = typeIdentifier;
    }

    public void ParseXmlToFoundElements(StreamReader xmlStream)
    {
        var settings = new XmlReaderSettings();
        settings.IgnoreWhitespace = true; // Ignore whitespace nodes

        using var reader = XmlReader.Create(xmlStream, settings);
        ParseElement(reader, string.Empty);
    }

    private XmlNodeHelper? ParseElement(XmlReader reader, string parentPath)
    {
        while (reader.Read())
        {
            if (reader.NodeType != XmlNodeType.Element) continue;

            var currentNode = new XmlNodeHelper(reader.Name, parentPath);
            RegisterNodeInFoundElements(currentNode);
            currentNode.Attributes = MapAttributesOfElement(reader).ToList();

            var theElementHasContent = reader.IsEmptyElement;
            var elementsInnerText = new StringBuilder();
            // Handle attributes or other aspects if necessary
            // for example, if you want to store attributes in Foo
            if (!theElementHasContent)
            {
                var depthOfElement = reader.Depth;
                while (reader.Read() && reader.Depth > depthOfElement)
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        reader.MoveToElement();
                        var childNode = ParseElement(reader, currentNode.ElementPath);
                        if (childNode == null) continue;
                        RegisterNodeInFoundElements(childNode);
                        Debug.Assert(currentNode.Children != null, nameof(currentNode.Children) + " != null");
                        currentNode.Children.Add(childNode);
                    }
                    else if (reader.NodeType == XmlNodeType.Text)
                    {
                        elementsInnerText.Append(reader.Value);
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        break;
                    }
                }
            }

            // If the current element has no children, it retains its InnerText.
            // If it has children, clear the InnerText since it's a nested element.
            Debug.Assert(currentNode.Children != null, nameof(currentNode.Children) + " != null");
            if (currentNode.Children.Count != 0) return currentNode;

            var valueTypeName = _typeIdentifier.IdentifyTypeRepresentation(elementsInnerText.ToString());
            var dominantDataType =
                _typeIdentifier.DetermineDominantDataType(valueTypeName, currentNode.TypeName);
            currentNode.TypeName = dominantDataType;

            return currentNode;
        }

        return null; // In case the XML is empty or has no elements
    }

    /// <summary>
    /// Maps the xml content into the <see cref="XmlElementMap">xml element map</see>.
    /// </summary>
    /// <param name="coupleXmlStream">The xml content as pure text.</param>
    /// <returns>The map of the xml elements.</returns>
    public XmlElementMap Map(StreamReader coupleXmlStream)
    {
        // We will start with an empty map.
        _foundElements.Clear();
        // Because we are only summarizing the xml elements like a book index,
        // we will take the element names, their attributes and either
        // the inner text, if it is a leaf, or continue with the children.
        ParseXmlToFoundElements(coupleXmlStream);
        return CreateRawElementMap();
    }

    private XmlElementMap CreateRawElementMap()
    {
        var elementMap = new XmlElementMap();
        foreach (var element in _foundElements.Values)
        {
            elementMap.AddDescriptor(element);
        }

        return elementMap;
    }

    /// <summary>
    /// This method adds the element to the found elements. If the parent path already
    /// exist this element is also added to the children of this parent.
    /// </summary>
    /// <param name="element"></param>
    private void RegisterNodeInFoundElements(XmlNodeHelper element)
    {
        var elementIsAlreadyRegistered = _foundElements.ContainsKey(element);
        if (elementIsAlreadyRegistered)
        {
            return;
        }

        _foundElements.Add(element, element);
    }

    private IEnumerable<XmlAttributeDescriptor> MapAttributesOfElement(XmlReader xmlReader)
    {
        if (!xmlReader.HasAttributes) yield break;

        while (xmlReader.MoveToNextAttribute())
        {
            yield return new XmlAttributeDescriptor
            {
                Name = xmlReader.Name,
                TypeName = _typeIdentifier.IdentifyTypeRepresentation(xmlReader.Value)
            };
        }
    }
}