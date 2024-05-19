using Kaede2.UI.Framework;

namespace Kaede2.UI
{
    public class TabGroup : SelectableGroup
    {
        protected override void Awake()
        {
            base.Awake();

            bool first = true;
            foreach (var selectableItem in Items)
            {
                if (selectableItem is not TabItem tab) continue;
                if (first)
                {
                    tab.Active = true;
                    first = false;
                }
                tab.group = this;
            }
        }

        public void SelectTab(SelectableItem item)
        {
            if (item is not TabItem) return;

            foreach (var selectableItem in Items)
            {
                if (selectableItem is TabItem tab)
                {
                    tab.Active = selectableItem == item;
                }
            }
        }
    }
}