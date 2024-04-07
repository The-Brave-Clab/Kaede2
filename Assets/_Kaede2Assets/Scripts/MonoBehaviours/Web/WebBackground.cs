#if UNITY_WEBGL && !UNITY_EDITOR
using Kaede2.Scenario;
using Kaede2.ScriptableObjects;
using UnityEngine.SceneManagement;
#endif
using Kaede2.Scenario.Framework.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2.Web
{
    public class WebBackground : SingletonMonoBehaviour<WebBackground>
    {
        [SerializeField]
        private Button playButton;

        [SerializeField]
        private Button replayButton;

        [SerializeField]
        private Button nextButton;

#if UNITY_WEBGL && !UNITY_EDITOR
        private Status currentStatus;
        public static Status CurrentStatus => Instance.currentStatus;

        // State machine:
        // AnyState -> ReadyToPlay: when WebInterop.ResetPlayer is called
        // ReadyToPlay -> Hidden: when playButton is clicked
        // Hidden -> Finished: when Commands.End is executed
        // Finished -> Hidden: when replayButton or nextButton is clicked

        public enum Status
        {
            Initial, // when the app started the background is shown with logos (logos are not DontDestroyOnLoad)
            ReadyToPlay, // a play button is shown, blocking the init_end command
            Hidden, // the background is hidden while the scenario is playing
            Finished // the scenario is finished, the background is shown again with a replay button and a next button (if available)
        }

        protected override void Awake()
        {
            base.Awake();

            playButton.onClick.AddListener(() =>
            {
                UpdateStatusInternal(Status.Hidden);
            });
            replayButton.onClick.AddListener(() =>
            {
                UpdateStatusInternal(Status.Hidden);
                SceneManager.LoadScene("ScenarioScene", LoadSceneMode.Single);
            });
            nextButton.onClick.AddListener(() =>
            {
                UpdateStatusInternal(Status.Hidden);
                PlayerScenarioModule.GlobalScenarioName = MasterScenarioInfo.GetNextScenarioInfo(PlayerScenarioModule.GlobalScenarioName).ScenarioName;
                WebInterop.OnScenarioChanged(PlayerScenarioModule.GlobalScenarioName);
                SceneManager.LoadScene("ScenarioScene", LoadSceneMode.Single);
            });

            UpdateStatusInternal(Status.Initial);
            DontDestroyOnLoad(gameObject);
        }

        public static void UpdateStatus(Status status)
        {
            Instance.UpdateStatusInternal(status);
        }

        private void UpdateStatusInternal(Status status)
        {
            currentStatus = status;
            switch (status)
            {
                case Status.Initial:
                    gameObject.SetActive(true);
                    playButton.gameObject.SetActive(false);
                    replayButton.gameObject.SetActive(false);
                    nextButton.gameObject.SetActive(false);
                    break;
                case Status.ReadyToPlay:
                    gameObject.SetActive(true);
                    playButton.gameObject.SetActive(true);
                    replayButton.gameObject.SetActive(false);
                    nextButton.gameObject.SetActive(false);
                    break;
                case Status.Hidden:
                    gameObject.SetActive(false);
                    playButton.gameObject.SetActive(false);
                    replayButton.gameObject.SetActive(false);
                    nextButton.gameObject.SetActive(false);
                    break;
                case Status.Finished:
                    gameObject.SetActive(true);
                    playButton.gameObject.SetActive(false);
                    replayButton.gameObject.SetActive(true);
                    nextButton.gameObject.SetActive(MasterScenarioInfo.GetNextScenarioInfo(PlayerScenarioModule.GlobalScenarioName) != null);
                    break;
            }
        }
#endif
    }
}
