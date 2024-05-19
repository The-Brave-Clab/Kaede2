using UnityEngine;

namespace Kaede2.UI
{
    public class TabItemVertical : TabItem
    {
        protected override Vector2 GetTargetSizeDelta(Vector2 sizeDelta, bool isSelected)
        {
            return new Vector2(isSelected ? 320 : 300, sizeDelta.y);
        }
    }
}