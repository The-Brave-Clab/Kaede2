using System;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2.UI.Framework
{
    public abstract class CustomUIMaterial : MonoBehaviour
    {
        // this is set from the editor by default reference to the shader
        [SerializeField, HideInInspector]
        private Shader customShader;

        protected abstract string shaderName { get; }

        protected Material material;
        private Graphic graphic;

        protected virtual void Awake()
        {
            if (customShader == null)
            {
                customShader = Shader.Find(shaderName);
                this.LogWarning($"Shader {shaderName} not set, please make sure to set it in the inspector.");
            }

            if (customShader == null)
            {
                this.LogError($"Shader {shaderName} not found!");
                enabled = false;
                return;
            }

            material = new Material(customShader);

            if (TryGetComponent(out graphic))
            {
                graphic.material = material;
            }
        }

        protected virtual void Update()
        {
            var materialForRendering = graphic.materialForRendering;
            if (materialForRendering == null || materialForRendering.shader != customShader)
            {
                graphic.material = material;
                materialForRendering = graphic.materialForRendering;
            }

            if (materialForRendering == null)
                this.LogError("Material for rendering is null!");
            else
                UpdateMaterial(material, materialForRendering);
        }

        protected abstract void UpdateMaterial(Material material, Material materialForRendering);
    }
}