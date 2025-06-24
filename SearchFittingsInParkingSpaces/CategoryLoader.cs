﻿using Newtonsoft.Json;
using System.IO;

namespace SearchFittingsInParkingSpaces;

public class CategoryLoader
{
    public static List<BuiltInCategory> LoadCategories(string jsonPath)
    {
        var json = File.ReadAllText(jsonPath);
        var data = JsonConvert.DeserializeObject<CategoryData>(json);
        var categories = new List<BuiltInCategory>();

        foreach (var cat in data.Categories)
        {
            if (Enum.TryParse(cat, out BuiltInCategory bic))
            {
                categories.Add(bic);
            }
        }

        return categories;
    }

    private class CategoryData
    {
        public List<string> Categories { get; set; }
    }
}