﻿using System.Collections.Generic;
using UnityEngine;

namespace Kaede2.UI.Framework
{
    public class SelectableGroup : MonoBehaviour
    {
        [SerializeField]
        protected List<SelectableItem> items;

        [SerializeField]
        protected int selectedIndex = -1;

        public IReadOnlyList<SelectableItem> Items => items;
        public SelectableItem SelectedItem => selectedIndex < 0 ? null : items[selectedIndex];

        protected virtual void Awake()
        {
            if (selectedIndex >= 0)
                selectedIndex = NextAvailable(1, true);

            for (var i = 0; i < items.Count; i++)
            {
                var sel = i;
                var item = items[sel];
                item.selected = sel == selectedIndex;
                item.onSelected.AddListener(() =>
                {
                    selectedIndex = sel;
                    for (var j = 0; j < items.Count; j++)
                    {
                        items[j].selected = sel == j;
                        if (!items[j].selected) items[j].onDeselected?.Invoke();
                    }
                });
            }
        }

        public void Select(int index)
        {
            selectedIndex = Mod(index, items.Count);
            selectedIndex = NextAvailable(1, true);
            for (var i = 0; i < items.Count; i++)
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
            for (var i = includeCurrent ? 0 : 1; i < items.Count; i++)
            {
                var index = Mod(selectedIndex + i * step, items.Count);
                if (!items[index].gameObject.activeSelf) continue;
                return index;
            }
            return -1;
        }

        // a mod function that works with negative numbers
        // Mod(-1, 3) == 2; Mod(1, 3) == 1
        private static int Mod(int x, int m)
        {
            if (m == 0) return x;
            return (x % m + m) % m;
        }
    }
}