using Kaede2.Utils;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Kaede2
{
    public class FairySelection : BaseZukanSelection<FairySelection>
    {
        protected override AsyncOperationHandle<Sprite> LoadImage()
        {
            return ResourceLoader.LoadFairyImage(profile);
        }

        protected override string Text => profile.Name;

        protected override void SetPreview()
        {
            sceneController.FairyPreviewImage.sprite = imageHandle.Result;
        }
    }
}
