namespace SearchFittingsInParkingSpaces.Models;

public class ElementGeometry
{
    private readonly ElementMetaData _elementMetaData;
    private PlanarFace _bottomFace;
    public PlanarFace BottomFace { get => _bottomFace; }
    private XYZ _originGlobal;
    public XYZ OriginGlobal { get => _originGlobal; }

    public ElementGeometry(ElementMetaData elementMetaData)
    {
        _elementMetaData = elementMetaData;
    }
    public PlanarFace GetBottomFace()
    {
        var opts = new Options { ComputeReferences = false };
        var geo = _elementMetaData.Element.get_Geometry(opts);
        var solid = geo.OfType<Solid>().FirstOrDefault(s => s.Volume > 0);
        _bottomFace =  solid.Faces
            .OfType<PlanarFace>()
            .Where(f => f.FaceNormal.IsAlmostEqualTo(XYZ.BasisZ.Negate()))
            .OrderBy(f => f.Origin.Z)
            .First();
        _originGlobal = _elementMetaData.Transform.OfPoint(_bottomFace.Origin);
        return _bottomFace;
    }
}