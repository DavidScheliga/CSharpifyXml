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

    private sealed class XmlNodeHelper
    {
        public XmlNodeHelper(string elementName, string parentPath, int depth)
        {
            ParentPath = string.IsNullOrEmpty(parentPath) ? TheRootPath : parentPath;
            IsRoot = string.IsNullOrEmpty(parentPath);
            ElementName = elementName;
            Depth = depth;
        }

        public string TypeName { get; set; } = GlobalConstants.UnknownTypeName;

        public bool IsRoot { get; }
        public string ElementName { get; }

        /// <summary>
        /// The depth at which this element is located within the xml stucture.
        /// </summary>
        public int Depth { get; }

        public string InnerText { get; set; } = string.Empty;

        public string ParentPath { get; }

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

        public bool HasChildren => Children is { Count: > 0 };
        public List<XmlNodeHelper> Children { get; set; } = [];
        public List<XmlAttributeDescriptor> Attributes { get; set; } = [];

        public void SetDataTypeByIdentifier(ITypeIdentifier identifier)
        {
            // Because we are only interested in the inner text of the leafs,
            // we will not continue with the children.
            var valueTypeName = identifier.IdentifyTypeRepresentation(InnerText);
            TypeName = identifier.DetermineDominantDataType(valueTypeName, TypeName);
        }
        
        public bool IsALeaf => Attributes.Count == 0 && Children.Count == 0;
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

            var currentNode = new XmlNodeHelper(reader.Name, parentPath, reader.Depth);
            RegisterNodeInFoundElements(currentNode);
            currentNode.Attributes = MapAttributesOfElement(reader).ToList();

            ParseChildElements(reader, ref currentNode);

            currentNode.SetDataTypeByIdentifier(_typeIdentifier);
            return currentNode;
        }

        return null; // In case the XML is empty or has no elements
    }

    /// <summary>
    /// Parses the child elements further down of the current element.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="parentElement"></param>
    private void ParseChildElements(XmlReader reader, ref XmlNodeHelper parentElement)
    {
        var theElementHasContent = reader.IsEmptyElement;
        if (theElementHasContent) return;

        var textOfParent = new StringBuilder();
        // Because we need to distinguish whether we are getting deeper in the structure
        // while reading the next elements, this is checked by the depth of the reader.
        while (reader.Read() && reader.Depth > parentElement.Depth)
        {
            // We are a level deeper as before.
            if (reader.NodeType == XmlNodeType.Element)
            {
                reader.MoveToElement();
                var childNode = ParseElement(reader, parentElement.ElementPath);
                if (childNode == null) continue;
                RegisterNodeInFoundElements(childNode);
                Debug.Assert(parentElement.Children != null, nameof(parentElement.Children) + " != null");
                parentElement.Children.Add(childNode);
            }
            else if (reader.NodeType == XmlNodeType.Text)
            {
                textOfParent.Append(reader.Value);
            }
            else if (reader.NodeType == XmlNodeType.EndElement)
            {
                break;
            }
        }

        var thisIsNotALeafSoWeDoNotSetTheInnerText = parentElement.HasChildren;
        if (thisIsNotALeafSoWeDoNotSetTheInnerText) return;
        parentElement.InnerText = textOfParent.ToString();
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
        var transformedElements = _foundElements
            .Values
            .Where(x => !x.IsALeaf)
            .Select(TransformHelperToDescriptor);
        foreach (var transformedElement in transformedElements)
        {
            elementMap.AddDescriptor(transformedElement);
        }

        return elementMap;
    }

    private XmlElementDescriptor TransformHelperToDescriptor(XmlNodeHelper helper)
    {
        var children = MergeChildrenByElementName(helper.Children).ToList();
        var attributes = MergeAttributesByName(helper.Attributes).ToList();

        var descriptor = new XmlElementDescriptor
        {
            ElementName = helper.ElementName,
            TypeName = helper.TypeName,
            GroupCount = helper.Children.Count,
            IsRoot = helper.IsRoot,
            Attributes = attributes,
            Children = children
        };
        return descriptor;
    }

    private static string DetermineFinalDataType(XmlNodeHelper element)
    {
        // Because non leaf elements, which has children define classes
        // their data type name representation will be their element name.
        var elementWillBeAClass = element.Children is { Count: > 0 };
        Debug.Assert(element.ElementName != null, nameof(element.ElementName) + " != null");
        Debug.Assert(element.TypeName != null, nameof(element.TypeName) + " != null");
        return elementWillBeAClass ? element.ElementName : element.TypeName;
    }

    private IEnumerable<XmlChildDescriptor> MergeChildrenByElementName(IEnumerable<XmlNodeHelper> children)
    {
        var childrenGrouped = children.GroupBy(child => child.ElementName);
        foreach (var group in childrenGrouped)
        {
            var dataTypeNames = group.Select(DetermineFinalDataType);
            var dominantTypeName = _typeIdentifier.DetermineDominantDataType(dataTypeNames.ToArray());
            var firstElement = group.First();
            Debug.Assert(firstElement.ElementName != null, "Element name must be defined.");

            var mergedElement = new XmlChildDescriptor
            {
                GroupCount = group.Count(),
                TypeName = dominantTypeName,
                ElementName = firstElement.ElementName,
                HadAttributes = group.SelectMany(x => x.Attributes).Any(),
                HadChildren = group.SelectMany(x => x.Children).Any()
            };
            yield return mergedElement;
        }
    }

    /// <summary>
    /// This function merges all attributes of the given children.
    /// </summary>
    /// <param name="attributes">The children of a group.</param>
    /// <returns>The unique remaining attributes.</returns>
    private IEnumerable<XmlAttributeDescriptor> MergeAttributesByName(IEnumerable<XmlAttributeDescriptor> attributes)
    {
        var attributesGrouped = attributes.GroupBy(attr => attr.Name);
        foreach (var group in attributesGrouped)
        {
            var dataTypeNames = group.Select(attr => attr.TypeName).ToArray();
            Debug.Assert(dataTypeNames.All(x => x != null), "All data type names must be defined.");
            var dominantTypeName = _typeIdentifier.DetermineDominantDataType(dataTypeNames!);
            var firstAttribute = group.First();
            yield return new XmlAttributeDescriptor
            {
                Name = firstAttribute.Name,
                TypeName = dominantTypeName
            };
        }
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