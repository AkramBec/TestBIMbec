namespace SearchFittingsInParkingSpaces.Models;

public class ElementMetaData
{
    public Element Element { get;}
    public Transform Transform { get;} = Transform.Identity;

    public ElementMetaData(Element element, Transform transform)
    {
        Element = element;
        Transform = transform;
    }
}