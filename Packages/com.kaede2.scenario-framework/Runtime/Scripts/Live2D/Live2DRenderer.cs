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
    }
}