using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using SteamConsoleHelper.Abstractions.Enums;
using SteamConsoleHelper.Abstractions.Inventory;
using SteamConsoleHelper.Exceptions;

namespace SteamConsoleHelper.Helpers
{
    public static class ItemTypeIdentifier
    {
        private const string FoilTagName = "Foil";
        private static readonly Dictionary<ItemType, string> typeDescriptionDictionary;

        static ItemTypeIdentifier()
        {
            typeDescriptionDictionary = typeof(ItemType).GetFields().Skip(1).ToDictionary(
                x => (ItemType)x.GetValue(null),
                field =>
            {
                if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    return attribute.Description;
                }
                else
                {
                    return field.Name;
                }
            });
        }

        public static ItemType ParseTypeFromDescription(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return ItemType.Undefined;
            }

            var (itemType, obj) = typeDescriptionDictionary.FirstOrDefault(x => value.Contains(x.Value, StringComparison.InvariantCultureIgnoreCase));

            return obj != null
                   ? itemType
                   : ItemType.Undefined;
        }

        public static bool IsCardFoil(this InventoryItem item)
        {
            if (item.ItemType != ItemType.TradingCard)
            {
                throw new InternalException(InternalError.InventoryItemIsNotACard);
            }

            return item.Tags.Exists(x => x.LocalizedTagName == FoilTagName);
        }
    }
}