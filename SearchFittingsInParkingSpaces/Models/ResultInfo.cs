namespace SearchFittingsInParkingSpaces.Models;

public class ResultInfo
{
    private readonly Element _element;
    private readonly TitleInfo _titleInfo;
    
    public int ElementId {get;}
    public string Category {get;}
    public string SystemClassification {get;}
    public string Name {get;}
    public string Designation {get;}
    public string DocumentTitle {get;}
    public string Discipline { get; }
    public string ViewName { get;}
    public ElementId ViewId { get;}
    
    public ResultInfo(Element element, string documentTitle, ElementId viewId)
    {
        _element = element;
        DocumentTitle = documentTitle;
        ViewId = viewId;

        ElementId = element.Id.IntegerValue;
        Category = element.Category?.Name ?? "Нет категории";
        SystemClassification = element.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM).AsString();

        if (documentTitle != "")
        {
            Discipline = TitleInfo.Discipline(documentTitle);
            ViewName = $"{Discipline}_{Category}_{ElementId}";
        }
        else
        {
            ViewName = $"{Category}_{ElementId}";
        }
    }

    public override string ToString()
    {
        if (DocumentTitle != "")
        {
            return $"{Discipline} : {ElementId}, Категория: {Category}";
        }
        else
        {
            return $"{ElementId}, Категория: {Category}";
        }
        
    }
}