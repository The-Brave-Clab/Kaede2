using System;
using Kaede2.Scenario.Framework.Entities;
using UnityEngine;

namespace Kaede2.Scenario.Framework.Live2D
{
    public class Live2DRenderer : MonoBehaviour
    {
        private void OnPostRender()
        {
            Live2DActorEntity.AllActors.ForEach(a => a.Render());
        }

        private void OnEnable()
        {
            live2d.Live2D.init();
        }

        private void OnDisable()
        {
            live2d.Live2D.dispose();
        }
    }
}