using System.Collections.Generic;
using UnityEngine;

namespace Kaede2.UI.Framework
{
    public class SelectableGroup : MonoBehaviour
    {
        [SerializeField]
        private SelectableItem[] items;

        [SerializeField]
        private int selectedIndex = -1;

        public IReadOnlyList<SelectableItem> Items => items;
        public SelectableItem SelectedItem => selectedIndex < 0 ? null : items[selectedIndex];

        private void Awake()
        {
            if (selectedIndex >= 0)
                selectedIndex = NextAvailable(1, true);

            for (var i = 0; i < items.Length; i++)
            {
                var sel = i;
                var item = items[sel];
                item.selected = sel == selectedIndex;
                item.onSelectedChanged.AddListener(selected =>
                {
                    if (!selected) return;

                    selectedIndex = sel;
                    for (var j = 0; j < items.Length; j++)
                        items[j].selected = sel == j;
                });
            }
        }

        public void Select(int index)
        {
            selectedIndex = Mod(index, items.Length);
            selectedIndex = NextAvailable(1, true);
            for (var i = 0; i < items.Length; i++)
            {
                items[i].selected = i == selectedIndex;
            }
        }

        public void Next()
        {
            var nextAvailable = NextAvailable(1);
            if (nextAvailable >= 0)
                Select(nextAvailable);
        }

        public void Previous()
        {
            var nextAvailable = NextAvailable(-1);
            if (nextAvailable >= 0)
                Select(nextAvailable);
        }

        public void Confirm()
        {
            if (SelectedItem != null) SelectedItem.Confirm();
        }

        private int NextAvailable(int step, bool includeCurrent = false)
        {
            for (var i = includeCurrent ? 0 : 1; i < items.Length; i++)
            {
                var index = Mod(selectedIndex + i * step, items.Length);
                if (!items[index].gameObject.activeSelf) continue;
                return index;
            }
            return -1;
        }

        // a mod function that works with negative numbers
        // Mod(-1, 3) == 2; Mod(1, 3) == 1
        private static int Mod(int x, int m)
        {
            return (x % m + m) % m;
        }
    }
}