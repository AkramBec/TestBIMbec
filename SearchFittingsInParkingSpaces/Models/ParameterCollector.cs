namespace SearchFittingsInParkingSpaces.Models;

public class ParameterCollector
{
    public static double GetFromSymbolByGuid(FamilyInstance parking, string guid)
    {
        Guid lengthGuid = new Guid(guid);
        Parameter lengthParam = parking.Symbol.get_Parameter(lengthGuid);
        double length = lengthParam?.AsDouble() ?? 0; // в футах
        return length;
    }
}