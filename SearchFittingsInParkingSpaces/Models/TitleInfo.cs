namespace SearchFittingsInParkingSpaces.Models;

public class TitleInfo
{
    private readonly char _nameSpliter = '_';
    public string Title { get;}

    public TitleInfo(string title)
    {
        Title = title;
    }

    private string[] nameParts => Title.Split(_nameSpliter);

    public string GetPart(DocNamePart part)
    {
        int index = (int)part - 1;
        return nameParts.Length > index ? nameParts[index] : "-";
    }

    public string Discipline => GetPart(DocNamePart.Discipline);
}