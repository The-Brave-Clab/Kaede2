using System.Collections.Generic;
using UnityEngine;

namespace Kaede2.UI
{
    public class ControlActive : MonoBehaviour
    {
        [SerializeField]
        private List<GameObject> objects;

        private void OnEnable()
        {
            foreach (var obj in objects)
            {
                obj.SetActive(true);
            }
        }

        private void OnDisable()
        {
            foreach (var obj in objects)
            {
                obj.SetActive(false);
            }
        }
    }
}