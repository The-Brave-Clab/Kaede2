using System;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2.UI
{
    [ExecuteAlways]
    public class AdjustHSV : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        private Shader adjustHSVShader;

        [SerializeField]
        private Vector3 hsvAdjustment;

        private CanvasRenderer canvasRenderer;
        private Material material;
        private Vector3 lastHSVAdjustment;

        private static readonly int Hue = Shader.PropertyToID("_Hue");
        private static readonly int Saturation = Shader.PropertyToID("_Saturation");
        private static readonly int Value = Shader.PropertyToID("_Value");
 
        private void Start()
        {
            if (adjustHSVShader == null)
            {
                return;
            }

            if (!TryGetComponent(out canvasRenderer))
            {
                return;
            }

            material = new Material(adjustHSVShader);

            lastHSVAdjustment = hsvAdjustment;
        }

        private void Update()
        {
            if (canvasRenderer.materialCount > 0 && canvasRenderer.GetMaterial() != material)
                canvasRenderer.SetMaterial(material, 0);

            if (hsvAdjustment == lastHSVAdjustment) return;

            material.SetFloat(Hue, hsvAdjustment.x);
            material.SetFloat(Saturation, hsvAdjustment.y);
            material.SetFloat(Value, hsvAdjustment.z);
            lastHSVAdjustment = hsvAdjustment;
        }
    }
}