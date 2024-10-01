using UnityEngine;

namespace Kaede2.UI
{
    public class TabItemHorizontal : TabItem
    {
        protected override Vector2 GetTargetSizeDelta(Vector2 sizeDelta, bool isSelected)
        {
            return new Vector2(sizeDelta.x, isSelected ? 80 : 60);
        }
    }
}