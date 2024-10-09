using Kaede2.UI.Framework;

namespace Kaede2
{
    public class StoryCategorySelectableGroup : SelectableGroup
    {
        protected override void OnItemSelected(int selection)
        {
            base.OnItemSelected(selection);

            if (items[selection] is not StoryCategorySelectable) return;
            for (var i = 0; i < items.Count; i++)
            {
                if (items[i] is not StoryCategorySelectable storyCategorySelectable) continue;
                storyCategorySelectable.Activated = selection == i;
            }
        }
    }
}
