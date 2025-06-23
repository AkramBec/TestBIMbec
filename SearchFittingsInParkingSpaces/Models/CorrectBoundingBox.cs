namespace SearchFittingsInParkingSpaces.Models;

public class CorrectBoundingBox
{
    public static BoundingBoxXYZ ComputeGlobal(FamilyInstance family)
    {
        var box = family.get_BoundingBox(null);
        var tr = family.GetTransform();
        return Transform(box, tr);
    }

    private static BoundingBoxXYZ Transform(BoundingBoxXYZ box, Transform transform)
    {
        var pts = new List<XYZ>
        {
            new XYZ(box.Min.X, box.Min.Y, box.Min.Z),
            new XYZ(box.Min.X, box.Min.Y, box.Max.Z),
            new XYZ(box.Min.X, box.Max.Y, box.Min.Z),
            new XYZ(box.Min.X, box.Max.Y, box.Max.Z),
            new XYZ(box.Max.X, box.Min.Y, box.Min.Z),
            new XYZ(box.Max.X, box.Min.Y, box.Max.Z),
            new XYZ(box.Max.X, box.Max.Y, box.Min.Z),
            new XYZ(box.Max.X, box.Max.Y, box.Max.Z)
        }.Select(p => transform.OfPoint(p)).ToList();

        var min = new XYZ(pts.Min(p => p.X), pts.Min(p => p.Y), pts.Min(p => p.Z));
        var max = new XYZ(pts.Max(p => p.X), pts.Max(p => p.Y), pts.Max(p => p.Z));

        return new BoundingBoxXYZ { Min = min, Max = max };
    }
}