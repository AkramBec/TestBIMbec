namespace SearchFittingsInParkingSpaces.Models;

    public static class BoundingBoxUtils
{
    public static BoundingBoxXYZ CombineBoundingBoxes(IEnumerable<BoundingBoxXYZ> bBoxes)
    {
        if (bBoxes == null) throw new ArgumentNullException(nameof(bBoxes));

        XYZ min = null;
        XYZ max = null;

        foreach (var bb in bBoxes)
        {
            if (bb == null) continue;

            if (min == null)
            {
                min = bb.Min;
                max = bb.Max;
            }
            else
            {
                min = new XYZ(
                    Math.Min(min.X, bb.Min.X),
                    Math.Min(min.Y, bb.Min.Y),
                    Math.Min(min.Z, bb.Min.Z));

                max = new XYZ(
                    Math.Max(max.X, bb.Max.X),
                    Math.Max(max.Y, bb.Max.Y),
                    Math.Max(max.Z, bb.Max.Z));
            }
        }

        if (min == null)
        {
            // Ни одного ненулевого бокса не было
            return null;
        }

        return new BoundingBoxXYZ
        {
            Min = min,
            Max = max
        };
    }
}