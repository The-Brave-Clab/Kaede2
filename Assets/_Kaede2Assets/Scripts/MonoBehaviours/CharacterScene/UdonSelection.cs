using Kaede2.Utils;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Kaede2
{
    public class UdonSelection : BaseZukanSelection<UdonSelection>
    {
        protected override AsyncOperationHandle<Sprite> LoadImage()
        {
            return ResourceLoader.LoadUdonImage(profile);
        }

        protected override string Text => profile.Name;

        protected override void SetPreview()
        {
            sceneController.UdonPreviewImage.sprite = imageHandle.Result;
        }
    }
}