namespace SearchFittingsInParkingSpaces.Models;

public class ViewsOfType
{
    public static View3D Create(Document doc, ViewFamilyType viewType, Element element)
    {
        ElementId viewId = ElementId.InvalidElementId;

        var view = View3D.CreateIsometric(doc, viewType.Id);
        var box = element.get_BoundingBox(null);
        if (box != null)
        {
            var expand = 0.5;
            box.Min -= new XYZ(expand, expand, expand);
            box.Max += new XYZ(expand, expand, expand);
            view.SetSectionBox(box);
        }

        return view;
    }

    public static void Delete(Document doc, ViewFamilyType viewType)
    {
        var viewsToDelete = new FilteredElementCollector(doc)
            .OfClass(typeof(View3D))
            .Cast<View3D>()
            .Where(v => !v.IsTemplate && v.GetTypeId() == viewType.Id)
            .Select(v => v.Id)
            .ToList();

        if (viewsToDelete.Any())
        {
            doc.Delete(viewsToDelete);
        }
    }
}