using System;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Kaede2
{
    public class VertexSelection : BaseZukanSelection<VertexSelection>
    {
        protected override AsyncOperationHandle<Sprite> LoadImage()
        {
            return ResourceLoader.LoadVertexImage(profile);
        }

        protected override string Text
        {
            get
            {
                const string kDotSeparator = "・";

                int lastIndex = profile.Name.LastIndexOf(kDotSeparator, StringComparison.Ordinal);
                if (lastIndex == -1)
                {
                    // ugly hack to make the text wrap properly
                    return profile.Name
                        .Replace("角のように硬質化して隆起したもの", "角のように硬質化して\n\n隆起したもの")
                        .Replace("矢のようなものを発生させたもの", "矢のようなものを\n\n発生させたもの")
                        .Replace("ムカデのように長い体形のもの", "ムカデのように\n\n長い体形のもの")
                        .Replace("巨大な蛇のような姿のもの", "巨大な蛇のような\n\n姿のもの");
                }

                return profile.Name[..lastIndex] + $"\n{kDotSeparator}\n" + profile.Name[(lastIndex + kDotSeparator.Length)..];
            }
        }

        protected override void SetPreview()
        {
            sceneController.VertexPreviewImage.sprite = imageHandle.Result;
        }
    }
}