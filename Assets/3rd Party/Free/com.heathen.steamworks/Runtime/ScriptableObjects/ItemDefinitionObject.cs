#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents a Steam Inventory item definition
    /// </summary>
    [Serializable]
    public class ItemDefinitionObject : ScriptableObject
    {
        /// <summary>
        /// The ID of the item
        /// </summary>
        [SerializeField]
        internal int id;
        /// <summary>
        /// The type of the item
        /// </summary>
        [SerializeField]
        internal InventoryItemType item_type;
        /// <summary>
        /// The name of the item
        /// </summary>
        [SerializeField]
        internal LanguageVariantNode item_name = new LanguageVariantNode { node = "name" };
        /// <summary>
        /// The description of the item
        /// </summary>
        [SerializeField]
        internal LanguageVariantNode item_description = new LanguageVariantNode { node = "description" };
        /// <summary>
        /// The display type for the item
        /// </summary>
        [SerializeField]
        internal LanguageVariantNode item_display_type = new LanguageVariantNode { node = "display_type" };
        /// <summary>
        /// The bundle information of the item
        /// </summary>
        [SerializeField]
        internal Bundle item_bundle = new Bundle();
        /// <summary>
        /// The promo information for the item
        /// </summary>
        [SerializeField]
        internal PromoRule item_promo = new PromoRule();
        /// <summary>
        /// The drop start time of the item
        /// </summary>
        [SerializeField]
        internal string item_drop_start_time;
        /// <summary>
        /// The item exchange recipes
        /// </summary>
        [SerializeField]
        internal ExchangeCollection item_exchange = new ExchangeCollection();
        /// <summary>
        /// The items price information
        /// </summary>
        [SerializeField]
        internal Price item_price = new Price();
        /// <summary>
        /// The items background color
        /// </summary>
        [SerializeField]
        internal Color item_background_color = new Color();
        /// <summary>
        /// The item's name's color
        /// </summary>
        [SerializeField]
        internal Color item_name_color = new Color();
        /// <summary>
        /// A URL for the items main icon
        /// </summary>
        [SerializeField]
        internal string item_icon_url;
        /// <summary>
        /// A large variant of the items icon
        /// </summary>
        [SerializeField]
        internal string item_icon_url_large;
        /// <summary>
        /// Is the item marketable
        /// </summary>
        [SerializeField]
        internal bool item_marketable;
        /// <summary>
        /// Is the item tradable among players
        /// </summary>
        [SerializeField]
        internal bool item_tradable;
        /// <summary>
        /// A collection of tags for the item
        /// </summary>
        [SerializeField]
        internal TagCollection item_tags = new TagCollection();
        /// <summary>
        /// A collection of tag generators related to the object
        /// </summary>
        [SerializeField]
        internal List<ItemDefinitionObject> item_tag_generators = new List<ItemDefinitionObject>();
        /// <summary>
        /// The tag generator name of the object
        /// </summary>
        [SerializeField]
        internal string item_tag_generator_name;
        /// <summary>
        /// The tag generator values of the object
        /// </summary>
        [SerializeField]
        internal TagGeneratorValues item_tag_generator_values = new TagGeneratorValues();
        /// <summary>
        /// Tags for use in the item store
        /// </summary>
        [SerializeField]
        internal List<string> item_store_tags = new List<string>();
        /// <summary>
        /// Additional item store images
        /// </summary>
        [SerializeField]
        internal List<string> item_store_images = new List<string>();
        /// <summary>
        /// Is the item hidden to the user
        /// </summary>
        [SerializeField]
        internal bool item_hidden;
        /// <summary>
        /// Is the item hidden to the store
        /// </summary>
        [SerializeField]
        internal bool item_store_hidden;
        /// <summary>
        /// Does the item have a drop limit
        /// </summary>
        [SerializeField]
        internal bool item_use_drop_limit;
        /// <summary>
        /// If the item has a drop limit what is the drop limit
        /// </summary>
        [SerializeField]
        internal uint item_drop_limit;
        /// <summary>
        /// The time between drops
        /// </summary>
        [SerializeField]
        internal uint item_drop_interval;
        /// <summary>
        /// Does the item use a drop window
        /// </summary>
        [SerializeField]
        internal bool item_use_drop_window;
        /// <summary>
        /// What is the items drop window
        /// </summary>
        [SerializeField]
        internal uint item_drop_window;
        /// <summary>
        /// How many drops can this item have per window
        /// </summary>
        [SerializeField]
        internal uint item_drop_max_per_window;
        /// <summary>
        /// Is this item a promotion that is granted manually
        /// </summary>
        [SerializeField]
        internal bool item_granted_manually;
        /// <summary>
        /// Does this item use bundle pricing
        /// </summary>
        [SerializeField]
        internal bool item_use_bundle_price;
        /// <summary>
        /// Does this item automatically stack
        /// </summary>
        [SerializeField]
        internal bool item_auto_stack;
        /// <summary>
        /// Extended item schema values for the item
        /// </summary>
        [SerializeField]
        internal ExtendedSchema item_extendedSchema = new ExtendedSchema();
        /// <summary>
        /// The items internal <see cref="ItemData"/>
        /// </summary>
        public ItemData Data
        { 
            get => id;
            set => id = value;
        }
        /// <summary>
        /// The details related to this item if any
        /// </summary>
        public List<ItemDetail> Details => Data.GetDetails();
        /// <summary>
        /// The total quantity of this item the local user owns
        /// </summary>
        public long TotalQuantity => Data.GetTotalQuantity();
        /// <summary>
        /// This is only useful after calling API.Inventory.Client.LoadItemDefinitions.
        /// This will return the localized name of the item if definitions have been loaded.
        /// </summary>
        /// <remarks>
        /// In general you shouldn't need to use this as you know what items your game has at development time.
        /// It is more performant to hard code the items name than to load it at run time so unless you support adding/changing items without deploying a patch this should never be needed.
        /// </remarks>
        public string DisplayName => Data.Name;
        /// <summary>
        /// Does this item have a valid price
        /// </summary>
        public bool HasPrice => Data.HasPrice;
        /// <summary>
        /// The currency code related to the user
        /// </summary>
        public Currency.Code CurrencyCode => ItemData.CurrencyCode;
        /// <summary>
        /// The currency symbol seen by the user
        /// </summary>
        public string CurrencySymbol => ItemData.CurrencySymbol;
        /// <summary>
        /// The current price of the item as seen by the user, this is base 100 e.g. 150 is $1.50
        /// </summary>
        public ulong CurrentPrice => Data.CurrentPrice;
        /// <summary>
        /// The base price of the item without any applicable discounts, this is base 100 e.g. 150 is $1.50
        /// </summary>
        public ulong BasePrice => Data.BasePrice;
        /// <summary>
        /// An event that is raised when the item experiences a change
        /// </summary>
        public ItemChangedEvent EventChanged
        {
            get
            {
                if (eventChanged == null)
                    eventChanged = new ItemChangedEvent();

                return eventChanged;
            }
        }
        private ItemChangedEvent eventChanged = new ItemChangedEvent();

        #region Internals
        /// <summary>
        /// An internal class which represents the language variants possible in string fields like name and description
        /// </summary>
        [Serializable]
        public class LanguageVariant
        {
            public LanguageCodes language;
            public string value;

            public bool Valid
            {
                get
                {
                    return !string.IsNullOrEmpty(language.ToString()) && !string.IsNullOrEmpty(value.Trim());
                }
            }
        }
        /// <summary>
        /// A collection of <see cref="LanguageVariant"/> entries
        /// </summary>
        [Serializable]
        public class LanguageVariantNode
        {
            [HideInInspector]
            public string node;
            public string value;

            public List<LanguageVariant> variants = new List<LanguageVariant>();

            public string GetSimpleValue()
            {
                if (!string.IsNullOrEmpty(value))
                    return value;
                else if (variants.Count > 0)
                    return variants[0].value;
                else
                    return string.Empty;
            }

            public override string ToString()
            {
                if (variants.Count == 0)
                    return "\t\t\"" + node.Trim() + "\": \"" + value + "\"";
                else
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (var variant in variants)
                    {
                        if (sb.Length > 0)
                            sb.Append(",\n");
                        sb.Append("\t\t\"" + node.Trim() + "_" + variant.language.ToString() + "\": \"" + variant + "\"");
                    }

                    return sb.ToString();
                }
            }

            public void Populate(SteamItemDef_t itemDefId)
            {
                value = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, node);

                if (variants == null)
                    variants = new List<LanguageVariant>();
                else
                    variants.Clear();

                var languages = Enum.GetNames(typeof(LanguageCodes));
                for (int i = 0; i < languages.Length; i++)
                {
                    var r = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, node + "_" + languages[i]);
                    if (!string.IsNullOrEmpty(r))
                    {
                        variants.Add(new LanguageVariant
                        {
                            language = (LanguageCodes)i,
                            value = r,
                        });
                    }
                }
            }
        }
        /// <summary>
        /// An internal class which represents the bundle information of an item
        /// </summary>
        [Serializable]
        public class Bundle
        {
            [Serializable]
            public struct Entry
            {
                public ItemDefinitionObject item;
                public int count;

                public bool Valid
                {
                    get
                    {
                        if (count < 0)
                            return false;
                        if (item == null)
                            return false;
                        if (item.item_type == InventoryItemType.tag_generator)
                            return false;

                        return true;
                    }
                }

                public override string ToString()
                {
                    if (!Valid)
                        return string.Empty;
                    else
                    {
                        if (count > 0)
                            return item.id.ToString() + "x" + count.ToString();
                        else
                            return item.id.ToString();
                    }
                }
            }

            public List<Entry> entries = new List<Entry>();

            public bool Valid
            {
                get
                {
                    if (entries.Count < 1)
                        return false;

                    return !entries.Any(p => !p.Valid);
                }
            }

            public override string ToString()
            {
                if (!Valid)
                    return string.Empty;
                else
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < entries.Count; i++)
                    {
                        if (sb.Length > 0)
                            sb.Append(";");

                        sb.Append(entries[i].ToString());
                    }

                    return sb.ToString();
                }
            }
        }
        /// <summary>
        /// An internal class which represents the promotion rule structure of an item
        /// </summary>
        [Serializable]
        public class PromoRule
        {
            [Serializable]
            public struct PlayedEntry
            {
                public AppId_t app;
                public uint minutes;
            }

            public List<AppId_t> owns = new List<AppId_t>();
            public List<string> achievements = new List<string>();
            public List<PlayedEntry> played = new List<PlayedEntry>();
            public bool manual = false;

            public bool Valid
            {
                get
                {
                    if (owns.Count < 1
                        && achievements.Count < 1
                        && played.Count < 1
                        && !manual)
                        return false;
                    else
                        return true;
                }
            }

            public override string ToString()
            {
                if (!Valid)
                    return string.Empty;

                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < owns.Count; i++)
                {
                    if (sb.Length > 0)
                        sb.Append(";");

                    sb.Append("owns:" + owns[i].m_AppId.ToString());
                }

                for (int i = 0; i < achievements.Count; i++)
                {
                    if (sb.Length > 0)
                        sb.Append(";");

                    sb.Append("ach:" + achievements[i]);
                }

                for (int i = 0; i < played.Count; i++)
                {
                    if (sb.Length > 0)
                        sb.Append(";");

                    sb.Append("played:" + played[i].app.ToString() + "/" + (played[i].minutes < 1 ? "1" : played[i].minutes.ToString()));
                }

                if (manual)
                {
                    if (sb.Length > 0)
                        sb.Append(";");

                    sb.Append("manual");
                }

                return sb.ToString();
            }

            public void Populate(SteamItemDef_t itemDefId)
            {
                owns = new List<AppId_t>();
                achievements = new List<string>();
                played = new List<PlayedEntry>();
                manual = false;

                var results = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "promo");

                if (!string.IsNullOrEmpty(results))
                {
                    var rules = results.Split(';');
                    foreach (var rule in rules)
                    {
                        if (rule.StartsWith("owns:"))
                            owns.Add(new AppId_t(uint.Parse(rule.Replace("owns:", string.Empty))));
                        else if (rule.StartsWith("ach:"))
                            achievements.Add(rule.Replace("ach:", string.Empty));
                        else if (rule.StartsWith("played:"))
                        {
                            var split = rule.Replace("played:", string.Empty).Split('/');
                            if (split.Length > 1)
                                played.Add(new PlayedEntry
                                {
                                    app = new AppId_t(uint.Parse(split[0])),
                                    minutes = uint.Parse(split[1])
                                });
                            else
                                played.Add(new PlayedEntry
                                {
                                    app = new AppId_t(uint.Parse(split[0]))
                                });
                        }
                    }
                }
            }
        }
        /// <summary>
        /// An internal struct which represents an exchange recipe
        /// </summary>
        [Serializable]
        public struct ExchangeRecipe
        {
            [Serializable]
            public struct Material
            {
                [Serializable]
                public struct Item_Def_Descriptor
                {
                    public ItemDefinitionObject item;
                    public uint count;

                    public override string ToString()
                    {
                        if (count > 1)
                        {
                            return item.id.ToString() + "x" + count.ToString();
                        }
                        else
                            return item.id.ToString();
                    }
                }
                [Serializable]
                public struct Item_Tag_Descriptor
                {
                    public string name;
                    public string value;
                    public uint count;

                    public override string ToString()
                    {
                        if (count > 1)
                        {
                            return name + ":" + value + "*" + count.ToString();
                        }
                        else
                            return name + ":" + value;
                    }
                }

                public Item_Def_Descriptor item;
                public Item_Tag_Descriptor tag;

                public override string ToString()
                {
                    if (!Valid)
                        return string.Empty;

                    if (item.item != null && !string.IsNullOrEmpty(tag.name))
                        return item.ToString() + "," + tag.ToString();
                    else if (item.item != null)
                        return item.ToString();
                    else if (!string.IsNullOrEmpty(tag.name))
                        return tag.ToString();
                    else
                        return string.Empty;
                }

                public bool Valid
                {
                    get
                    {
                        //If an item is referenced
                        if (item.item != null)
                        {
                            //Make sure we dont define a tag
                            if (!string.IsNullOrEmpty(tag.name) || !string.IsNullOrEmpty(tag.value) || tag.count != 0)
                            {
                                return false;
                            }
                            else if (item.item.item_type != InventoryItemType.item)
                            {
                                return false;
                            }
                            else if (item.count == 0)
                            {
                                return false;
                            }
                            else
                                return true;
                        }
                        else
                        {
                            //We are not an item reference so we must be a valid tag reference
                            if (string.IsNullOrEmpty(tag.name) || string.IsNullOrEmpty(tag.value) || tag.count == 0)
                            {
                                return false;
                            }
                            else
                                return true;
                        }
                    }
                }
            }

            public List<Material> materials;

            public bool Valid => materials != null && !materials.Any(p => !p.Valid);

            public string GetSchema()
            {
                if (materials.Count > 1)
                {
                    StringBuilder sb = new StringBuilder(materials[0].ToString());
                    for (int i = 1; i < materials.Count; i++)
                    {
                        sb.Append("," + materials[i].ToString());
                    }

                    return sb.ToString();
                }
                else if (materials.Count == 1)
                    return materials[0].ToString();
                else
                    return string.Empty;
            }
        }
        /// <summary>
        /// An internal class which represents the exchange collection of an item
        /// </summary>
        [Serializable]
        public class ExchangeCollection
        {
            public List<ExchangeRecipe> recipe = new List<ExchangeRecipe>();

            public override string ToString()
            {
                if (recipe.Count == 0)
                    return string.Empty;

                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < recipe.Count; i++)
                {
                    if (recipe[i].Valid)
                    {
                        if (sb.Length > 0)
                            sb.Append(";");

                        sb.Append(recipe[i].GetSchema());
                    }
                }

                return sb.ToString();
            }
        }
        /// <summary>
        /// An internal class which represents price
        /// </summary>
        [Serializable]
        public class Price
        {
            [Serializable]
            public class Value
            {
                public string currency;
                public uint value;

                public bool Valid
                {
                    get
                    {
                        if (string.IsNullOrEmpty(currency.Trim()))
                            return false;
                        if (value < 1)
                            return false;

                        return true;
                    }
                }

                public override string ToString()
                {
                    return currency.ToUpper().Trim() + value.ToString();
                }
            }

            [Serializable]
            public class PriceList
            {
                [Serializable]
                public class PriceCollection
                {
                    public List<Value> values = new List<Value>();

                    public bool Valid
                    {
                        get
                        {
                            if (values.Count < 1)
                                return false;

                            return !values.Any(p => !p.Valid);
                        }
                    }

                    public override string ToString()
                    {
                        if (!Valid)
                            return string.Empty;

                        StringBuilder sb = new StringBuilder();

                        for (int i = 0; i < values.Count; i++)
                        {
                            if (sb.Length > 0)
                                sb.Append(",");

                            sb.Append(values[i].ToString());
                        }

                        return sb.ToString();
                    }
                }

                [Serializable]
                public class ChangeCollection
                {
                    public string fromDate;
                    public string untilDate;
                    public PriceCollection prices;

                    public bool Valid
                    {
                        get
                        {
                            if (!Valid)
                                return false;
                            if (string.IsNullOrEmpty(fromDate.Trim()) || string.IsNullOrEmpty(untilDate.Trim()))
                                return false;

                            return true;
                        }
                    }

                    public override string ToString()
                    {
                        if (!Valid)
                            return string.Empty;

                        return fromDate + "-" + untilDate + prices.ToString();
                    }
                }

                public PriceCollection original;
                public List<ChangeCollection> changes = new List<ChangeCollection>();

                public bool Valid
                {
                    get => original.Valid && !changes.Any(p => !p.Valid);
                }

                public override string ToString()
                {
                    if (!Valid)
                        return string.Empty;

                    StringBuilder sb = new StringBuilder();
                    sb.Append(original.ToString());

                    for (int i = 0; i < changes.Count; i++)
                    {
                        sb.Append(";");
                        sb.Append(changes[i]);
                    }

                    return sb.ToString();
                }
            }

            public uint version = 1;
            public bool usePricing = false;
            public bool useCategory = false;
            public ValvePriceCategories category;
            public PriceList priceList = new PriceList();

            public string Node => useCategory ? "price_category" : "price";

            public bool Valid
            {
                get
                {
                    if (useCategory)
                        return true;
                    else
                        return priceList.Valid;
                }
            }

            public override string ToString()
            {
                if (useCategory)
                    return version.ToString() + ";" + category.ToString();
                else
                    return version.ToString() + ";" + priceList.ToString();
            }

            public void Populate(SteamItemDef_t itemDefId)
            {
                var result = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "price_category");
                if (!string.IsNullOrEmpty(result))
                {
                    usePricing = true;
                    useCategory = true;
                    var entries = result.Split(';');
                    if (entries.Length > 1)
                    {
                        version = uint.Parse(entries[0]);
                        category = (ValvePriceCategories)Enum.Parse(typeof(ValvePriceCategories), entries[1]);
                    }
                    else
                    {
                        version = 1;
                        category = (ValvePriceCategories)Enum.Parse(typeof(ValvePriceCategories), result);
                    }
                    priceList = new PriceList();
                }
                else
                {
                    result = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "price");
                    if (!string.IsNullOrEmpty(result))
                    {
                        usePricing = true;
                        useCategory = false;
                        category = ValvePriceCategories.VLV100;
                        priceList = new PriceList();

                        var entries = result.Split(';');
                        version = uint.Parse(entries[0]);
                        priceList.original = new PriceList.PriceCollection();
                        priceList.original.values = new List<Value>();
                        var originalValues = entries[1].Split(',');
                        for (int i = 0; i < originalValues.Length; i++)
                        {
                            var currency = originalValues[i].Substring(0, 3);
                            var value = uint.Parse(originalValues[i].Replace(currency, string.Empty));
                            priceList.original.values.Add(new Value
                            {
                                currency = currency,
                                value = value,
                            });
                        }
                        priceList.changes = new List<PriceList.ChangeCollection>();
                        if (entries.Length > 2)
                        {
                            for (int i = 2; i < entries.Length; i++)
                            {
                                var changeData = new PriceList.ChangeCollection();
                                var change = entries[i].Split(',');
                                var times = change[0].Split('-');
                                changeData.fromDate = times[0];
                                changeData.untilDate = times[1];
                                changeData.prices = new PriceList.PriceCollection();
                                for (int ii = 1; ii < change.Length; ii++)
                                {
                                    var currency = originalValues[i].Substring(0, 3);
                                    var value = uint.Parse(originalValues[i].Replace(currency, string.Empty));
                                    changeData.prices.values.Add(new Value
                                    {
                                        currency = currency,
                                        value = value,
                                    });
                                }
                                priceList.changes.Add(changeData);
                            }
                        }
                    }
                    else
                    {
                        usePricing = false;
                        useCategory = false;
                        category = ValvePriceCategories.VLV100;
                        priceList = new PriceList();
                    }
                }
            }
        }
        /// <summary>
        /// An internal class representing the color related to an item
        /// </summary>
        [Serializable]
        public class Color
        {
            public bool use;
            public UnityEngine.Color color = UnityEngine.Color.black;

            public override string ToString()
            {
                return UnityEngine.ColorUtility.ToHtmlStringRGB(color);
            }
        }
        /// <summary>
        /// An internal class representing the items tags
        /// </summary>
        [Serializable]
        public class TagCollection
        {
            public List<ItemTag> tags = new List<ItemTag>();

            public override string ToString()
            {
                if (tags.Count == 0)
                    return string.Empty;

                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < tags.Count; i++)
                {
                    if (sb.Length > 0)
                        sb.Append(";");

                    sb.Append(tags[i].category.Trim() + ":" + tags[i].tag.Trim());
                }

                return sb.ToString();
            }

            public void Populate(SteamItemDef_t itemDefId)
            {
                tags = new List<ItemTag>();
                var result = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "tags");

                if (!string.IsNullOrEmpty(result))
                {
                    var split1 = result.Split(';');

                    for (int i = 0; i < split1.Length; i++)
                    {
                        var tag = split1[i].Split(':');
                        tags.Add(new ItemTag { category = tag[0], tag = tag[1] });
                    }
                }
            }
        }
        /// <summary>
        /// An internal struct representing the tag generator value
        /// </summary>
        [Serializable]
        public struct TagGeneratorValue
        {
            public string value;
            public uint weight;

            public bool Valid
            {
                get
                {
                    return !string.IsNullOrEmpty(value.Trim());
                }
            }

            public override string ToString()
            {
                return value.Trim() + (weight > 0 ? ":" + weight.ToString() : "");
            }
        }
        /// <summary>
        /// An internal class collection of <see cref="TagGeneratorValue"/>
        /// </summary>
        [Serializable]
        public class TagGeneratorValues
        {
            public List<TagGeneratorValue> values = new List<TagGeneratorValue>();

            public bool Valid
            {
                get
                {
                    return values.Count > 0 && !values.Any(p => !p.Valid);
                }
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < values.Count; i++)
                {
                    if (sb.Length > 0)
                        sb.Append(";");

                    sb.Append(values[i].ToString());
                }

                return sb.ToString();
            }

            public void Populate(SteamItemDef_t itemDefId)
            {
                values = new List<TagGeneratorValue>();
                var result = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "tag_generator_values");

                if (!string.IsNullOrEmpty(result))
                {
                    var split1 = result.Split(';');

                    for (int i = 0; i < split1.Length; i++)
                    {
                        var tagId = split1[i].Split(':');
                        values.Add(new TagGeneratorValue { value = tagId[0], weight = uint.Parse(tagId[1]) });
                    }
                }
            }
        }
        /// <summary>
        /// An internal class object representing the extended schema
        /// </summary>
        [Serializable]
        public class ExtendedSchema
        {
            [Serializable]
            public struct Entry
            {
                public string node;
                public string value;

                public override string ToString()
                {
                    return "\"" + node.Trim() + "\": " + "\"" + value.Trim() + "\"";
                }
            }

            public List<Entry> entries = new List<Entry>();

            public override string ToString()
            {
                if (entries.Count < 1)
                    return string.Empty;

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < entries.Count; i++)
                {
                    if (sb.Length > 0)
                        sb.Append(",\n\t\t");
                    sb.Append(entries[i].ToString());
                }

                return sb.ToString();
            }
        }
        /// <summary>
        /// The inventory item type
        /// </summary>
        public InventoryItemType Type => item_type;        
        /// <summary>
        /// The name of the item
        /// </summary>
        public string Name
        {
            get => item_name.GetSimpleValue();
            set => item_name.value = value;
        }      
        /// <summary>
        /// The description of the item
        /// </summary>
        public string Description => item_description.GetSimpleValue();        
        /// <summary>
        /// The display type of the item
        /// </summary>
        public string DisplayType => item_display_type.GetSimpleValue();
        /// <summary>
        /// The item's ID
        /// </summary>
        public SteamItemDef_t Id
        {
            get => Data;

            set => Data = value;
        }
        /// <summary>
        /// The collection of Bundle entry items
        /// </summary>
        public Bundle.Entry[] BundleEntries => item_bundle.entries.ToArray();       
        /// <summary>
        /// The set of apps the promotion rule checks for ownership
        /// </summary>
        public AppId_t[] PromoRuleOwns => item_promo.owns.ToArray();
        /// <summary>
        /// The promotion rule achievements related to the item
        /// </summary>
        public string[] PromoRuleAchievements => item_promo.achievements.ToArray();
        /// <summary>
        /// The promotion rule played app entries
        /// </summary>
        public PromoRule.PlayedEntry[] PromoRulePlayed => item_promo.played.ToArray();        
        /// <summary>
        /// The drop start time
        /// </summary>
        public string DropStartTime => item_drop_start_time;        
        /// <summary>
        /// The list of exchange recipes
        /// </summary>
        public ExchangeRecipe[] Recipes => item_exchange?.recipe?.ToArray();
        /// <summary>
        /// The background color
        /// </summary>
        public Color BackgroundColor => item_background_color;        
        /// <summary>
        /// The name color
        /// </summary>
        public Color NameColor => item_name_color;        
        /// <summary>
        /// The Icon URL
        /// </summary>
        public string IconUrl => item_icon_url;        
        /// <summary>
        /// The large icon URL
        /// </summary>
        public string IconUrlLarge => item_icon_url_large;        
        /// <summary>
        /// Is the item marketable
        /// </summary>
        public bool Marketable => item_marketable;        
        /// <summary>
        /// Is the item tradable between players
        /// </summary>
        public bool Tradable => item_tradable;        
        /// <summary>
        /// The item tags
        /// </summary>
        public ItemTag[] Tags => item_tags.tags.ToArray();        
        /// <summary>
        /// The related tag generators
        /// </summary>
        public ItemDefinitionObject[] TagGenerators => item_tag_generators.ToArray();        
        /// <summary>
        /// The tag generator name
        /// </summary>
        public string TagGeneratorName => item_tag_generator_name;        
        /// <summary>
        /// Tag generator values
        /// </summary>
        public TagGeneratorValue[] TagGeneratorValueArray => item_tag_generator_values.values.ToArray();        
        /// <summary>
        /// Store tags for the item
        /// </summary>
        public string[] StoreTags => item_store_tags.ToArray();        
        /// <summary>
        /// Additional store images
        /// </summary>
        public string[] StoreImages => item_store_images.ToArray();        
        /// <summary>
        /// Is this item hidden from the player
        /// </summary>
        public bool Hidden => item_hidden;    
        /// <summary>
        /// Is this item hidden from the store
        /// </summary>
        public bool StoreHidden => item_store_hidden;      
        /// <summary>
        /// Does this item use drop limit
        /// </summary>
        public bool UseDropLimit => item_use_drop_limit;        
        /// <summary>
        /// If using a drop limit what is its value
        /// </summary>
        public uint DropLimit => item_drop_limit;  
        /// <summary>
        /// What is the drop interval
        /// </summary>
        public uint DropInterval => item_drop_interval;        
        /// <summary>
        /// Does this item use a drop window
        /// </summary>
        public bool UseDropWindow => item_use_drop_window;        
        /// <summary>
        /// If using a drop window what is the window
        /// </summary>
        public uint DropWindow => item_drop_window;
        /// <summary>
        /// Max drops per window
        /// </summary>
        public uint DropMaxPerWindow => item_drop_max_per_window;
        /// <summary>
        /// Is this item a promo item that can be granted manually
        /// </summary>
        public bool GrantedManually => item_granted_manually;
        /// <summary>
        /// Does this item use bundle pricing
        /// </summary>
        public bool UseBundlePrice => item_use_bundle_price;
        /// <summary>
        /// Does this item automatically stack
        /// </summary>
        public bool AutoStack => item_auto_stack;
        /// <summary>
        /// List of extended schema entries
        /// </summary>
        public ExtendedSchema.Entry[] ExtendedSchemaEntries => item_extendedSchema.entries.ToArray();
        #endregion
        /// <summary>
        /// Is the item valid, this examens the item name, id and type information to check for required fields. It is an estimate not an absolute
        /// </summary>
        public bool Valid
        {
            get
            {
                if (string.IsNullOrEmpty(item_name.ToString().Trim()))
                {
                    UnityEngine.Debug.LogError(name + " ItemDefinition: Name field must be populated");
                    return false;
                }
                if (id >= 1000000)
                {
                    UnityEngine.Debug.LogError(name + " ItemDefinition: ID field must be less than 1,000,000");
                    return false;
                }

                switch (item_type)
                {
                    case InventoryItemType.item:
                        break;
                    case InventoryItemType.bundle:
                        break;
                    case InventoryItemType.generator:
                        break;
                    case InventoryItemType.playtimegenerator:
                        break;
                    case InventoryItemType.tag_generator:
                        if (string.IsNullOrEmpty(item_tag_generator_name.Trim()))
                        {
                            UnityEngine.Debug.LogError(name + " ItemDefinition: Tag Generators must define a tag_generator_name");
                            return false;
                        }
                        if (!item_tag_generator_values.Valid)
                        {
                            UnityEngine.Debug.LogError(name + " ItemDefinition: Tag Generators must have valid tag_generator_values");
                            return false;
                        }
                        break;
                    default:
                        return false;
                }

                return true;
            }
        }
        /// <summary>
        /// Get the JSON ItemDefinition of this item as it is currently configured
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            switch (item_type)
            {
                case InventoryItemType.item:
                    return ItemString();
                case InventoryItemType.bundle:
                    return BundleString();
                case InventoryItemType.generator:
                    return GeneratorString();
                case InventoryItemType.playtimegenerator:
                    return PlaytimeGeneratorString();
                default:
                    return TagGeneratorString();
            }
        }
        private string ItemString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\t{");

            sb.Append("\n\t\t\"itemdefid\": " + id.ToString());
            sb.Append(",\n\t\t\"type\": \"item\"");
            sb.Append(",\n" + item_name.ToString());
            sb.Append(",\n" + item_description.ToString());
            sb.Append(",\n" + item_display_type.ToString());

            var promo = item_promo.ToString();
            if (!string.IsNullOrEmpty(promo))
                sb.Append(",\n\t\t\"promo\": \"" + promo + "\"");

            if (!string.IsNullOrEmpty(item_drop_start_time))
                sb.Append(",\n\t\t\"drop_start_time\": \"" + item_drop_start_time + "\"");

            var exchange = item_exchange.ToString();
            if (!string.IsNullOrEmpty(exchange))
                sb.Append(",\n\t\t\"exchange\": \"" + exchange + "\"");

            if (item_price.usePricing || item_price.useCategory)
            {
                var price = item_price.ToString();
                if (!string.IsNullOrEmpty(price))
                    sb.Append(",\n\t\t\"" + item_price.Node + "\": \"" + price + "\"");
            }

            if (item_background_color.use)
            {
                var bgColor = item_background_color.ToString();
                if (!string.IsNullOrEmpty(bgColor))
                    sb.Append(",\n\t\t\"background_color\": \"" + bgColor + "\"");
            }

            if (item_name_color.use)
            {
                var nColor = item_name_color.ToString();
                if (!string.IsNullOrEmpty(nColor))
                    sb.Append(",\n\t\t\"name_color\": \"" + nColor + "\"");
            }

            if (!string.IsNullOrEmpty(item_icon_url))
                sb.Append(",\n\t\t\"icon_url\": \"" + item_icon_url + "\"");

            if (!string.IsNullOrEmpty(item_icon_url_large))
                sb.Append(",\n\t\t\"icon_url_large\": \"" + item_icon_url_large + "\"");

            sb.Append(",\n\t\t\"marketable\": " + item_marketable.ToString().ToLower());
            sb.Append(",\n\t\t\"tradable\": " + item_tradable.ToString().ToLower());

            var tags = item_tags.ToString();
            if (!string.IsNullOrEmpty(tags))
                sb.Append(",\n\t\t\"tags\": \"" + tags + "\"");

            if (item_tag_generators.Count > 0)
            {
                StringBuilder tg = new StringBuilder();

                for (int i = 0; i < item_tag_generators.Count; i++)
                {
                    if (tg.Length > 0)
                        tg.Append(";");

                    tg.Append(item_tag_generators[i].id);
                }

                sb.Append(",\n\t\t\"tag_generators\": \"" + tg.ToString() + "\"");
            }

            if (item_store_tags.Count > 0)
            {
                StringBuilder tg = new StringBuilder();

                for (int i = 0; i < item_store_tags.Count; i++)
                {
                    if (tg.Length > 0)
                        tg.Append(";");

                    tg.Append(item_store_tags[i]);
                }

                sb.Append(",\n\t\t\"store_tags\": \"" + tg.ToString() + "\"");
            }

            if (item_store_images.Count > 0)
            {
                StringBuilder tg = new StringBuilder();

                for (int i = 0; i < item_store_images.Count; i++)
                {
                    if (tg.Length > 0)
                        tg.Append(";");

                    tg.Append(item_store_images[i]);
                }

                sb.Append(",\n\t\t\"store_images\": \"" + tg.ToString() + "\"");
            }

            sb.Append(",\n\t\t\"hidden\": " + item_hidden.ToString().ToLower());
            sb.Append(",\n\t\t\"store_hidden\": " + item_store_hidden.ToString().ToLower());
            sb.Append(",\n\t\t\"granted_manually\": " + item_granted_manually.ToString().ToLower());
            sb.Append(",\n\t\t\"auto_stack\": " + item_auto_stack.ToString().ToLower());

            var extn = item_extendedSchema.ToString();
            if (!string.IsNullOrEmpty(extn))
                sb.Append(",\n\t\t" + extn);

            sb.Append("\n\t}");

            return sb.ToString();
        }
        private string BundleString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\t{");

            sb.Append("\n\t\t\"itemdefid\": " + id.ToString());
            sb.Append(",\n\t\t\"type\": \"bundle\"");

            var bundle = item_bundle.ToString();
            if (!string.IsNullOrEmpty(bundle))
                sb.Append(",\n\t\t\"bundle\": \"" + bundle + "\"");

            sb.Append(",\n" + item_name.ToString());
            sb.Append(",\n" + item_description.ToString());
            sb.Append(",\n" + item_display_type.ToString());

            var promo = item_promo.ToString();
            if (!string.IsNullOrEmpty(promo))
                sb.Append(",\n\t\t\"promo\": \"" + promo + "\"");

            if (!string.IsNullOrEmpty(item_drop_start_time))
                sb.Append(",\n\t\t\"drop_start_time\": \"" + item_drop_start_time + "\"");

            var exchange = item_exchange.ToString();
            if (!string.IsNullOrEmpty(exchange))
                sb.Append(",\n\t\t\"exchange\": \"" + exchange + "\"");

            if (item_price.usePricing)
            {
                var price = item_price.ToString();
                if (!string.IsNullOrEmpty(price))
                    sb.Append(",\n\t\t\"" + item_price.Node + "\": \"" + price + "\"");
            }

            if (item_background_color.use)
            {
                var bgColor = item_background_color.ToString();
                if (!string.IsNullOrEmpty(bgColor))
                    sb.Append(",\n\t\t\"background_color\": \"" + bgColor + "\"");
            }

            if (item_name_color.use)
            {
                var nColor = item_name_color.ToString();
                if (!string.IsNullOrEmpty(nColor))
                    sb.Append(",\n\t\t\"name_color\": \"" + nColor + "\"");
            }

            if (!string.IsNullOrEmpty(item_icon_url))
                sb.Append(",\n\t\t\"icon_url\": \"" + item_icon_url + "\"");

            if (!string.IsNullOrEmpty(item_icon_url_large))
                sb.Append(",\n\t\t\"icon_url_large\": \"" + item_icon_url_large + "\"");

            if (item_store_tags.Count > 0)
            {
                StringBuilder tg = new StringBuilder();

                for (int i = 0; i < item_store_tags.Count; i++)
                {
                    if (tg.Length > 0)
                        tg.Append(";");

                    tg.Append(item_store_tags[i]);
                }

                sb.Append(",\n\t\t\"store_tags\": \"" + tg.ToString() + "\"");
            }

            if (item_store_images.Count > 0)
            {
                StringBuilder tg = new StringBuilder();

                for (int i = 0; i < item_store_images.Count; i++)
                {
                    if (tg.Length > 0)
                        tg.Append(";");

                    tg.Append(item_store_images[i]);
                }

                sb.Append(",\n\t\t\"store_images\": \"" + tg.ToString() + "\"");
            }

            sb.Append(",\n\t\t\"hidden\": " + item_hidden.ToString().ToLower());
            sb.Append(",\n\t\t\"store_hidden\": " + item_store_hidden.ToString().ToLower());
            sb.Append(",\n\t\t\"granted_manually\": " + item_granted_manually.ToString().ToLower());
            sb.Append(",\n\t\t\"use_bundle_price\": " + item_use_bundle_price.ToString().ToLower());

            var extn = item_extendedSchema.ToString();
            if (!string.IsNullOrEmpty(extn))
                sb.Append(",\n\t\t" + extn);

            sb.Append("\n\t}");

            return sb.ToString();
        }
        private string GeneratorString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\t{");

            sb.Append("\n\t\t\"itemdefid\": " + id.ToString());
            sb.Append(",\n\t\t\"type\": \"generator\"");

            var bundle = item_bundle.ToString();
            if (!string.IsNullOrEmpty(bundle))
                sb.Append(",\n\t\t\"bundle\": \"" + bundle + "\"");
            {
                Debug.LogWarning("The Bundle node is empty for generator '" + this.name + "'; Valve deliberately omits this content when importing items from the Steam API. As such the bundle node is erased every time you import item definitions meaning you will need to manually update this field for every Item Generator every time you Copy the JSON data for upload to Steam.");
            }

            sb.Append(",\n" + item_name.ToString());

            var promo = item_promo.ToString();
            if (!string.IsNullOrEmpty(promo))
                sb.Append(",\n\t\t\"promo\": \"" + promo + "\"");

            if (!string.IsNullOrEmpty(item_drop_start_time))
                sb.Append(",\n\t\t\"drop_start_time\": \"" + item_drop_start_time + "\"");

            var exchange = item_exchange.ToString();
            if (!string.IsNullOrEmpty(exchange))
                sb.Append(",\n\t\t\"exchange\": \"" + exchange + "\"");

            sb.Append(",\n\t\t\"granted_manually\": " + item_granted_manually.ToString().ToLower());

            var extn = item_extendedSchema.ToString();
            if (!string.IsNullOrEmpty(extn))
                sb.Append(",\n\t\t" + extn);

            sb.Append("\n\t}");

            return sb.ToString();
        }
        private string PlaytimeGeneratorString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\t{");

            sb.Append("\n\t\t\"itemdefid\": " + id.ToString());
            sb.Append(",\n\t\t\"type\": \"playtimegenerator\"");

            var bundle = item_bundle.ToString();
            if (!string.IsNullOrEmpty(bundle))
                sb.Append(",\n\t\t\"bundle\": \"" + bundle + "\"");

            sb.Append(",\n" + item_name.ToString());

            var promo = item_promo.ToString();
            if (!string.IsNullOrEmpty(promo))
                sb.Append(",\n\t\t\"promo\": \"" + promo + "\"");

            if (!string.IsNullOrEmpty(item_drop_start_time))
                sb.Append(",\n\t\t\"drop_start_time\": \"" + item_drop_start_time + "\"");

            sb.Append(",\n\t\t\"use_drop_limit\": " + item_use_drop_limit.ToString().ToLower());
            sb.Append(",\n\t\t\"drop_limit\": " + item_use_drop_limit.ToString().ToLower());
            sb.Append(",\n\t\t\"drop_interval\": " + item_use_drop_limit.ToString().ToLower());
            sb.Append(",\n\t\t\"use_drop_window\": " + item_use_drop_limit.ToString().ToLower());
            sb.Append(",\n\t\t\"drop_max_per_window\": " + item_use_drop_limit.ToString().ToLower());

            sb.Append(",\n\t\t\"granted_manually\": " + item_granted_manually.ToString().ToLower());

            var extn = item_extendedSchema.ToString();
            if (!string.IsNullOrEmpty(extn))
                sb.Append(",\n\t\t" + extn);

            sb.Append("\n\t}");

            return sb.ToString();
        }
        private string TagGeneratorString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\t{");

            sb.Append("\n\t\t\"itemdefid\": " + id.ToString());
            sb.Append(",\n\t\t\"type\": \"tag_generator\"");

            sb.Append(",\n" + item_name.ToString());

            if (!string.IsNullOrEmpty(item_tag_generator_name))
                sb.Append(",\n\t\t\"tag_generator_name\": \"" + item_tag_generator_name + "\"");

            var values = item_tag_generator_values.ToString();
            if (!string.IsNullOrEmpty(values))
                sb.Append(",\n\t\t\"tag_generator_values\": \"" + values + "\"");

            sb.Append(",\n\t\t\"granted_manually\": " + item_granted_manually.ToString().ToLower());

            var extn = item_extendedSchema.ToString();
            if (!string.IsNullOrEmpty(extn))
                sb.Append(",\n\t\t" + extn);

            sb.Append("\n\t}");

            return sb.ToString();
        }
        /// <summary>
        /// Grants the item in a one-time promotional to the user
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public bool AddPromoItem(Action<InventoryResult> callback) => Data.AddPromoItem(callback);
        public ConsumeOrder[] GetConsumeOrders(uint quantity) => Data.GetConsumeOrders(quantity);
        /// <summary>
        /// Consume a single instance of this item
        /// </summary>
        /// <returns></returns>
        public bool Consume(Action<InventoryResult> callback) => Data.Consume(callback);
        /// <summary>
        /// Consumes a consume order, you can get consume orders for multiple instances via the <see cref="GetConsumeOrders(uint)"/> method.
        /// </summary>
        /// <param name="order"></param>
        /// <param name="callback"></param>
        public void Consume(ConsumeOrder order, Action<InventoryResult> callback) => Data.Consume(order, callback);
        /// <summary>
        /// Gets a set of exchange entries required to satisfy a given quantity in exchange
        /// </summary>
        /// <param name="quantity"></param>
        /// <param name="entries"></param>
        /// <returns></returns>
        public bool GetExchangeEntry(uint quantity, out ExchangeEntry[] entries) => Data.GetExchangeEntry(quantity, out entries);
        /// <summary>
        /// Requests an exchange of a given set of items for this item
        /// </summary>
        /// <param name="recipeEntries"></param>
        /// <param name="callback"></param>
        public void Exchange(IEnumerable<ExchangeEntry> recipeEntries, Action<InventoryResult> callback) => Data.Exchange(recipeEntries, callback);
        /// <summary>
        /// Generates a single instance of this item
        /// </summary>
        /// <remarks>
        /// This is only available to developers of this Steam Game and cannot be used by regular users
        /// </remarks>
        /// <param name="callback"></param>
        public void GenerateItem(Action<InventoryResult> callback) => Data.GenerateItem(callback);
        /// <summary>
        /// Generates a number of this item
        /// </summary>
        /// <remarks>
        /// This is only available to developers of this Steam Game and cannot be used by regular users
        /// </remarks>
        /// <param name="quantity"></param>
        /// <param name="callback"></param>
        public void GenerateItem(uint quantity, Action<InventoryResult> callback) => Data.GenerateItem(quantity, callback);
        /// <summary>
        /// Starts the purchase process for the user, given a shopping cart of item definitions that the user would like to buy
        /// </summary>
        /// <param name="callback"></param>
        public void StartPurchase(Action<SteamInventoryStartPurchaseResult_t, bool> callback) => Data.StartPurchase(callback);
        /// <summary>
        /// Starts the purchase process for the user, given a shopping cart of item definitions that the user would like to buy
        /// </summary>
        public void StartPurchase(uint count, Action<SteamInventoryStartPurchaseResult_t, bool> callback) => Data.StartPurchase(count, callback);
        /// <summary>
        /// After a successful call to RequestPrices, you can call this method to get the pricing for a specific item definition.
        /// </summary>
        /// <param name="currentPrice"></param>
        /// <param name="basePrice"></param>
        /// <returns></returns>
        public bool GetPrice(out ulong currentPrice, out ulong basePrice) => Data.GetPrice(out currentPrice, out basePrice);
        /// <summary>
        /// Trigger an item drop if the user has played a long enough period of time.
        /// </summary>
        /// <param name="callback"></param>
        public void TriggerDrop(Action<InventoryResult> callback) => Data.TriggerDrop(callback);
        /// <summary>
        /// Builds up a list of ExchangeEntries that can be used with the Exchange method to create this item form a recipe
        /// </summary>
        /// <remarks>
        /// This only works on recipies that only use item references. if the recipe requires a specific tag type it will not resolve
        /// </remarks>
        /// <param name="recipe">The recipe to try and build ... you can see all recipies for an item in the item_exchange.recipe member</param>
        /// <param name="entries"></param>
        /// <returns></returns>
        public bool CanExchange(ItemDefinitionObject.ExchangeRecipe recipe, out List<ExchangeEntry> entries)
        {
            if (!recipe.Valid)
            {
                entries = null;
                Debug.LogWarning("The indicated recipe appears to be invalid and cannot automatically be resolved to an ExchangeEntry list.");
                return false;
            }

            entries = new List<ExchangeEntry>();
            foreach (var mat in recipe.materials)
            {
                if (mat.item.item == null)
                {
                    Debug.LogWarning("We can only build recipies that take specific items. This recipe uses tag types");
                    entries = null;
                    return false;
                }
                else
                {
                    if (mat.item.item.GetExchangeEntry(mat.item.count, out ExchangeEntry[] found))
                    {
                        entries.AddRange(found);
                    }
                    else
                    {
                        Debug.LogWarning("Insufficient quantity of item " + mat.item.item.name);
                        entries = null;
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// This will only work well with currency formats broken into 1/100th e.g. dollars, euro, pounds, etc.
        /// </summary>
        /// <returns></returns>
        public string CurrentPriceString() => Data.CurrentPriceString();

        /// <summary>
        /// This will only work well with currency formats broken into 1/100th e.g. dollars, euro, pounds, etc.
        /// </summary>
        /// <returns></returns>
        public string BasePriceString() => Data.BasePriceString();
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(ItemDefinitionObject))]
    public class ItemDefinitionEditor : UnityEditor.Editor
    {
        private UnityEditor.SerializedProperty item_type;
        private UnityEditor.SerializedProperty item_name;
        private UnityEditor.SerializedProperty item_description;
        private UnityEditor.SerializedProperty item_display_type;
        private UnityEditor.SerializedProperty id;
        private UnityEditor.SerializedProperty item_bundle;
        private UnityEditor.SerializedProperty item_promo;
        private UnityEditor.SerializedProperty item_drop_start_time;
        private UnityEditor.SerializedProperty item_exchange;
        private UnityEditor.SerializedProperty item_price;
        private UnityEditor.SerializedProperty item_background_color;
        private UnityEditor.SerializedProperty item_name_color;
        private UnityEditor.SerializedProperty item_icon_url;
        private UnityEditor.SerializedProperty item_icon_url_large;
        private UnityEditor.SerializedProperty item_marketable;
        private UnityEditor.SerializedProperty item_tradable;
        private UnityEditor.SerializedProperty item_tags;
        private UnityEditor.SerializedProperty item_tag_generators;
        private UnityEditor.SerializedProperty item_tag_generator_name;
        private UnityEditor.SerializedProperty item_tag_generator_values;
        private UnityEditor.SerializedProperty item_store_tags;
        private UnityEditor.SerializedProperty item_store_images;
        private UnityEditor.SerializedProperty item_hidden;
        private UnityEditor.SerializedProperty item_store_hidden;
        private UnityEditor.SerializedProperty item_use_drop_limit;
        private UnityEditor.SerializedProperty item_drop_limit;
        private UnityEditor.SerializedProperty item_drop_interval;
        private UnityEditor.SerializedProperty item_use_drop_window;
        private UnityEditor.SerializedProperty item_drop_window;
        private UnityEditor.SerializedProperty item_drop_max_per_window;
        private UnityEditor.SerializedProperty item_granted_manually;
        private UnityEditor.SerializedProperty item_use_bundle_price;
        private UnityEditor.SerializedProperty item_auto_stack;
        private UnityEditor.SerializedProperty item_extendedSchema;

        private void OnEnable()
        {
            item_type = serializedObject.FindProperty("item_type");
            item_name = serializedObject.FindProperty("item_name");
            item_description = serializedObject.FindProperty("item_description");
            item_display_type = serializedObject.FindProperty("item_display_type");
            id = serializedObject.FindProperty("id");
            item_bundle = serializedObject.FindProperty("item_bundle");
            item_promo = serializedObject.FindProperty("item_promo");
            item_drop_start_time = serializedObject.FindProperty("item_drop_start_time");
            item_exchange = serializedObject.FindProperty("item_exchange");
            item_price = serializedObject.FindProperty("item_price");
            item_background_color = serializedObject.FindProperty("item_background_color");
            item_name_color = serializedObject.FindProperty("item_name_color");
            item_icon_url = serializedObject.FindProperty("icon_url");
            item_icon_url_large = serializedObject.FindProperty("icon_url_large");
            item_marketable = serializedObject.FindProperty("item_marketable");
            item_tradable = serializedObject.FindProperty("item_tradable");
            item_tags = serializedObject.FindProperty("item_tags");
            item_tag_generators = serializedObject.FindProperty("item_tag_generators");
            item_tag_generator_name = serializedObject.FindProperty("item_tag_generator_name");
            item_tag_generator_values = serializedObject.FindProperty("item_tag_generator_values");
            item_store_tags = serializedObject.FindProperty("item_store_tags");
            item_store_images = serializedObject.FindProperty("item_store_images");
            item_hidden = serializedObject.FindProperty("item_hidden");
            item_store_hidden = serializedObject.FindProperty("item_store_hidden");
            item_use_drop_limit = serializedObject.FindProperty("item_use_drop_limit");
            item_drop_limit = serializedObject.FindProperty("item_drop_limit");
            item_drop_interval = serializedObject.FindProperty("item_drop_interval");
            item_use_drop_window = serializedObject.FindProperty("item_use_drop_window");
            item_drop_window = serializedObject.FindProperty("item_drop_window");
            item_drop_max_per_window = serializedObject.FindProperty("item_drop_max_per_window");
            item_granted_manually = serializedObject.FindProperty("item_granted_manually");
            item_use_bundle_price = serializedObject.FindProperty("item_use_bundle_price");
            item_auto_stack = serializedObject.FindProperty("item_auto_stack");
            item_extendedSchema = serializedObject.FindProperty("item_extendedSchema");
        }

        public override void OnInspectorGUI()
        {
            var itemRef = target as ItemDefinitionObject;

            UnityEditor.EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy JSON", UnityEditor.EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                GUI.FocusControl(null);

                StringBuilder sb = new StringBuilder();
                sb.Append("{");
                sb.Append("\t\"appid\": " + SteamSettings.current?.applicationId.ToString());
                sb.Append(",\n\t\"items\": [");
                sb.Append("\n" + itemRef.ToJson());
                sb.Append("\n\t]\n}");

                var n = new TextEditor();
                n.text = sb.ToString();
                n.SelectAll();
                n.Copy();

                Debug.Log("Output copied to clipboard:\n\n" + sb.ToString());
            }
            if (GUILayout.Button("Copy Add Item (Web API URL)", UnityEditor.EditorStyles.toolbarButton, GUILayout.Width(200)))
            {
                GUI.FocusControl(null);
                StringBuilder sb = new StringBuilder();
                var thisObj = target as ItemDefinitionObject;
                sb.Append($"https://partner.steam-api.com/IInventoryService/AddItem/v1/?key=<Go to steamcommunity.com/dev/apikey>&appid={SteamSettings.current?.applicationId}&itemdefid[0]={thisObj.Id}&steamid=<User id to give it to>");

                var n = new TextEditor();
                n.text = sb.ToString();
                n.SelectAll();
                n.Copy();

                Debug.Log($"Copied to clipboard:{sb}");
            }
            UnityEditor.EditorGUILayout.EndHorizontal();

            var objName = "[Inv] " + itemRef.Id.ToString() + " " + itemRef.item_name.GetSimpleValue();
            if (itemRef.name != objName)
            {
                itemRef.name = objName;
                UnityEditor.EditorUtility.SetDirty(itemRef);

                UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(itemRef));

                UnityEditor.EditorGUIUtility.PingObject(itemRef);
                
            }

            UnityEditor.EditorGUILayout.LabelField("Required Settings", UnityEditor.EditorStyles.boldLabel);
            UnityEditor.EditorGUILayout.PropertyField(item_type, new GUIContent("Type", "The type item this should represent"), true);
            UnityEditor.EditorGUILayout.PropertyField(item_name, new GUIContent("Name", "The English name of your item. You can provide localized versions of the name as required"), true);
            UnityEditor.EditorGUILayout.PropertyField(id, new GUIContent("Id", "The ID of this itemdef. This must be lower than 1,000,000 for non-workshop items."), true);

            UnityEditor.EditorGUILayout.Space();
            UnityEditor.EditorGUILayout.LabelField("Configuration Settings", UnityEditor.EditorStyles.boldLabel);
            if (itemRef.item_type != InventoryItemType.tag_generator)
            {
                if (itemRef.item_type == InventoryItemType.bundle)
                    UnityEditor.EditorGUILayout.PropertyField(item_bundle, new GUIContent("Bundle", "The items to include in the bundle"), true);
                else if (itemRef.item_type == InventoryItemType.generator || itemRef.item_type == InventoryItemType.playtimegenerator)
                {
                    UnityEditor.EditorGUILayout.HelpBox("Valve deliberately omits this content when importing items from the Steam API. As such this will be erased every time you import item definitions meaning you will need to manually update this field for every Item Generator every time you Copy the JSON data for upload to Steam.", UnityEditor.MessageType.Warning, true);
                    UnityEditor.EditorGUILayout.PropertyField(item_bundle, new GUIContent("Items", "The items to be generated"), true);
                }

                UnityEditor.EditorGUILayout.PropertyField(item_description, new GUIContent("Description", "The English description of your item. You can provide localized versions of the description as required"), true);

                UnityEditor.EditorGUILayout.PropertyField(item_display_type, new GUIContent("Display Type", "The English description of item's 'type'. You can provide localized versions of the type as required"), true);
                UnityEditor.EditorGUILayout.PropertyField(item_background_color, new GUIContent("Background Color", "The color to display in the inventory background"), true);
                UnityEditor.EditorGUILayout.PropertyField(item_name_color, new GUIContent("Name Color", "The color to display the name in the inventory"), true);
                UnityEditor.EditorGUILayout.PropertyField(item_icon_url, new GUIContent("Icon URL", "The URL to the item's small icon. The URL should be publicly accessible because the Steam servers will download and cache. Recommended size is 200x200."), true);
                UnityEditor.EditorGUILayout.PropertyField(item_icon_url_large, new GUIContent("Large Icon URL", "The URL to the item's large image. The URL should be publicly accessible because the Steam servers will download and cache. Recommended size is 2048x2048."), true);

                
                UnityEditor.EditorGUILayout.PropertyField(item_drop_start_time, new GUIContent("Drop Start Time", "UTC timestamp - prevent promo grants before this time, only applicable when promo = manual"), true);
                UnityEditor.EditorGUILayout.PropertyField(item_exchange, new GUIContent("Exchange", "The recipes of materials that be exchanged for this item"), true);
                UnityEditor.EditorGUILayout.PropertyField(item_tags, new GUIContent("Tags", "The tags assigned to the item"), true);
                UnityEditor.EditorGUILayout.PropertyField(item_tag_generators, new GUIContent("Tag Generators", "Set of tag generators to apply to this item"), true);

                UnityEditor.EditorGUILayout.PropertyField(item_marketable, new GUIContent("Marketable", "Whether this item can be sold to other users in the Steam Community Market."), true);
                UnityEditor.EditorGUILayout.PropertyField(item_tradable, new GUIContent("Tradeable", "Whether this item can be traded to other users using Steam Trading."), true);
                UnityEditor.EditorGUILayout.PropertyField(item_auto_stack, new GUIContent("Auto Stack?", "If true, item grants will automatically be added to a single stack of the given type. Grants will be visible in inventory callbacks as quantity changes."), true);
                UnityEditor.EditorGUILayout.PropertyField(item_hidden, new GUIContent("Hidden?", "If true, the item definition will not be shown to clients. Use this to hide unused, or under-development, itemdefs."), true);

                UnityEditor.EditorGUILayout.Space();
                UnityEditor.EditorGUILayout.LabelField("Promotion Features", UnityEditor.EditorStyles.boldLabel);
                UnityEditor.EditorGUILayout.PropertyField(item_promo, new GUIContent("Promo", "Promotional items can be granted to players based on defined criteria"), true);
                UnityEditor.EditorGUILayout.PropertyField(item_use_drop_limit, new GUIContent("Use Drop Limit?", "If true, then we rely on drop_limit to limit the items grant"), true);
                UnityEditor.EditorGUILayout.PropertyField(item_drop_limit, new GUIContent("Drop Limit", "Limits for a specific user the number of times this item will be dropped"), true);
                UnityEditor.EditorGUILayout.PropertyField(item_drop_interval, new GUIContent("Drop Interval", "Playtime in minutes before the item can be granted to the user."), true);
                UnityEditor.EditorGUILayout.PropertyField(item_use_drop_window, new GUIContent("Use Drop Window?", "If true, we will use the 'drop_window' for this itemdef."), true);
                UnityEditor.EditorGUILayout.PropertyField(item_drop_window, new GUIContent("Drop Window", "Elapsed time in minutes of a cool-down window before we will grant an item."), true);
                UnityEditor.EditorGUILayout.PropertyField(item_drop_max_per_window, new GUIContent("Drop Max per-Window", "Numbers of grants within the window permitted before Cool-down applies. "), true);
                UnityEditor.EditorGUILayout.PropertyField(item_granted_manually, new GUIContent("Granted Manually?", "If true, will only be granted when AddPromoItem() or AddPromoItems() are called with the explicit item definition id. Otherwise, it may be granted via the GrantPromoItems() call."), true);

                if (itemRef.item_type == InventoryItemType.item || itemRef.item_type == InventoryItemType.bundle)
                {
                    UnityEditor.EditorGUILayout.Space();
                    UnityEditor.EditorGUILayout.LabelField("Store Settings", UnityEditor.EditorStyles.boldLabel);
                    UnityEditor.EditorGUILayout.HelpBox("Price is only required for items you wish to sale.\nYou can use Price **or** Price Category but never both.", UnityEditor.MessageType.Warning, true);
                    UnityEditor.EditorGUILayout.PropertyField(item_use_bundle_price, new GUIContent("Use Bundle Price?", "If true the automatic bundle pricing will be overridden with the provided price"), true);
                    UnityEditor.EditorGUILayout.PropertyField(item_price, new GUIContent("Price", "Defines the price of the item"), true);
                    UnityEditor.EditorGUILayout.PropertyField(item_store_tags, new GUIContent("Store Tags", "These tags will be used to categorize/filter items in the Steam item store for your app."), true);
                    UnityEditor.EditorGUILayout.PropertyField(item_store_images, new GUIContent("Store Images", "These images will be proxied and used on the detail page of the Steam item store for your app."), true);
                    UnityEditor.EditorGUILayout.PropertyField(item_store_hidden, new GUIContent("Store Hidden?", "If true, this item will be hidden in the Steam Item Store for your app. By default, any items with a price will be shown in the store."), true);
                }
            }
            else
            {
                UnityEditor.EditorGUILayout.PropertyField(item_tag_generator_name, new GUIContent("Tag Generator Name", "The name of the tag category token"), true);
                UnityEditor.EditorGUILayout.PropertyField(item_tag_generator_values, new GUIContent("Tag Generator Values", "The values and the chance that they will be picked"), true);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
#endif