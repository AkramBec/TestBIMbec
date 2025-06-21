namespace SearchFittingsInParkingSpaces.Models;

public class FittingInfo
{
    private readonly Element _element;
    private readonly TitleInfo _titleInfo;
    
    public int ElementId { get; }
    public string Category { get; }
    public string DocumentTitle { get; }
    public string Discipline { get; }
    public string ViewName { get;}
    public ElementId ViewId { get;}
    
    public FittingInfo(Element element, string documentTitle, ElementId viewId)
    {
        _element = element;
        DocumentTitle = documentTitle;
        ViewId = viewId;
        
        _titleInfo = new TitleInfo(documentTitle);
        
        ElementId = element.Id.IntegerValue;
        Category = element.Category?.Name ?? "Нет категории";
        Discipline = _titleInfo.Discipline;
        ViewName = $"{Discipline}_{Category}_{ElementId}";;
    }

    public override string ToString()
    {
        return $"{ElementId} : {Discipline}, Категория: {Category}";
    }
}