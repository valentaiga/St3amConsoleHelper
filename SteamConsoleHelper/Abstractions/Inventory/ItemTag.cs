namespace SteamConsoleHelper.Abstractions.Inventory
{
    public readonly struct ItemTag
    {
        public string Name { get; }
        
        public string Category { get; }
        
        public string LocalizedCategoryName { get; }
        
        public string LocalizedTagName { get; }

        public ItemTag(string name, string category, string localizedCategoryName, string localizedTagName)
        {
            Name = name;
            Category = category;
            LocalizedCategoryName = localizedCategoryName;
            LocalizedTagName = localizedTagName;
        }
    }
}