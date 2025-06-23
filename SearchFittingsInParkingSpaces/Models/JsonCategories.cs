namespace SearchFittingsInParkingSpaces.Models;

public class JsonCategories
{
    public static List<BuiltInCategory> BuiltInCategories
    {
        get
        {
            string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string folder = System.IO.Path.GetDirectoryName(assemblyPath);
            string jsonPath = System.IO.Path.Combine(folder, "familyCategories.json");
            List<BuiltInCategory> categories = CategoryLoader.LoadCategories(jsonPath);
            return categories;
        }
    }
}