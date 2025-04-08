#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using UnityEngine;
using UnityEngine.UI;

namespace Heathen.SteamworksIntegration.UI
{
    public class WorkshopBrowserSimpleItemRecord : MonoBehaviour, IWorkshopBrowserItemTemplate
    {
        public RawImage previewImage;
        public TMPro.TextMeshProUGUI titleLabel;
        public TMPro.TextMeshProUGUI authorLabel;
        public Image voteFillImage;
        [Header("Tooltip Elements")]
        public TMPro.TextMeshProUGUI tipTitleLabel;
        public TMPro.TextMeshProUGUI tipDescriptionLabel;

        private WorkshopItem _item;
        public WorkshopItem Item 
        {
            get => _item;
            set => Load(value);
        }

        public void Load(WorkshopItem item)
        {
            _item = item;

            if(item.previewImage != null)
                previewImage.texture = item.previewImage;
            else
            {
                item.DownloadPreviewImage();
            }
        }
    }
}
#endif