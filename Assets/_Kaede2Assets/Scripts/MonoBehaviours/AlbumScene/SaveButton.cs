using System;
using Kaede2.Input;
using Kaede2.UI.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kaede2
{
    class SaveButton : MonoBehaviour
    {
        private CommonButton button;

        private void Awake()
        {
            button = GetComponent<CommonButton>();

            button.onClick.AddListener(AlbumItemViewCanvas.SaveCurrent);
        }
    }

}