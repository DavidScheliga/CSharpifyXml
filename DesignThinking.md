# Design Thinking

Duration: 1.5h
Tools: Rider, GitHub Copilot

This section describes the design thinking behind the tool, at the beginning.
The final implementation of the prototype will most likely deviate from this 
first draft here.

## Empathize

### Why did XSD.exe not fulfill my needs?

- I wanted more control over the generated class names and name mappings.
- XSD.exe does not work with complex XML files.
- XSD.exe if perfect for handling serialization and deserialization.
  But it is not perfect for generating human-readable C# classes,
  in which information transfer to the human is my main goal.
- XSD.exe does not give me the ability to define the resulting c# file structure.
- XSD.exe is overkill for my purpose.

### What are my tasks?

- I just want to get a quick overview of the base structure of the XML file.
- I want to be able to quickly generate understandable C# classes from the XML file.
- Generate C# classes -> Rename classes for context -> deserialize XML file -> use the data.
- I do not really intend to serialize the data back to XML. Deserialization is the main goal.
- Main work path is: Deserialize XML -> Transform data -> Process data -> Store resulting data.

### What do I want?

- I want to be able to generate C# class files from XML files having the ability to define the resulting C# class
  structure especially the class naming.
- What I want is to get a quick overview of elements, their attributes and their direct children.
- I want to be able to define a template, by which the c# class files are generated.
- I want to use the XmlReader for exploration and training purposes.

## Define

I want to give the user a simple tool to generate C# classes from XML files for deserialization purposes, which
main goal is to achieve a human-readable C# class structure.

## Prototype

Application flow:

- Initialization phase (Use json configuration file or user input)
    - Read the configuration file or user input.
    - Read the XML file.

Seam 1: Configuration file or user input

This is the sole section of the library

- Mapping phase (Associated classes: XmlElementMap, XmlElementMapper)
    - List all elements as descriptors.
- Filtering phase (Associated classes: XmlElementFilter)
    - Remove all attributes, which are set to be ignored.
    - Drop if the element descriptor has no children and attributes, because they are primitives.
- Processing phase (Associated classes: XmlElementProcessor)
    - Transform the remaining element descriptors to class descriptors.
    - Generate the C# class file content using a template framework, by using the class descriptors.

Seam 2: Final output

- Output phase (Associated interface: IXmlElementOutput)
    - Store the generated C# class files in the output source.

### Class template structure

These are the points I want to define, or need.
The xml element can have attributes, which may be handled as fields or properties.
And this class can have children, which are also fields or properties.

````
# Keep the class renaming capability. Xml files are sometimes cryptic in their naming.
[XmlRoot or XmlElement(elementName= <<element's name>>)]
public class <<element's name>>
{
    # Keep also the renaming capability for the attributes and children.
    << foreach attribute in element.attributes >>
    [XmlAttribute(attributeName= <<attribute's name>>)]
    public <<attribute.GuesedType>> <<attribute.ElementName>> { get; set; }
    
    << foreach child in element.children >>
    [XmlElement(elementName= <<child's name>>)]
    public <<child.GuesedType>> <<child.ElementName>> { get; set; }
}
````
### What do I need?

I need to provide a collection of elements which fits classes, and their collection

### Elements override attributes

Because elements can have attributes and children, children must override attributes,
because they would get lost otherwise. **This project is not here to save xml files,
from being badly designed (garbage in, garbage out).**


### Classes after mapping

**class XmlElementDescriptor**
+ string: ElementName
+ List<string>: AttributeNames
+ List<XmlChildDescriptor>: Children

For a class file I need to know the element
Has the element name, its attributes and its first level children.


**class XmlChildDescriptor**
+ string: ElementName
+ string: TypeName
+ int: GroupCount
~+ bool: HasChildren~
~+ bool: HasAttributes~

The count states the number of this child within the parent element, to be able to directly
use a collection for this property. The HasChildren and HasAttributes are used to determine
if the child is a primitive or a complex type.

### Classes after processing for the template

After the mapping and filtering phase, I just need a class descriptor, which is a simple class
with a name and a list of fields. The user can then use this class to generate the C# class file.

**class XmlClassDescriptor**
+ bool: IsRoot
+ string: ClassName            
+ List<XmlClassField>: Fields

**class XmlClassField**
+ bool: IsAttribute
+ string: FieldName
+ string: FieldType
