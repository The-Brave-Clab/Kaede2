using System.Collections.Generic;
using System.Linq;
using Kaede2.ScriptableObjects;
using TMPro;
using UnityEngine;

namespace Kaede2
{
    public class MainStoryChapter : MonoBehaviour, MasterScenarioInfo.IProvider
    {
        [SerializeField]
        private TextMeshProUGUI titleText;

        [SerializeField]
        private bool isSub; // we could just get this from the chapter ID though

        [SerializeField]
        private int chapterId;

        public string Text => titleText.text;

        public IEnumerable<MasterScenarioInfo.ScenarioInfo> Provide()
        {
            return MasterScenarioInfo.Instance.Data
                .Where(si => si.ChapterId == chapterId);
        }
    }
}