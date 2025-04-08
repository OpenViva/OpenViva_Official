#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents a Steam Inventory Item
    /// </summary>
    [Serializable]
    public struct ItemData : IEquatable<ItemData>, IEquatable<int>, IEquatable<SteamItemDef_t>, IComparable<ItemData>, IComparable<int>, IComparable<SteamItemDef_t>
    {
        /// <summary>
        /// The item's ID as seen in Steam Inventory
        /// </summary>
        public int id;

        [NonSerialized]
        private ItemDefinitionObject _so;
        /// <summary>
        /// A pointer to the related <see cref="ItemDefinitionObject"/> if any, this is only relevant if you are using <see cref="ScriptableObject"/>s to track your items
        /// </summary>
        public ItemDefinitionObject ScriptableObject
        {
            get
            {
                if (SteamSettings.current == null)
                    return null;

                if (_so == null)
                {
                    var nId = this;
                    _so = SteamSettings.Client.inventory.items.FirstOrDefault(p => p.id == nId);
                }

                return _so;
            }
            set
            {
                _so = value;
                id = value.id;
            }
        }
        /// <summary>
        /// The name property of the item if known
        /// <para>This will be a localized value if localized values are defined for this item</para>
        /// </summary>
        public readonly string Name => API.Inventory.Client.GetItemDefinitionProperty(new SteamItemDef_t(id), "name");
        /// <summary>
        /// Does this item have a price
        /// </summary>
        public readonly bool HasPrice => API.Inventory.Client.GetItemPrice(new SteamItemDef_t(id), out ulong _, out ulong _);
        /// <summary>
        /// The currency applicable to the local user
        /// </summary>
        public static Currency.Code CurrencyCode => API.Inventory.Client.LocalCurrencyCode;
        /// <summary>
        /// The string representation of the local user's currency e.g. $, €, £, etc.
        /// </summary>
        public static string CurrencySymbol => API.Inventory.Client.LocalCurrencySymbol;
        /// <summary>
        /// The current price in base 100. This is the current price displayed in the Steam store if the Steam store and accounts for any discounts or other promotions.
        /// <para>A value of 150 is equivalent to $1.50</para>
        /// </summary>
        public readonly ulong CurrentPrice
        {
            get
            {
                if (API.Inventory.Client.GetItemPrice(new SteamItemDef_t(id), out ulong current, out ulong _))
                {
                    return current;
                }
                else
                    return 0;
            }
        }
        /// <summary>
        /// The base price in base 100. This is the core price before any discounts or promotions are applied
        /// <para>A value of 150 is equivalent to $1.50</para>
        /// </summary>
        public readonly ulong BasePrice
        {
            get
            {
                if (API.Inventory.Client.GetItemPrice(new SteamItemDef_t(id), out ulong _, out ulong baseprice))
                {
                    return baseprice;
                }
                else
                    return 0;
            }
        }
        /// <summary>
        /// Get the list of <see cref="ItemDetail"/> for this item.
        /// <para>A <see cref="ItemData"/> represents a "type" of item, the <see cref="ItemDetail"/> represent the instances or "stacks" of the item. A detail may have a quantity of 0 or many and should be thought of as a "stack" of this item type.</para>
        /// </summary>
        /// <returns></returns>
        public readonly List<ItemDetail> GetDetails() => API.Inventory.Client.Details(this);
        /// <summary>
        /// Get the total quantity of this item owned by the user
        /// <para>This is simply the sum of all <see cref="ItemData"/></para>
        /// </summary>
        /// <returns></returns>
        public readonly long GetTotalQuantity() => API.Inventory.Client.ItemTotalQuantity(this);
        /// <summary>
        /// Attempt to add this item to the user's inventory. This will only work if this is a promo item and the user is eligible for it.
        /// </summary>
        /// <param name="callback">A delegate of the form (<see cref="InventoryResult"/> results) that is invoked when the process completes</param>
        /// <returns></returns>
        public readonly bool AddPromoItem(Action<InventoryResult> callback) => API.Inventory.Client.AddPromoItem(new SteamItemDef_t(id), callback);
        /// <summary>
        /// Create a collection of <see cref="ConsumeOrder"/> that will consume the <see cref="ItemDetail"/> of this item for use with consume processes.
        /// </summary>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public readonly ConsumeOrder[] GetConsumeOrders(uint quantity)
        {
            var details = GetDetails();

            if (details.Sum(p => (long)p.Quantity) < quantity)
                return null;

            var results = new List<ConsumeOrder>();
            uint count = 0;
            var index = 0;
            while (count < quantity)
            {
                uint cCount = (uint)Mathf.Min(details[index].Quantity, quantity - count);
                count += cCount;

                results.Add(new ConsumeOrder
                {
                    detail = details[index].itemDetails,
                    quantity = cCount
                });
            }

            return results.ToArray();
        }
        /// <summary>
        /// Attempts to consume a single instance of this item
        /// </summary>
        /// <param name="callback">A delegate of the form (<see cref="InventoryResult"/> results) that is invoked when the process completes</param>
        /// <returns></returns>
        public readonly bool Consume(Action<InventoryResult> callback)
        {
            var details = GetDetails();

            if (details.Sum(p => (long)p.Quantity) < 1)
                return false;

            var instance = details.First(p => p.Quantity > 0);
            API.Inventory.Client.ConsumeItem(instance.itemDetails.m_itemId, 1, callback);
            return true;
        }
        /// <summary>
        /// Attempts to consume the order provided ... you can get an order for more than 1 item to be consumed at once via the <see cref="GetConsumeOrders(uint)"/> method
        /// </summary>
        /// <param name="order">The order to consume</param>
        /// <param name="callback">A delegate of the form (<see cref="InventoryResult"/> results) that is invoked when the process completes</param>
        public readonly void Consume(ConsumeOrder order, Action<InventoryResult> callback) => API.Inventory.Client.ConsumeItem(order.detail.m_itemId, order.quantity, callback);
        /// <summary>
        /// Attempts to consume a given quantity of the item, this will use the <see cref="GetConsumeOrders(uint)"/>, to get orders and then will consume each if possible.
        /// </summary>
        /// <param name="quantity">The number to be consumed</param>
        /// <param name="callback">A delegate of the form (<see cref="InventoryResult"/> results) that is invoked when the process completes</param>
        /// <returns></returns>
        public readonly bool Consume(uint quantity, Action<InventoryResult> callback)
        {
            var orders = GetConsumeOrders(quantity);
            if(orders == null || orders.Length < 1)
            {
                return false;
            }
            else
            {
                List<ItemDetail> details = new List<ItemDetail>();
                EResult eResult = EResult.k_EResultOK;
                var worker = new BackgroundWorker();
                worker.DoWork += (sender, eventArgs) =>
                {
                    foreach(var order in orders)
                    {
                        bool wait = true;
                        
                        API.Inventory.Client.ConsumeItem(order.detail.m_itemId, order.quantity, (r) =>
                        {
                            eResult = r.result;

                            if (eResult == EResult.k_EResultOK)
                                details.AddRange(r.items);
                                
                            wait = false;
                        });

                        while(wait)
                            Thread.Sleep(50);

                        if (eResult != EResult.k_EResultOK)
                            break;
                    }

                    var final = new InventoryResult();
                    final.result = eResult;
                    final.items = details.ToArray();

                    eventArgs.Result = final;
                };
                worker.RunWorkerCompleted += (sender, eventArgs) =>
                {
                    var final = (InventoryResult)eventArgs.Result;
                    callback?.Invoke(final);
                    worker.Dispose();
                };
                return true;
            }
        }
        /// <summary>
        /// Get a collection fo <see cref="ExchangeEntry"/> for use in exchange processes
        /// </summary>
        /// <param name="quantity">The number of items that should be included in the exchange</param>
        /// <param name="entries">The resulting <see cref="ExchangeEntry"/> that represent the requested items</param>
        /// <returns>True if sufficient items where found, false if the exchange request is not possible</returns>
        public readonly bool GetExchangeEntry(uint quantity, out ExchangeEntry[] entries)
        {
            var details = GetDetails();

            if (details.Sum(p => (long)p.Quantity) < quantity)
            {
                entries = new ExchangeEntry[0];
                return false;
            }
            else
            {
                var list = new List<ExchangeEntry>();
                uint count = 0;
                var index = 0;
                while (count < quantity)
                {
                    if (details[index].Quantity <= quantity - count)
                    {
                        if (details[index].Quantity > 0)
                        {
                            list.Add(new ExchangeEntry
                            {
                                instance = details[index].ItemId,
                                quantity = details[index].Quantity
                            });
                            count += details[index].Quantity;
                        }
                    }
                    else
                    {
                        if (details[index].Quantity > 0)
                        {
                            var remainder = (uint)(quantity - count);
                            list.Add(new ExchangeEntry
                            {
                                instance = details[index].ItemId,
                                quantity = remainder,
                            });
                            count += remainder;
                        }
                    }

                    index++;
                }

                entries = list.ToArray();
                return true;
            }
        }
        /// <summary>
        /// Exchange the items represented by teh <see cref="ExchangeEntry"/> collection you pass in to produce 1 of this item.
        /// <para>For this to work this item must define an exchange recipe that takes the items defined in the <see cref="ExchangeEntry"/> collection you provided. This is a secure way to convert 1 or more items into a specific other item e.g. "crafting"</para>
        /// </summary>
        /// <param name="recipeEntries">A collection of <see cref="ExchangeEntry"/> to be consumed</param>
        /// <param name="callback">A delegate of the form (<see cref="InventoryResult"/> results) that is invoked when the process completes</param>
        public readonly void Exchange(IEnumerable<ExchangeEntry> recipeEntries, Action<InventoryResult> callback)
        {
            var list = recipeEntries.ToArray();

            var instances = new SteamItemInstanceID_t[list.Length];
            var counts = new uint[list.Length];
            for (int i = 0; i < list.Length; i++)
            {
                instances[i] = list[i].instance;
                counts[i] = list[i].quantity;
            }
            API.Inventory.Client.ExchangeItems(new SteamItemDef_t(id), instances, counts, callback);
        }
        /// <summary>
        /// This is only available to developers of this app and should only be used prior to launch of the game for testing purposes.
        /// </summary>
        /// <param name="callback">A delegate of the form (<see cref="InventoryResult"/> results) that is invoked when the process completes</param>
        public readonly void GenerateItem(Action<InventoryResult> callback)
        {
            API.Inventory.Client.GenerateItems(new SteamItemDef_t[] { new SteamItemDef_t(id) }, new uint[] { 1 }, callback);
        }
        /// <summary>
        /// This is only available to developers of this app and should only be used prior to launch of the game for testing purposes.
        /// </summary>
        /// <param name="callback">A delegate of the form (<see cref="InventoryResult"/> results) that is invoked when the process completes</param>
        public readonly void GenerateItem(uint quantity, Action<InventoryResult> callback)
        {
            API.Inventory.Client.GenerateItems(new SteamItemDef_t[] { new SteamItemDef_t(id) }, new uint[] { quantity }, callback);
        }
        /// <summary>
        /// Starts the purchase process
        /// <para>Assuming the item has a valid price configured this will open the Steam overlay with this item in the shopping cart. The <see cref="SteamInventoryStartPurchaseResult_t"/> provided in the callback will indicate the transaction ID which can be used with <see cref="API.Inventory.Client.EventSteamMicroTransactionAuthorizationResponse"/> to know the purchase is completed if it is completed.</para>
        /// <para>Generally speaking you do not need to track when the purchase is completed and in most games the game will simply monitor for any changes to inventory. This is advisable as its possible the player out side of your game use Steam store or other mechanism to purchase an item or was gifted an item from another player or in some other way acquired an item from a process out side your game's control. As such it is important that your game handle the possibility of inventory changing at any time which would of course catch any pruchases made from within game as well.</para>
        /// </summary>
        /// <param name="callback">A delegate of the form (<see cref="SteamInventoryStartPurchaseResult_t"/> result, <see cref="bool"/> ioError) that is invoked when the process completes</param>
        public readonly void StartPurchase(Action<SteamInventoryStartPurchaseResult_t, bool> callback)
        {
            API.Inventory.Client.StartPurchase(new SteamItemDef_t[] { new SteamItemDef_t(id) }, new uint[] { 1 }, callback);
        }
        /// <summary>
        /// Starts the purchase process
        /// <para>Assuming the item has a valid price configured this will open the Steam overlay with this item in the shopping cart. The <see cref="SteamInventoryStartPurchaseResult_t"/> provided in the callback will indicate the transaction ID which can be used with <see cref="API.Inventory.Client.EventSteamMicroTransactionAuthorizationResponse"/> to know the purchase is completed if it is completed.</para>
        /// <para>Generally speaking you do not need to track when the purchase is completed and in most games the game will simply monitor for any changes to inventory. This is advisable as its possible the player out side of your game use Steam store or other mechanism to purchase an item or was gifted an item from another player or in some other way acquired an item from a process out side your game's control. As such it is important that your game handle the possibility of inventory changing at any time which would of course catch any pruchases made from within game as well.</para>
        /// </summary>
        /// <param name="count">The number of items to add to the shopping cart</param>
        /// <param name="callback">A delegate of the form (<see cref="SteamInventoryStartPurchaseResult_t"/> result, <see cref="bool"/> ioError) that is invoked when the process completes</param>
        public readonly void StartPurchase(uint count, Action<SteamInventoryStartPurchaseResult_t, bool> callback)
        {
            API.Inventory.Client.StartPurchase(new SteamItemDef_t[] { new SteamItemDef_t(id) }, new uint[] { count }, callback);
        }
        /// <summary>
        /// Gets the price values for the item
        /// </summary>
        /// <param name="currentPrice">The current price in base 100 e.g. 150 = $1.50</param>
        /// <param name="basePrice">The base price in base 100 e.g 150 = $1.50</param>
        /// <returns>True if the item has a price, false otherwise</returns>
        public readonly bool GetPrice(out ulong currentPrice, out ulong basePrice) => API.Inventory.Client.GetItemPrice(new SteamItemDef_t(id), out currentPrice, out basePrice);
        /// <summary>
        /// Request a drop of this item, this will cause the item to be dropped if the item has a valid promo rule configured and the user is eligible for it
        /// </summary>
        /// <param name="callback">A delegate of the form (<see cref="InventoryResult"/> results) that is invoked when the process completes</param>
        public readonly void TriggerDrop(Action<InventoryResult> callback) => API.Inventory.Client.TriggerItemDrop(new SteamItemDef_t(id), callback);
        /// <summary>
        /// Returns the culturally formatted string representing the price as seen by the user
        /// <para>For example a price of 150 would be seen by an Irish user as €1.50 and by a German user as €1,50</para>
        /// </summary>
        /// <returns>The culturally formatted string representation of the price including its currency symbol</returns>
        public readonly string CurrentPriceString()
        {
            System.Globalization.NumberFormatInfo cultureInfo = (System.Globalization.NumberFormatInfo)System.Globalization.CultureInfo.CurrentCulture.NumberFormat.Clone();
            cultureInfo.CurrencySymbol = CurrencySymbol;

            return ((double)CurrentPrice / 100).ToString("c", cultureInfo);
        }
        /// <summary>
        /// Returns the culturally formatted string representing the price as seen by the user
        /// <para>For example a price of 150 would be seen by an Irish user as €1.50 and by a German user as €1,50</para>
        /// </summary>
        /// <returns>The culturally formatted string representation of the price including its currency symbol</returns>
        public readonly string BasePriceString()
        {
            System.Globalization.NumberFormatInfo cultureInfo = (System.Globalization.NumberFormatInfo)System.Globalization.CultureInfo.CurrentCulture.NumberFormat.Clone();
            cultureInfo.CurrencySymbol = CurrencySymbol;

            return ((double)BasePrice / 100).ToString("c", cultureInfo);
        }
        /// <summary>
        /// Request the inventory item prices be loaded from the Steam backend
        /// </summary>
        /// <param name="callback">A delegate of teh form (<see cref="SteamInventoryRequestPricesResult_t"/> result, <see cref="bool"/> ioError) that will be invoked when the process completes</param>
        public static void RequestPrices(Action<SteamInventoryRequestPricesResult_t, bool> callback) => API.Inventory.Client.RequestPrices(callback);
        /// <summary>
        /// Request all items be updated in local memory
        /// </summary>
        /// <param name="callback">A delegate of the form (<see cref="InventoryResult"/> results) that is invoked when the process completes</param>
        public static void Update(Action<InventoryResult> callback) => API.Inventory.Client.GetAllItems(callback);
        /// <summary>
        /// Gets the <see cref="ItemData"/> representing the indicated item
        /// </summary>
        /// <param name="id">The item ID to get</param>
        /// <returns>The <see cref="ItemData"/> result</returns>
        public static ItemData Get(int id) => id;
        /// <summary>
        /// Get the <see cref="ItemData"/> based on the native <see cref="SteamItemDef_t"/>
        /// </summary>
        /// <param name="id">The native <see cref="SteamItemDef_t"/> of the item you want to get</param>
        /// <returns>The <see cref="ItemData"/> result</returns>
        public static ItemData Get(SteamItemDef_t id) => id;
        /// <summary>
        /// Get the <see cref="ItemData"/> from the <see cref="ItemDefinitionObject"/>
        /// </summary>
        /// <param name="item">The object instance of the item you want to get</param>
        /// <returns>The <see cref="ItemData"/> result</returns>
        public static ItemData Get(ItemDefinitionObject item) => item.id;

        #region Boilerplate
        public readonly int CompareTo(ItemData other)
        {
            return id.CompareTo(other.id);
        }

        public readonly int CompareTo(int other)
        {
            return id.CompareTo(other);
        }

        public readonly int CompareTo(SteamItemDef_t other)
        {
            return id.CompareTo(other);
        }

        public readonly bool Equals(ItemData other)
        {
            return id.Equals(other.id);
        }

        public readonly bool Equals(int other)
        {
            return id.Equals(other);
        }

        public readonly bool Equals(SteamItemDef_t other)
        {
            return id.Equals(other);
        }

        public readonly override bool Equals(object obj)
        {
            return id.Equals(obj);
        }

        public readonly override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public static bool operator ==(ItemData l, ItemData r) => l.id == r.id;
        public static bool operator ==(ItemData l, int r) => l.id == r;
        public static bool operator ==(ItemData l, SteamItemDef_t r) => l.id == r.m_SteamItemDef;
        public static bool operator !=(ItemData l, ItemData r) => l.id != r.id;
        public static bool operator !=(ItemData l, int r) => l.id != r;
        public static bool operator !=(ItemData l, SteamItemDef_t r) => l.id != r.m_SteamItemDef;

        public static implicit operator int(ItemData c) => c.id;
        public static implicit operator ItemData(int id) => new ItemData { id = id };
        public static implicit operator SteamItemDef_t(ItemData c) => new SteamItemDef_t(c.id);
        public static implicit operator ItemData(SteamItemDef_t id) => new ItemData { id = id.m_SteamItemDef };
        #endregion
    }
}
#endif