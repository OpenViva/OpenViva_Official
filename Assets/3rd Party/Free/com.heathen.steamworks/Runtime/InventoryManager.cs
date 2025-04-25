#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    public class InventoryManager : MonoBehaviour
    {
        public Currency.Code CurrencyCode => API.Inventory.Client.LocalCurrencyCode;
        public string CurrencySymbol => API.Inventory.Client.LocalCurrencySymbol;
        public List<ItemDefinitionObject> Items
        {
            get
            {
                if (SteamSettings.current != null)
                    return SteamSettings.Client.inventory.items;
                else
                {
                    Debug.LogWarning("You can only fetch the list of items if your using a SteamSettings object");
                    return null;
                }
            }
        }

        public InventoryChangedEvent evtChanged;
        public SteamMicroTransactionAuthorizationResponce evtTransactionResponce;

        private void OnEnable()
        {
            if (SteamSettings.current != null)
                SteamSettings.Client.inventory.EventChanged.AddListener(evtChanged.Invoke);
            API.Inventory.Client.EventSteamMicroTransactionAuthorizationResponse.AddListener(evtTransactionResponce.Invoke);
        }

        private void OnDisable()
        {
            if (SteamSettings.current != null)
                SteamSettings.Client.inventory.EventChanged.RemoveListener(evtChanged.Invoke);
            API.Inventory.Client.EventSteamMicroTransactionAuthorizationResponse.RemoveListener(evtTransactionResponce.Invoke);
        }

        /// <summary>
        /// Returns the sub set of items that have a price and are not hidden.
        /// These should be the same items visible in Steam's store
        /// </summary>
        /// <returns></returns>
        public ItemDefinitionObject[] GetStoreItems()
        {
            return Items.Where(i => !i.Hidden && !i.StoreHidden && i.item_price.Valid).ToArray();
        }
    }
}
#endif