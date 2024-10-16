using Kaede2.UI.Framework;

namespace Kaede2.UI
{
    public class TabGroup : SelectableGroup
    {
        public int ActiveIndex { get; private set; }

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

            ActiveIndex = 0;
        }

        public bool SelectTab(SelectableItem item)
        {
            if (item is not TabItem) return false;

            int currentActive = ActiveIndex;
            for (var i = 0; i < Items.Count; i++)
            {
                var selectableItem = Items[i];
                if (selectableItem is not TabItem tab) continue;

                tab.Active = selectableItem == item;
                if (tab.Active)
                {
                    ActiveIndex = i;
                }
            }

            return currentActive != ActiveIndex;
        }
    }
}