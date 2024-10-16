using UnityEngine;
using UnityEngine.UI;

namespace Kaede2
{
    public class RefreshLayout : MonoBehaviour
    {
        [SerializeField]
        private HorizontalLayoutGroup horizontal;

        [SerializeField]
        private VerticalLayoutGroup vertical;

        private void Update()
        {
            if (horizontal != null) horizontal.SetLayoutHorizontal();
            if (vertical != null) vertical.SetLayoutVertical();
        }
    }
}