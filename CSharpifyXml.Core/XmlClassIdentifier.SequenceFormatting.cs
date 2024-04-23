using System.Diagnostics;
using CSharpifyXml.Core.Abstractions;
using CSharpifyXml.Core.Mapping;

namespace CSharpifyXml.Core;

public partial class XmlClassIdentifier
{
    /// <summary>
    /// Firstly just identify the pure type name.
    /// </summary>
    /// <param name="map"></param>
    /// <param name="sequenceFormatter"></param>
    private static void IdentifySequenceTypeNames(
        Dictionary<RelationKey, XmlElementDescriptor> map,
        ISequenceFormatter sequenceFormatter
    )
    {
        // <Table>                      class Table { Rows[] Rows {get;set;} }
        //   <Rows>                     class Rows { Row[] Row {get;set;} }
        //  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .
        //     <Row>                    class Row { int[] Cell {get;set;} }
        //         <Cell>1</Cell>
        //         <Cell>2</Cell>
        //     </Row>
        //  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .
        //     <Row>                            class Row { Cell[] Cell {get;set;} object Foo {get;set;} }
        //         <Cell col="A" val="1"/>      class Cell { string col {get;set;} string val {get;set;} }
        //         <Cell col="B" val="2"/>
        //         <Foo/>
        //     </Row>
        //  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .
        //   </Rows>
        // </Table>
        //
        foreach (var (key, element) in map)
        {
            if (!element.IsASequence) continue;
            var parentKey = key.GetParent();

            if (!map.TryGetValue(parentKey, out var myParent)) continue;
            var mySelfAtParent = myParent.Children.First(x => x.ElementName == element.ElementName);
            Debug.Assert(mySelfAtParent != null);
            Debug.Assert(element.ElementName != null);
            mySelfAtParent.TypeName = sequenceFormatter.FormatSequence(element.ElementName);

            foreach (var child in element.Children.Where(x => x.IsASequence))
            {
                child.TypeName = sequenceFormatter.FormatSequence(child.TypeName);
            }
        }
    }
}