using Kaede2.Scenario.Entities;
using UnityEngine;

namespace Kaede2.Live2D
{
    public class Live2DRenderer : MonoBehaviour
    {
        private void OnPostRender()
        {
            Live2DActorEntity.AllActors.ForEach(a => a.Render());
        }
    }
}