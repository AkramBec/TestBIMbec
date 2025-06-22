namespace SearchFittingsInParkingSpaces.Models;

public class TitleInfo
{
    private static readonly char _nameSpliter = '_';
    private static string _title;
    public static string Title {get => _title;}
    
    public static string Discipline(string title)
    {
        _title = title;
        return GetPart(DocNamePart.Discipline);
    }
    
    private static string GetPart(DocNamePart part)
    {
        int index = (int)part - 1;
        return nameParts.Length > index ? nameParts[index] : "-";
    }

    private static string[] nameParts => Title.Split(_nameSpliter);

}