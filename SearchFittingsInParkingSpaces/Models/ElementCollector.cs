namespace SearchFittingsInParkingSpaces.Models;

public class ElementCollector
{
    private readonly List<ElementMetaData> _elementsMetaData = new();
    public IReadOnlyList<ElementMetaData> ElementsMetaData  => _elementsMetaData;

    public ElementCollector(Document doc, FilterRule filterRule)
    {
        _elementsMetaData.Clear();
        
        var elements = filterRule.Apply(doc);
        
        _elementsMetaData.AddRange(elements
            .Select(el =>new ElementMetaData(el,Transform.Identity)));

        foreach (var link in RevitLinks.Links)
        {
            var linkDoc = link.GetLinkDocument();
            if (linkDoc == null) continue;
            var linkTransform = link.GetTotalTransform();

            var linkElements = filterRule.Apply(linkDoc);

            _elementsMetaData.AddRange(linkElements
                .Select(el =>new ElementMetaData(el,linkTransform,linkDoc.Title)));
        }
    }
}