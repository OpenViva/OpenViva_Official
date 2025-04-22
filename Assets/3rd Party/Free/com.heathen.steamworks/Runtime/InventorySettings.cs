#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public class InventorySettings
    {
        public Currency.Code LocalCurrencyCode => API.Inventory.Client.LocalCurrencyCode;
        public string LocalCurrencySymbol => API.Inventory.Client.LocalCurrencySymbol;
        public List<ItemDefinitionObject> items = new List<ItemDefinitionObject>();
        public bool runTimeUpdateItemDefinitions = false;

        public InventoryChangedEvent EventChanged
        {
            get
            {
                if (eventChanged == null)
                    eventChanged = new InventoryChangedEvent();

                return eventChanged;
            }
        }
        private InventoryChangedEvent eventChanged = new InventoryChangedEvent();

        public void Load()
        {
            API.Inventory.Client.EventSteamInventoryDefinitionUpdate.AddListener(HandleDefinitionLoad);
            API.Inventory.Client.EventSteamInventoryResultReady.AddListener(HandleItemResults);
            if (items.Count > 0)
            {
                API.Inventory.Client.GetAllItems();
                API.Inventory.Client.RequestPrices(null);
            }
        }

#if UNITY_EDITOR
        public void UpdateItemDefinitions(bool debugLog)
        {
            if (debugLog)
                Debug.Log("Update Item Definitions: Start");

            if (API.Inventory.Client.GetItemDefinitionIDs(out SteamItemDef_t[] results))
            {
                if (debugLog)
                    Debug.Log($"Get Item Definition IDs found {results.Length} entries.");

                List<ItemDefinitionObject> bundles = new List<ItemDefinitionObject>();
                Dictionary<ItemDefinitionObject, string> craftable = new Dictionary<ItemDefinitionObject, string>();
                Dictionary<ItemDefinitionObject, string> generators = new Dictionary<ItemDefinitionObject, string>();

                for (int i = 0; i < results.Length; i++)
                {
                    if (debugLog)
                        Debug.Log($"Processing Index {i}, ID = {results[i].m_SteamItemDef}");

                    try
                    {
                        var itemDefId = results[i];
                        var target = items.FirstOrDefault(p => p.id == itemDefId.m_SteamItemDef);
                        var created = false;
                        if (target == null)
                        {
                            if (debugLog)
                                Debug.Log($"New Item Definition Object required");
                            created = true;
                            target = ScriptableObject.CreateInstance<ItemDefinitionObject>();
                        }
                        else if (debugLog)
                            Debug.Log($"Item Definition Object {target.name} found");

                        target.id = itemDefId.m_SteamItemDef;

                        target.item_name.Populate(itemDefId);

                        var namePart = "";

                        if (!string.IsNullOrEmpty(target.item_name.value))
                            namePart = itemDefId.ToString() + " " + target.item_name.value;
                        else if (target.item_name.variants.Count > 0)
                            namePart = itemDefId.ToString() + " " + target.item_name.variants[0].value;
                        else
                            namePart = itemDefId.ToString() + " UNKNOWN";

                        target.name = "[Inv] " + namePart;

                        target.item_description.Populate(itemDefId);
                        target.item_display_type.Populate(itemDefId);

                        var type = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "type");
                        switch (type)
                        {
                            case "item":
                                target.item_type = InventoryItemType.item;
                                break;
                            case "bundle":
                                target.item_type = InventoryItemType.bundle;
                                bundles.Add(target);
                                break;
                            case "generator":
                                target.item_type = InventoryItemType.generator;
                                bundles.Add(target);
                                Debug.LogWarning("Importing an Item Generator from Steam API ...\nValve deliberately omits the bundle node when importing items from the Steam API. As such the item pool (aka Items, aka Bundle) of this generator will be blank. You must manually reset this value before you export the JSON otherwise the generator will have an empty bundle.\nPlease let Valve know that this is a problem for you and that you would like to see this changed. This is a limitation from Valve and not something Heathen can effect.");
                                break;
                            case "playtimegenerator":
                                target.item_type = InventoryItemType.playtimegenerator;
                                bundles.Add(target);
                                break;
                            case "tag_generator":
                                target.item_type = InventoryItemType.tag_generator;
                                break;
                            default:
                                UnityEngine.Debug.LogWarning("Unknown Item Type: " + target.name);
                                break;
                        }

                        target.item_promo.Populate(itemDefId);
                        target.item_drop_start_time = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "drop_start_time");

                        var excahnge = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "exchange");
                        if (!string.IsNullOrEmpty(excahnge) && !craftable.ContainsKey(target))
                            craftable.Add(target, excahnge);

                        target.item_price.Populate(itemDefId);

                        var bgColor = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "background_color");
                        if (!string.IsNullOrEmpty(bgColor))
                        {
                            if (UnityEngine.ColorUtility.TryParseHtmlString(bgColor, out Color color))
                                target.item_background_color.color = color;
                            else
                                target.item_background_color.use = false;
                        }
                        else
                            target.item_background_color.use = false;

                        var nColor = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "name_color");
                        if (!string.IsNullOrEmpty(nColor))
                        {
                            if (UnityEngine.ColorUtility.TryParseHtmlString(nColor, out Color color))
                                target.item_name_color.color = color;
                            else
                                target.item_name_color.use = false;
                        }
                        else
                            target.item_name_color.use = false;

                        target.item_icon_url = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "icon_url");
                        target.item_icon_url_large = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "icon_url_large");

                        var returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "marketable");
                        if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool marketable))
                            target.item_marketable = marketable;

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "tradable");
                        if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool tradable))
                            target.item_tradable = tradable;

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "tag_generators");
                        if (!string.IsNullOrEmpty(returnString))
                            generators.Add(target, returnString);

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "store_hidden");
                        if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool store_hidden))
                            target.item_store_hidden = store_hidden;

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "use_drop_limit");
                        if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool use_drop_limit))
                            target.item_use_drop_limit = use_drop_limit;

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "use_drop_window");
                        if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_use_drop_window))
                            target.item_tradable = item_use_drop_window;

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "granted_manually");
                        if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_granted_manually))
                            target.item_tradable = item_granted_manually;

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "use_bundle_price");
                        if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_use_bundle_price))
                            target.item_tradable = item_use_bundle_price;

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "auto_stack");
                        if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_auto_stack))
                            target.item_tradable = item_auto_stack;

                        target.item_tag_generator_name = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "tag_generator_name");
                        target.item_tag_generator_values.Populate(itemDefId);
                        target.item_tags.Populate(itemDefId);

                        target.item_store_tags = new List<string>(API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "store_tags").Split(';'));
                        target.item_store_images = new List<string>(API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "store_images").Split(';'));

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "tradabitem_drop_limitle");
                        if (!string.IsNullOrEmpty(returnString) && uint.TryParse(returnString, out uint item_drop_limit))
                            target.item_drop_limit = item_drop_limit;

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "item_drop_interval");
                        if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_drop_interval))
                            target.item_tradable = item_drop_interval;

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "item_drop_window");
                        if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_drop_window))
                            target.item_tradable = item_drop_window;

                        returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "item_drop_max_per_window");
                        if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_drop_max_per_window))
                            target.item_tradable = item_drop_max_per_window;

                        //TODO: check for and parse extended properties

                        if (created)
                        {
                            if (debugLog)
                                Debug.Log($"Adding new Item Definition Object: {target.name}, to Steam Settings");
                            items.Add(target);
#if UNITY_EDITOR
                            UnityEditor.AssetDatabase.AddObjectToAsset(target, SteamSettings.current);
#endif
                        }
                        else if(debugLog)
                                Debug.Log($"Updating Item Definition Object: {target.name}");
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(target);
                        UnityEditor.EditorUtility.SetDirty(SteamSettings.current);
                        UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(SteamSettings.current));
                        UnityEditor.EditorUtility.SetDirty(target);
