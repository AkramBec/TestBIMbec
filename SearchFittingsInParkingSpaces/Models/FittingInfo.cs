namespace SearchFittingsInParkingSpaces.Models;

public class FittingInfo
{
    public int ElementId { get; set; }
    public string Category { get; set; }
    public string DocumentTitle { get; set; }
    public ElementId? LinkInstanceId { get; set; }
    
    public ElementId ViewId { get; set; }

    public override string ToString()
    {
        return $"ID: {ElementId}, Категория: {Category}, Документ: {DocumentTitle}";
    }
}