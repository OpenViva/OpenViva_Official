#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using System;
using System.Linq;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public struct InventoryChangeRecord
    {
        public ItemChangeRecord[] changes;

        public bool HasChanges => changes != null && changes.Length > 0;
        public long TotalQuantityBefore => changes.Sum(x => x.TotalQuantityBefore);
        public long TotalQuantityAfter => changes.Sum(x => x.TotalQuantityAfter);
        public long TotalQuantityChange => changes.Sum(x => x.TotalQuantityChange);
    }
}
#endif