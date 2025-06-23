namespace SearchFittingsInParkingSpaces.Models;

public class ElementMetaData
{
    public Element Element { get;}
    public Transform Transform { get;} = Transform.Identity;
    public string DocTitle{ get;} = "";

    public ElementMetaData(Element element, Transform transform)
    {
        Element = element;
        Transform = transform;
    }
    public ElementMetaData(Element element, Transform transform, string docTitle): this(element,transform)
    {
        DocTitle = docTitle;
    }
}