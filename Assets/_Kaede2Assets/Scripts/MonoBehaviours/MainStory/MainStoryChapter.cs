using System;
using System.Collections.Generic;
using System.Linq;
using Kaede2.ScriptableObjects;
using TMPro;
using UnityEngine;

namespace Kaede2
{
    [ExecuteAlways]
    public class MainStoryChapter : MonoBehaviour, MasterScenarioInfo.IProvider
    {
        [SerializeField]
        private TextMeshProUGUI titleText;

        [SerializeField]
        private bool isSub; // we could just get this from the chapter ID though

        [SerializeField]
        private int chapterId;

        [SerializeField]
        private string text;

        [SerializeField]
        private Color textColor;

        public string Text
        {
            get => text;
            set => text = value;
        }
        public bool IsSub => isSub;

        public IEnumerable<MasterScenarioInfo.ScenarioInfo> Provide()
        {
            return MasterScenarioInfo.Instance.Data
                .Where(si => si.ChapterId == chapterId);
        }

        private void Update()
        {
            titleText.text = text.Length > 1 ? $"{text[..1]}<color=black>{text[1..]}</color>" : text;
            titleText.color = textColor;
        }
    }
}