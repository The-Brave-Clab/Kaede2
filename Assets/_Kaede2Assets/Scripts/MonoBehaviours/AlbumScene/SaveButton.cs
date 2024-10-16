using Kaede2.Audio;
using Kaede2.UI.Framework;
using UnityEngine;

namespace Kaede2
{
    class SaveButton : MonoBehaviour
    {
        private CommonButton button;

        private void Awake()
        {
            button = GetComponent<CommonButton>();

            button.onClick.AddListener(AlbumItemViewCanvas.SaveCurrent);
            button.onClick.AddListener(AudioManager.ConfirmSound);
        }
    }

}