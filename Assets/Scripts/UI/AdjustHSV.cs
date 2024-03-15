using UnityEngine;

namespace Kaede2.UI
{
    [ExecuteAlways]
    public class AdjustHSV : MonoBehaviour
    {
        // this is set from the editor by default reference to the shader
        [SerializeField, HideInInspector]
        private Shader adjustHSVShader;

        [SerializeField]
        private Vector3 hsvAdjustment;

#if UNITY_EDITOR
        // this makes setting the adjustment easier in the editor
        // it is made a serialized field since we want to save the value for next usage
        // not used in player
        [SerializeField, JetBrains.Annotations.UsedImplicitly]
        private Color referenceColor = Color.red;
#endif

        private CanvasRenderer canvasRenderer;
        private Material material;
        private Vector3 lastHSVAdjustment;

        private static readonly int Hue = Shader.PropertyToID("_Hue");
        private static readonly int Saturation = Shader.PropertyToID("_Saturation");
        private static readonly int Value = Shader.PropertyToID("_Value");

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

            if (!TryGetComponent(out canvasRenderer))
            {
                Debug.LogError("AdjustHSV requires a CanvasRenderer component");
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