#endif

                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Failed to parse item definition load from Valve: " + ex.Message);
                    }
                }

                if (debugLog)
                    Debug.Log($"Processing {bundles.Count} bundles");

                for (int i = 0; i < bundles.Count; i++)
                {
                    try
                    {
                        var target = bundles[i];
                        target.item_bundle.entries = new List<ItemDefinitionObject.Bundle.Entry>();
                        var bundleData = API.Inventory.Client.GetItemDefinitionProperty(target.Id, "bundle");
                        if (!string.IsNullOrEmpty(bundleData))
                        {
                            var bundleString = bundleData.Split(';');
                            for (int ii = 0; ii < bundleString.Length; ii++)
                            {
                                var bundleItem = bundleString[ii];
                                if (bundleItem.Contains("x"))
                                {
                                    var kvp = bundleItem.Split('x');
                                    var id = int.Parse(kvp[0]);
                                    var count = int.Parse(kvp[1]);
                                    target.item_bundle.entries.Add(new ItemDefinitionObject.Bundle.Entry
                                    {
                                        count = count,
                                        item = items.FirstOrDefault(p => p.Id.m_SteamItemDef == id)
                                    });
                                }
                                else
                                {
                                    var id = int.Parse(bundleItem);
                                    target.item_bundle.entries.Add(new ItemDefinitionObject.Bundle.Entry
                                    {
                                        count = 1,
                                        item = items.FirstOrDefault(p => p.Id.m_SteamItemDef == id)
                                    });
                                }
                            }
                        }

#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(target);
                        UnityEditor.EditorUtility.SetDirty(SteamSettings.current);
                        UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(SteamSettings.current));
                        UnityEditor.EditorUtility.SetDirty(target);
#endif
                    }
                    catch (Exception ex)
                    {
#pragma warning disable UNT0008 // Null propagation on Unity objects
                        Debug.LogError("Failed to process bundle information for " + bundles[i]?.ToString() + "\nException: " + ex.Message);
#pragma warning restore UNT0008 // Null propagation on Unity objects
                    }
                }

                if (debugLog)
                    Debug.Log($"Processing {craftable.Count} craftable");

                foreach (var keyValuePair in craftable)
                {

                    var target = keyValuePair.Key;
                    var schema = keyValuePair.Value;
                    try
                    {
                        target.item_exchange = new ItemDefinitionObject.ExchangeCollection();
                        target.item_exchange.recipe = new List<ItemDefinitionObject.ExchangeRecipe>();
                        var recipies = schema.Split(';');
                        foreach (var recipe in recipies)
                        {
                            var recipieObject = new ItemDefinitionObject.ExchangeRecipe();
                            var materials = recipe.Split(',');
                            foreach (var material in materials)
                            {
                                if (material.Contains(":"))
                                {
                                    //Tag
                                    var tagCat = material.Split(':');
                                    var catigory = tagCat[0];
                                    var tag = string.Empty;
                                    uint count = 1;
                                    if (tagCat[1].Contains("*"))
                                    {
                                        var tagCount = tagCat[1].Split('*');
                                        tag = tagCount[0];
                                        count = uint.Parse(tagCount[1]);
                                    }
                                    else
                                        tag = tagCat[1];

                                    if (recipieObject.materials == null)
                                        recipieObject.materials = new List<ItemDefinitionObject.ExchangeRecipe.Material>();

                                    recipieObject.materials.Add(new ItemDefinitionObject.ExchangeRecipe.Material
                                    {
                                        item = new ItemDefinitionObject.ExchangeRecipe.Material.Item_Def_Descriptor { item = null, count = 0 },
                                        tag = new ItemDefinitionObject.ExchangeRecipe.Material.Item_Tag_Descriptor
                                        {
                                            name = catigory,
                                            value = tag,
                                            count = count
                                        }
                                    });
                                }
                                else
                                {
                                    //Item
                                    if (material.Contains("x"))
                                    {
                                        //Has count
                                        var itemCount = material.Split('x');
                                        var itemID = int.Parse(itemCount[0]);
                                        var count = uint.Parse(itemCount[1]);
                                        var itemTarget = items.FirstOrDefault(p => p.Id.m_SteamItemDef == itemID);

                                        if (recipieObject.materials == null)
                                            recipieObject.materials = new List<ItemDefinitionObject.ExchangeRecipe.Material>();

                                        recipieObject.materials.Add(new ItemDefinitionObject.ExchangeRecipe.Material
                                        {
                                            item = new ItemDefinitionObject.ExchangeRecipe.Material.Item_Def_Descriptor
                                            {
                                                item = itemTarget,
                                                count = count
                                            },
                                            tag = new ItemDefinitionObject.ExchangeRecipe.Material.Item_Tag_Descriptor
                                            {
                                                name = string.Empty,
                                                value = string.Empty,
                                                count = 0
                                            }
                                        });
                                    }
                                    else
                                    {
                                        var itemID = int.Parse(material);
                                        var itemTarget = items.FirstOrDefault(p => p.Id.m_SteamItemDef == itemID);

                                        if (recipieObject.materials == null)
                                            recipieObject.materials = new List<ItemDefinitionObject.ExchangeRecipe.Material>();

                                        recipieObject.materials.Add(new ItemDefinitionObject.ExchangeRecipe.Material
                                        {
                                            item = new ItemDefinitionObject.ExchangeRecipe.Material.Item_Def_Descriptor
                                            {
                                                item = itemTarget,
                                                count = 1
                                            },
                                            tag = new ItemDefinitionObject.ExchangeRecipe.Material.Item_Tag_Descriptor
                                            {
                                                name = string.Empty,
                                                value = string.Empty,
                                                count = 0
                                            }
                                        });
                                    }
                                }
                            }

                            target.item_exchange.recipe.Add(recipieObject);
                        }
                    }
                    catch (Exception ex)
                    {
#pragma warning disable UNT0008 // Null propagation on Unity objects
                        Debug.LogError("Failed to parse excahnge schema for " + target?.ToString() + "; schema = " + schema + "; \n Exception = " + ex.Message);
#pragma warning restore UNT0008 // Null propagation on Unity objects
                    }
                }

                if (debugLog)
                    Debug.Log($"Processing {generators.Count} generators");

                foreach (var keyValuePair in generators)
                {
                    var target = keyValuePair.Key;
                    var schema = keyValuePair.Value;

                    if (schema.Contains(";"))
                    {
                        target.item_tag_generators = new List<ItemDefinitionObject>();
                        var gens = schema.Split(';');
                        foreach (var idString in gens)
                        {
                            var id = int.Parse(idString);
                            var targetTagGen = items.FirstOrDefault(p => p.Id.m_SteamItemDef == id);
                            if (targetTagGen != null)
                            {
                                target.item_tag_generators.Add(targetTagGen);
                            }
                        }
                    }
                    else
                    {
                        // Only 1
                        var id = int.Parse(schema);
                        target.item_tag_generators = new List<ItemDefinitionObject>();
                        var targetTagGen = items.FirstOrDefault(p => p.Id.m_SteamItemDef == id);
                        if (targetTagGen != null)
                        {
                            target.item_tag_generators.Add(targetTagGen);
                        }
                    }
                }

