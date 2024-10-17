using Kaede2.Localization;
using TMPro;
using UnityEngine;

namespace Kaede2
{
    public class DownloadConfirmWindow : MonoBehaviour
    {
        [SerializeField] 
        private MessageWindow messageWindow;

        [SerializeField]
        private TextMeshProUGUI windowContent;

        [SerializeField]
        private LocalizedString formatString;

        public MessageWindow Window => messageWindow;

        public void SetSize(long bytes)
        {
            string humanReadableSize = Utils.CommonUtils.BytesToHumanReadable(bytes);
            windowContent.text = string.Format(formatString.Get(LocalizationManager.CurrentLocale), humanReadableSize);
        }
    }
}