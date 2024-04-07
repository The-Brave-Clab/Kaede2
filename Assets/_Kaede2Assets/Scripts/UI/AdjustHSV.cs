using System;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2.UI
{
    [ExecuteAlways]
    public class AdjustHSV : MonoBehaviour
    {
        // this is set from the editor by default reference to the shader
        [SerializeField, HideInInspector]
        private Shader adjustHSVShader;

        [SerializeField]
        private Adjustment adjustment = new()
        {
            hsvAdjustment = Vector3.zero,
            referenceColor = Color.red
        };

        // private CanvasRenderer canvasRenderer;
        private Material material;
        private Vector3 lastHSVAdjustment;

        private static readonly int ReferenceColor = Shader.PropertyToID("_ReferenceColor");
        private static readonly int TargetColor = Shader.PropertyToID("_TargetColor");

        [Serializable]
        public struct Adjustment
        {
            [ColorUsage(false, false)]
            public Color referenceColor;

            public Vector3 hsvAdjustment;
        }

        private void Awake()
        {
#if UNITY_EDITOR
            if (adjustHSVShader == null)
            {
                // in editor, when we add this component to an object freshly, the default reference may not be set
                // in this case, we try to find the shader by name
                // this will only happen on the first time the component is added to an object
                adjustHSVShader = Shader.Find("UI/HSV Adjustable");

                // in player we don't need to worry about this, because the reference is guaranteed to be set
            }
#endif

            material = new Material(adjustHSVShader);
            material.SetColor(ReferenceColor, adjustment.referenceColor);
            material.SetColor(TargetColor, CalculateTargetColor(adjustment.referenceColor, adjustment.hsvAdjustment));

            if (TryGetComponent(out Graphic graphic)) // this covers many components
            {
                graphic.material = material;
            }
            else
            {
                // add other supported UI components
            }

            lastHSVAdjustment = new Vector3(Single.NaN, Single.NaN, Single.NaN);
        }

        private void Update()
        {
            // if (canvasRenderer.materialCount > 0 && canvasRenderer.GetMaterial() != material)
            //     canvasRenderer.SetMaterial(material, 0);

            if (adjustment.hsvAdjustment == lastHSVAdjustment) return;

            material.SetColor(ReferenceColor, adjustment.referenceColor);
            material.SetColor(TargetColor, CalculateTargetColor(adjustment.referenceColor, adjustment.hsvAdjustment));
            lastHSVAdjustment = adjustment.hsvAdjustment;
        }

        public static Color CalculateTargetColor(Color referenceColor, Vector3 hsvAdjustment)
        {
            Color.RGBToHSV(referenceColor, out var h, out var s, out var v);
            h += hsvAdjustment.x;
            s += hsvAdjustment.y;
            v += hsvAdjustment.z;

            h = Mathf.Repeat(h, 1);

            Color result = Color.HSVToRGB(h, s, v);
            result.a = referenceColor.a;
            return result;
        }
    }
}