#if !UNITY_EDITOR
                API.Inventory.Client.GetAllItems(HandleItemResults);
#endif
            }
        }
#endif

        private void HandleDefinitionLoad()
        {
            SteamSettings.behaviour.StartCoroutine(AsyncDefinitionLoad());
        }

        private IEnumerator AsyncDefinitionLoad()
        {
            if (runTimeUpdateItemDefinitions)
            {
                yield return null;

                if (API.Inventory.Client.GetItemDefinitionIDs(out SteamItemDef_t[] results))
                {
                    List<ItemDefinitionObject> bundles = new List<ItemDefinitionObject>();
                    Dictionary<ItemDefinitionObject, string> craftable = new Dictionary<ItemDefinitionObject, string>();
                    Dictionary<ItemDefinitionObject, string> generators = new Dictionary<ItemDefinitionObject, string>();

                    var counter = 0;

                    for (int i = 0; i < results.Length; i++)
                    {
                        counter++;
                        if (counter > 100)
                        {
                            counter = 0;
                            yield return new WaitForEndOfFrame();
                        }

                        try
                        {
                            var itemDefId = results[i];
                            var target = items.FirstOrDefault(p => p.id == itemDefId.m_SteamItemDef);
                            var created = false;
                            if (target == null)
                            {
                                created = true;
                                target = ScriptableObject.CreateInstance<ItemDefinitionObject>();
                            }

                            target.id = itemDefId.m_SteamItemDef;

                            target.item_name.Populate(itemDefId);

                            var namePart = "";

                            if (!string.IsNullOrEmpty(target.item_name.value))
                                namePart = itemDefId.ToString() + " " + target.item_name.value;
                            else if (target.item_name.variants.Count > 0)
                                namePart = itemDefId.ToString() + " " + target.item_name.variants[0].value;
                            else
                                namePart = itemDefId.ToString() + " UNKNOWN";

                            target.name = "[Inv] " + namePart;

                            target.item_description.Populate(itemDefId);
                            target.item_display_type.Populate(itemDefId);

                            var type = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "type");
                            switch (type)
                            {
                                case "item":
                                    target.item_type = InventoryItemType.item;
                                    break;
                                case "bundle":
                                    target.item_type = InventoryItemType.bundle;
                                    bundles.Add(target);
                                    break;
                                case "generator":
                                    target.item_type = InventoryItemType.generator;
                                    bundles.Add(target);
                                    Debug.LogWarning("Importing an Item Generator from Steam API ...\nValve deliberately omits the bundle node when importing items from the Steam API. As such the item pool (aka Items, aka Bundle) of this generator will be blank. You must manually reset this value before you export the JSON otherwise the generator will have an empty bundle.\nPlease let Valve know that this is a problem for you and that you would like to see this changed. This is a limitation from Valve and not something Heathen can effect.");
                                    break;
                                case "playtimegenerator":
                                    target.item_type = InventoryItemType.playtimegenerator;
                                    bundles.Add(target);
                                    break;
                                case "tag_generator":
                                    target.item_type = InventoryItemType.tag_generator;
                                    break;
                                default:
                                    UnityEngine.Debug.LogWarning("Unknown Item Type: " + target.name);
                                    break;
                            }

                            target.item_promo.Populate(itemDefId);
                            target.item_drop_start_time = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "drop_start_time");

                            var excahnge = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "exchange");
                            if (!string.IsNullOrEmpty(excahnge) && !craftable.ContainsKey(target))
                                craftable.Add(target, excahnge);

                            target.item_price.Populate(itemDefId);

                            var bgColor = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "background_color");
                            if (!string.IsNullOrEmpty(bgColor))
                            {
                                if (UnityEngine.ColorUtility.TryParseHtmlString(bgColor, out Color color))
                                    target.item_background_color.color = color;
                                else
                                    target.item_background_color.use = false;
                            }
                            else
                                target.item_background_color.use = false;

                            var nColor = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "name_color");
                            if (!string.IsNullOrEmpty(nColor))
                            {
                                if (UnityEngine.ColorUtility.TryParseHtmlString(nColor, out Color color))
                                    target.item_name_color.color = color;
                                else
                                    target.item_name_color.use = false;
                            }
                            else
                                target.item_name_color.use = false;

                            target.item_icon_url = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "icon_url");
                            target.item_icon_url_large = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "icon_url_large");

                            var returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "marketable");
                            if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool marketable))
                                target.item_marketable = marketable;

                            returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "tradable");
                            if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool tradable))
                                target.item_tradable = tradable;

                            returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "tag_generators");
                            if (!string.IsNullOrEmpty(returnString))
                                generators.Add(target, returnString);

                            returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "store_hidden");
                            if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool store_hidden))
                                target.item_store_hidden = store_hidden;

                            returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "use_drop_limit");
                            if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool use_drop_limit))
                                target.item_use_drop_limit = use_drop_limit;

                            returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "use_drop_window");
                            if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_use_drop_window))
                                target.item_tradable = item_use_drop_window;

                            returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "granted_manually");
                            if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_granted_manually))
                                target.item_tradable = item_granted_manually;

                            returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "use_bundle_price");
                            if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_use_bundle_price))
                                target.item_tradable = item_use_bundle_price;

                            returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "auto_stack");
                            if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_auto_stack))
                                target.item_tradable = item_auto_stack;

                            target.item_tag_generator_name = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "tag_generator_name");
                            target.item_tag_generator_values.Populate(itemDefId);
                            target.item_tags.Populate(itemDefId);

                            target.item_store_tags = new List<string>(API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "store_tags").Split(';'));
                            target.item_store_images = new List<string>(API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "store_images").Split(';'));

                            returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "tradabitem_drop_limitle");
                            if (!string.IsNullOrEmpty(returnString) && uint.TryParse(returnString, out uint item_drop_limit))
                                target.item_drop_limit = item_drop_limit;

                            returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "item_drop_interval");
                            if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_drop_interval))
                                target.item_tradable = item_drop_interval;

                            returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "item_drop_window");
                            if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_drop_window))
                                target.item_tradable = item_drop_window;

                            returnString = API.Inventory.Client.GetItemDefinitionProperty(itemDefId, "item_drop_max_per_window");
                            if (!string.IsNullOrEmpty(returnString) && bool.TryParse(returnString, out bool item_drop_max_per_window))
                                target.item_tradable = item_drop_max_per_window;

                            //TODO: check for and parse extended properties

                            if (created)
                            {
                                items.Add(target);
#if UNITY_EDITOR
                                UnityEditor.AssetDatabase.AddObjectToAsset(target, SteamSettings.current);
#endif
                            }
