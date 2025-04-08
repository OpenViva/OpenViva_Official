#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    public class ItemShoppingCartManager : MonoBehaviour
    {
        [Serializable]
        public class StartPurchaseError : UnityEvent<EResult>
        { }

        [Serializable]
        public class StartPurchaseSuccess : UnityEvent<SteamInventoryStartPurchaseResult_t>
        { }

        [Serializable]
        public class OrderAuthorization : UnityEvent<ItemEntry[], bool>
        { }

        [Serializable]
        public struct ItemEntry
        {
            public ItemDefinitionObject item;
            public int quantity;
        }

        public StartPurchaseError evtStartPurchaseError;
        public StartPurchaseSuccess evtStartPurchaseSuccess;
        public OrderAuthorization evtOrderAuthorization;

        /// <summary>
        /// Defaults to false
        /// </summary>
        public bool OrderPending => _result.HasValue;
        /// <summary>
        /// Defaults to 0
        /// </summary>
        public ulong OrderId => _result.HasValue ? _result.Value.m_ulOrderID : 0;
        /// <summary>
        /// Defaults to 0
        /// </summary>
        public ulong TransactionId => _result.HasValue ? _result.Value.m_ulTransID : 0;

        private SteamInventoryStartPurchaseResult_t? _result = null;
        
        public List<ItemEntry> items = new List<ItemEntry>();

        private void Start()
        {
            API.Inventory.Client.EventSteamMicroTransactionAuthorizationResponse.AddListener(HandleAuthorizationResponce);
        }

        private void OnDestroy()
        {
            API.Inventory.Client.EventSteamMicroTransactionAuthorizationResponse.RemoveListener(HandleAuthorizationResponce);
        }

        private void HandleAuthorizationResponce(AppId_t appId, ulong orderId, bool authorized)
        {
            if (OrderPending
                && appId == API.App.Id
                && orderId == OrderId)
            {
                var itemsSent = items.ToArray();
                if(authorized)
                {
                    items.Clear();
                }
                _result = null;
                evtOrderAuthorization.Invoke(itemsSent, authorized);
            }
        }

        public void Add(ItemDefinitionObject item, int count)
        {
            if (OrderPending)
            {
                Debug.LogWarning($"{nameof(ItemShoppingCartManager.Add)} - Attempted to add items with a purchase pending, wait for order authorization responce or call {nameof(ItemShoppingCartManager.ClearPending)} before starting a new one.");
                return;
            }

            var current = items.FirstOrDefault(x => x.item == item);
            items.RemoveAll(p => p.item == item);
            current.item = item;
            current.quantity += count;
            if (current.quantity > 0)
                items.Add(current);
        }

        public void Set(ItemDefinitionObject item, int count)
        {
            if (OrderPending)
            {
                Debug.LogWarning($"{nameof(ItemShoppingCartManager.Set)} - Attempted to set item quantity with a purchase pending, wait for order authorization responce or call {nameof(ItemShoppingCartManager.ClearPending)} before starting a new one.");
                return;
            }

            if (count <= 0)
                items.RemoveAll(p => p.item == item);
            else
            {
                var current = items.FirstOrDefault(x => x.item == item);
                if (current.quantity != count)
                {
                    items.RemoveAll(p => p.item == item);
                    current.item = item;
                    current.quantity = count;
                    items.Add(current);
                }
            }
        }

        public int Get(ItemDefinitionObject item)
        {
            return items.FirstOrDefault(x => x.item == item).quantity;
        }

        /// <summary>
        /// Returns the raw 'price' reported by Steam for these items.
        /// This simply gets the per item price, multiplies it by the quantity and sums the total.
        /// This cannot account for taxes, conversion rates or other variance between the Steam reported price per item and the checkout price.
        /// </summary>
        /// <returns></returns>
        public ulong TotalPrice()
        {
            var total = 0ul;
            foreach(var item in items)
            {
                if(item.item != null
                    && item.quantity > 0)
                {
                    total += item.item.CurrentPrice * (ulong)item.quantity;
                }
            }

            return total;
        }

        /// <summary>
        /// Estimates the price and returns a string such as '$0.99' by summing up the reported price for each item.
        /// This cannot account for taxes, conversion rates or other variance between the Steam reported price per item and the checkout price.
        /// </summary>
        /// <returns></returns>
        public string TotalPriceSymbolledString()
        {
            var total = TotalPrice();
            return API.Inventory.Client.LocalCurrencySymbol + (total * 0.01d).ToString("0.00");
        }

        /// <summary>
        /// Estimates the price and returns a string such as '0.99 USD' by summing up the reported price for each item.
        /// This cannot account for taxes, converion rates or other variance between the Steam reported price per item and the checkout price.
        /// </summary>
        /// <returns></returns>
        public string TotalPriceCurrencyCodeString()
        {
            var total = TotalPrice();
            return (total * 0.01d).ToString("0.00") + " " + API.Inventory.Client.LocalCurrencyCode.ToString();
        }

        public void StartPurchase()
        {
            StartPurchase(null);
        }

        public void StartPurchase(Action<SteamInventoryStartPurchaseResult_t, bool> callback)
        {
            if (OrderPending)
            {
                Debug.LogWarning($"{nameof(ItemShoppingCartManager.StartPurchase)} - Attempted to start a purcahse with a purchase pending, wait for order authorization responce or call {nameof(ItemShoppingCartManager.ClearPending)} before starting a new one.");
            }
            else
            {
                items.RemoveAll(p => p.item == null || p.quantity <= 0);
                var itemDefs = new Steamworks.SteamItemDef_t[items.Count];
                var itemQuan = new uint[items.Count];
                for (int i = 0; i < items.Count; i++)
                {
                    var entry = items[i];
                    itemDefs[i] = entry.item.Id;
                    itemQuan[i] = entry.quantity <= 0 ? 0 : (uint)(entry.quantity);
                }

                API.Inventory.Client.StartPurchase(itemDefs, itemQuan, (result, error) =>
                {
                    if(error)
                    {
                        Debug.LogError($"{nameof(ItemShoppingCartManager.StartPurchase)} - IO Error reported by Steam");
                        evtStartPurchaseError.Invoke(EResult.k_EResultIOFailure);
                    }
                    else
                    {
                        if (result.m_result != EResult.k_EResultOK)
                        {
                            Debug.LogError($"{nameof(ItemShoppingCartManager.StartPurchase)} - Error reported by Steam: {result.m_result}");
                            evtStartPurchaseError.Invoke(result.m_result);
                        }
                        else
                        {
                            _result = result;
                            evtStartPurchaseSuccess.Invoke(result);
                        }
                    }

                    callback?.Invoke(result, error);
                });
            }
        }

        public void ClearPending(bool clearCart = false)
        {
            if(OrderPending)
            {
                Debug.LogWarning($"{nameof(ItemShoppingCartManager.ClearPending)}(clearCart = {clearCart}) - Clearing a pending order before the Authorization Responce is returned does not cancel the order, the order may still complete at a later time but will be ignored by the cart.");
                _result = null;
                if (clearCart)
                    items.Clear();
            }
        }
    }
}
#endif