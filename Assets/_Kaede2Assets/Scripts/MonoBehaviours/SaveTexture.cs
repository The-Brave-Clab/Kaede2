using Kaede2.Utils;
using UnityEngine;
using UnityEngine.UI;

public class SaveTexture : MonoBehaviour
{
    public Image image;

    public void Save()
    {
        var texture = image.sprite.texture;
        this.Log("Saving texture");
        Kaede2.Utils.SaveTexture.Save("background.png", texture);
    }
}
