namespace SearchFittingsInParkingSpaces.Models;

public class Transformer
{
    public static Transform GetInverse(FamilyInstance parking)
    {
        LocationPoint location = parking.Location as LocationPoint;
        Transform translationTransform = Transform.CreateTranslation(location.Point);
        Transform transform = translationTransform.Multiply(Transform.CreateRotation(XYZ.BasisZ, location.Rotation));
        Transform inverseTransform = transform.Inverse;
        return inverseTransform;
    }
}