#if UNITY_EDITOR
                            UnityEditor.EditorUtility.SetDirty(target);
                            UnityEditor.EditorUtility.SetDirty(SteamSettings.current);
                            UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(SteamSettings.current));
                            UnityEditor.EditorUtility.SetDirty(target);
#endif

                        }
                        catch (Exception ex)
                        {
                            Debug.LogError("Failed to parse item definition load from Valve: " + ex.Message);
                        }
                    }

                    counter = 0;

                    for (int i = 0; i < bundles.Count; i++)
                    {
                        counter++;
                        if (counter > 100)
                        {
                            counter = 0;
                            yield return new WaitForEndOfFrame();
                        }

                        try
                        {
                            var target = bundles[i];
                            target.item_bundle.entries = new List<ItemDefinitionObject.Bundle.Entry>();
                            var bundleData = API.Inventory.Client.GetItemDefinitionProperty(target.Id, "bundle");
                            if (!string.IsNullOrEmpty(bundleData))
                            {
                                var bundleString = bundleData.Split(';');
                                for (int ii = 0; ii < bundleString.Length; ii++)
                                {
                                    var bundleItem = bundleString[ii];
                                    if (bundleItem.Contains("x"))
                                    {
                                        var kvp = bundleItem.Split('x');
                                        var id = int.Parse(kvp[0]);
                                        var count = int.Parse(kvp[1]);
                                        target.item_bundle.entries.Add(new ItemDefinitionObject.Bundle.Entry
                                        {
                                            count = count,
                                            item = items.FirstOrDefault(p => p.Id.m_SteamItemDef == id)
                                        });
                                    }
                                    else
                                    {
                                        var id = int.Parse(bundleItem);
                                        target.item_bundle.entries.Add(new ItemDefinitionObject.Bundle.Entry
                                        {
                                            count = 1,
                                            item = items.FirstOrDefault(p => p.Id.m_SteamItemDef == id)
                                        });
                                    }
                                }
                            }

#if UNITY_EDITOR
                            UnityEditor.EditorUtility.SetDirty(target);
                            UnityEditor.EditorUtility.SetDirty(SteamSettings.current);
                            UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(SteamSettings.current));
                            UnityEditor.EditorUtility.SetDirty(target);
#endif
                        }
                        catch (Exception ex)
                        {
#pragma warning disable UNT0008 // Null propagation on Unity objects
                            Debug.LogError("Failed to process bundle information for " + bundles[i]?.ToString() + "\nException: " + ex.Message);
#pragma warning restore UNT0008 // Null propagation on Unity objects
                        }
                    }

                    foreach (var keyValuePair in craftable)
                    {

                        var target = keyValuePair.Key;
                        var schema = keyValuePair.Value;
                        try
                        {
                            target.item_exchange = new ItemDefinitionObject.ExchangeCollection();
                            target.item_exchange.recipe = new List<ItemDefinitionObject.ExchangeRecipe>();
                            var recipies = schema.Split(';');
                            foreach (var recipe in recipies)
                            {
                                var recipieObject = new ItemDefinitionObject.ExchangeRecipe();
                                var materials = recipe.Split(',');
                                foreach (var material in materials)
                                {
                                    if (material.Contains(":"))
                                    {
                                        //Tag
                                        var tagCat = material.Split(':');
                                        var catigory = tagCat[0];
                                        var tag = string.Empty;
                                        uint count = 1;
                                        if (tagCat[1].Contains("*"))
                                        {
                                            var tagCount = tagCat[1].Split('*');
                                            tag = tagCount[0];
                                            count = uint.Parse(tagCount[1]);
                                        }
                                        else
                                            tag = tagCat[1];

                                        if (recipieObject.materials == null)
                                            recipieObject.materials = new List<ItemDefinitionObject.ExchangeRecipe.Material>();

                                        recipieObject.materials.Add(new ItemDefinitionObject.ExchangeRecipe.Material
                                        {
                                            item = new ItemDefinitionObject.ExchangeRecipe.Material.Item_Def_Descriptor { item = null, count = 0 },
                                            tag = new ItemDefinitionObject.ExchangeRecipe.Material.Item_Tag_Descriptor
                                            {
                                                name = catigory,
                                                value = tag,
                                                count = count
                                            }
                                        });
                                    }
                                    else
                                    {
                                        //Item
                                        if (material.Contains("x"))
                                        {
                                            //Has count
                                            var itemCount = material.Split('x');
                                            var itemID = int.Parse(itemCount[0]);
                                            var count = uint.Parse(itemCount[1]);
                                            var itemTarget = items.FirstOrDefault(p => p.Id.m_SteamItemDef == itemID);

                                            if (recipieObject.materials == null)
                                                recipieObject.materials = new List<ItemDefinitionObject.ExchangeRecipe.Material>();

                                            recipieObject.materials.Add(new ItemDefinitionObject.ExchangeRecipe.Material
                                            {
                                                item = new ItemDefinitionObject.ExchangeRecipe.Material.Item_Def_Descriptor
                                                {
                                                    item = itemTarget,
                                                    count = count
                                                },
                                                tag = new ItemDefinitionObject.ExchangeRecipe.Material.Item_Tag_Descriptor
                                                {
                                                    name = string.Empty,
                                                    value = string.Empty,
                                                    count = 0
                                                }
                                            });
                                        }
                                        else
                                        {
                                            var itemID = int.Parse(material);
                                            var itemTarget = items.FirstOrDefault(p => p.Id.m_SteamItemDef == itemID);

                                            if (recipieObject.materials == null)
                                                recipieObject.materials = new List<ItemDefinitionObject.ExchangeRecipe.Material>();

                                            recipieObject.materials.Add(new ItemDefinitionObject.ExchangeRecipe.Material
                                            {
                                                item = new ItemDefinitionObject.ExchangeRecipe.Material.Item_Def_Descriptor
                                                {
                                                    item = itemTarget,
                                                    count = 1
                                                },
                                                tag = new ItemDefinitionObject.ExchangeRecipe.Material.Item_Tag_Descriptor
                                                {
                                                    name = string.Empty,
                                                    value = string.Empty,
                                                    count = 0
                                                }
                                            });
                                        }
                                    }
                                }

                                target.item_exchange.recipe.Add(recipieObject);
                            }
                        }
                        catch (Exception ex)
                        {
#pragma warning disable UNT0008 // Null propagation on Unity objects
                            Debug.LogError("Failed to parse excahnge schema for " + target?.ToString() + "; schema = " + schema + "; \n Exception = " + ex.Message);
#pragma warning restore UNT0008 // Null propagation on Unity objects
                        }
                    }

                    foreach (var keyValuePair in generators)
                    {
                        var target = keyValuePair.Key;
                        var schema = keyValuePair.Value;

                        if (schema.Contains(";"))
                        {
                            target.item_tag_generators = new List<ItemDefinitionObject>();
                            var gens = schema.Split(';');
                            foreach (var idString in gens)
                            {
                                var id = int.Parse(idString);
                                var targetTagGen = items.FirstOrDefault(p => p.Id.m_SteamItemDef == id);
                                if (targetTagGen != null)
                                {
                                    target.item_tag_generators.Add(targetTagGen);
                                }
                            }
                        }
                        else
                        {
                            // Only 1
                            var id = int.Parse(schema);
                            target.item_tag_generators = new List<ItemDefinitionObject>();
                            var targetTagGen = items.FirstOrDefault(p => p.Id.m_SteamItemDef == id);
                            if (targetTagGen != null)
                            {
                                target.item_tag_generators.Add(targetTagGen);
                            }
                        }
                    }

