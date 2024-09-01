using System;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2.UI
{
    public class LabeledListLayout : LayoutElement
    {
        [SerializeField]
        private RectTransform childLayoutGroup;

        public override bool ignoreLayout => false;
        public override float preferredHeight => childLayoutGroup.rect.height;

        private ContentSizeFitter contentSizeFitter;

        private float lastPreferredHeight;
        protected override void Awake()
        {
            base.Awake();

            contentSizeFitter = childLayoutGroup.GetComponent<ContentSizeFitter>();
            SetDirty();

            lastPreferredHeight = preferredHeight;
        }

        private void Update()
        {
            if (Math.Abs(lastPreferredHeight - preferredHeight) > 0.01f)
            {
                lastPreferredHeight = preferredHeight;
                SetDirty();
            }
        }
    }
}