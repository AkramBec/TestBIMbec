namespace SearchFittingsInParkingSpaces.Models;

public class ViewsOfType
{
    public static View3D Create(Document doc, ViewFamilyType viewType, BoundingBoxXYZ bBox)
    {
        ElementId viewId = ElementId.InvalidElementId;

        var view = View3D.CreateIsometric(doc, viewType.Id);
        if (bBox != null)
        {
            var expand = 0.5;
            bBox.Min -= new XYZ(expand, expand, expand);
            bBox.Max += new XYZ(expand, expand, expand);
            view.SetSectionBox(bBox);
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