#if !UNITY_EDITOR
                API.Inventory.Client.GetAllItems(HandleItemResults);
#endif
                }
            }
        }

        private void HandleItemResults(InventoryResult results)
        {
            //Create the existing record
            Dictionary<ItemDefinitionObject, List<ItemDetail>> currentState = new Dictionary<ItemDefinitionObject, List<ItemDetail>>();
            Dictionary<ItemDefinitionObject, List<ItemDetail>> newState = new Dictionary<ItemDefinitionObject, List<ItemDetail>>();

            foreach (var item in items)
            {
                currentState.Add(item, new List<ItemDetail>(item.Details.ToArray()));
            }

            foreach (var detail in results.items)
            {
                var def = items.FirstOrDefault(p => p.id == detail.Definition.id);
                if (def != null)
                {
                    var itemDetails = def.Details;
                    itemDetails.RemoveAll(p => p.ItemId == detail.ItemId);
                    itemDetails.Add(detail);
                }
            }

            foreach (var item in items)
            {
                newState.Add(item, new List<ItemDetail>(item.Details.ToArray()));
            }

            //Compare
            List<ItemChangeRecord> changes = new List<ItemChangeRecord>();

            foreach (var kvp in currentState)
            {
                var item = kvp.Key;
                var before = kvp.Value;
                var after = newState[item];

                //Was in before is not in after
                var removed = before.Where(b => !after.Any(a => a.ItemId == b.ItemId));

                //Is in after was not in before
                var added = after.Where(a => !before.Any(b => b.ItemId == a.ItemId));

                //Is in both but count doesn't match
                var bChange = before.Where(b => after.Any(a => a.ItemId == b.ItemId) && after.FirstOrDefault(a => a.ItemId == b.ItemId).Quantity != b.Quantity);
                var aChange = after.Where(a => before.Any(b => b.ItemId == a.ItemId) && before.FirstOrDefault(b => b.ItemId == a.ItemId).Quantity != a.Quantity);

                if (removed.Count() > 0
                    || added.Count() > 0
                    || bChange.Count() > 0)
                {
                    //We have some change so record it
                    List<ItemInstanceChangeRecord> changeRecords = new List<ItemInstanceChangeRecord>();

                    foreach (var r in removed)
                    {
                        changeRecords.Add(new ItemInstanceChangeRecord
                        {
                            added = false,
                            changed = false,
                            removed = true,
                            quantityBefore = r.Quantity,
                            quantityAfter = 0,
                            instance = r.ItemId
                        });
                    }

                    foreach (var r in added)
                    {
                        changeRecords.Add(new ItemInstanceChangeRecord
                        {
                            added = true,
                            changed = false,
                            removed = false,
                            quantityBefore = 0,
                            quantityAfter = r.Quantity,
                            instance = r.ItemId
                        });
                    }

                    foreach (var r in bChange)
                    {
                        var match = aChange.FirstOrDefault(a => a.ItemId == r.ItemId);

                        changeRecords.Add(new ItemInstanceChangeRecord
                        {
                            added = false,
                            changed = true,
                            removed = false,
                            quantityBefore = r.Quantity,
                            quantityAfter = match.Quantity,
                            instance = match.ItemId
                        });
                    }

                    var iChanged = new ItemChangeRecord
                    {
                        item = item,
                        changes = changeRecords.ToArray()
                    };

                    changes.Add(iChanged);

                    try
                    {
                        iChanged.item.EventChanged.Invoke(iChanged);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }

            if (changes.Count > 0)
            {
                EventChanged.Invoke(new InventoryChangeRecord
                {
                    changes = changes.ToArray()
                });
            }
        }
    }
}
#endif