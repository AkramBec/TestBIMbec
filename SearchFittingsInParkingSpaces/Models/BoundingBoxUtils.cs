namespace SearchFittingsInParkingSpaces.Models;

    public static class BoundingBoxUtils
{
    public static List<XYZ> GetOrientedBottomCorners(ElementMetaData elementMetaData)
    {
        // локальный bbox семейства
        var inst = elementMetaData.Element as FamilyInstance;
        var symBB = inst.get_BoundingBox(null);
        // дно локального bbox
        var localCorners = new List<XYZ>
        {
            new XYZ(symBB.Min.X, symBB.Min.Y, symBB.Min.Z),
            new XYZ(symBB.Max.X, symBB.Min.Y, symBB.Min.Z),
            new XYZ(symBB.Max.X, symBB.Max.Y, symBB.Min.Z),
            new XYZ(symBB.Min.X, symBB.Max.Y, symBB.Min.Z)
        };
        // мировой Transform
        var tr = inst.GetTransform();
        // поворачиваем каждую в мир
        return localCorners
            .Select(pt => tr.OfPoint(pt))
            .ToList();
    }
    
    public static bool PointInPolygonXY(XYZ p, IList<XYZ> poly)
    {
        bool inside = false;
        for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
        {
            var pi = poly[i];
            var pj = poly[j];
            // сравниваем по Y
            if (((pi.Y > p.Y) != (pj.Y > p.Y)) &&
                (p.X < (pj.X - pi.X) * (p.Y - pi.Y) / (pj.Y - pi.Y) + pi.X))
                inside = !inside;
        }
        return inside;
    }
    /// <summary>
    /// Создаёт плоскость, совпадающую с нижней гранью глобального BoundingBox.
    /// </summary>
    public static Plane CreateBottomPlane(BoundingBoxXYZ bb)
    {
        // Берём любую точку на нижней грани, например (Min.X, Min.Y, Min.Z)
        XYZ origin = new XYZ(bb.Min.X, bb.Min.Y, bb.Min.Z);
        // Нормаль—вверх по мировому Z
        XYZ normal = XYZ.BasisZ;
        return Plane.CreateByNormalAndOrigin(normal, origin);
    }

    /// <summary>
    /// Проецирует точку point на плоскость plane и возвращает UV-координаты относительно этой грани.
    /// U вдоль оси X, V вдоль оси Y плоскости.
    /// </summary>
    public static UV ProjectPointToBottomUV(Plane plane, XYZ point)
    {
        // находим смещение по нормали
        double d = plane.Normal.DotProduct(point - plane.Origin);
        // точка проекции
        XYZ proj = point - plane.Normal.Multiply(d);
        // UV относительно origin: U = ΔX, V = ΔY
        double u = proj.X - plane.Origin.X;
        double v = proj.Y - plane.Origin.Y;
        return new UV(u, v);
    }

    /// <summary>
    /// Построить UV-многоугольник для нижней грани: 
    /// четыре угла (Min,Min), (Max,Min), (Max,Max), (Min,Max).
    /// </summary>
    public static IList<UV> GetBottomUVPolygon(BoundingBoxXYZ bb)
    {
        return new List<UV>
        {
            new UV(bb.Min.X - bb.Min.X, bb.Min.Y - bb.Min.Y), // (0,0)
            new UV(bb.Max.X - bb.Min.X, bb.Min.Y - bb.Min.Y), // (dx,0)
            new UV(bb.Max.X - bb.Min.X, bb.Max.Y - bb.Min.Y), // (dx,dy)
            new UV(bb.Min.X - bb.Min.X, bb.Max.Y - bb.Min.Y)  // (0,dy)
        };
    }

    /// <summary>
    /// Классический алгоритм «точка в многоугольнике» (ray-crossing) в UV-координатах.
    /// </summary>
    public static bool PointInPolygon(UV p, IList<UV> poly)
    {
        bool inside = false;
        for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
        {
            UV pi = poly[i], pj = poly[j];
            if (((pi.V > p.V) != (pj.V > p.V)) &&
                (p.U < (pj.U - pi.U) * (p.V - pi.V) / (pj.V - pi.V) + pi.U))
            {
                inside = !inside;
            }
        }
        return inside;
    }
    
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