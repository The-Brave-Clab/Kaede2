using Kaede2.Scenario.Framework.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede2.UI.Web
{
    public class WebBackground : Singleton<WebBackground>
    {
        [SerializeField]
        private Button playButton;

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
#if !UNITY_WEBGL || UNITY_EDITOR
            Debug.LogError("WebBackground should only be used in Web builds");
            Destroy(gameObject);
#endif
            base.Awake();

            playButton.onClick.AddListener(() => { UpdateStatusInternal(Status.Hidden); });

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
                    break;
                case Status.ReadyToPlay:
                    gameObject.SetActive(true);
                    playButton.gameObject.SetActive(true);
                    break;
                case Status.Hidden:
                    gameObject.SetActive(false);
                    playButton.gameObject.SetActive(false);
                    break;
                case Status.Finished:
                    gameObject.SetActive(true);
                    playButton.gameObject.SetActive(false);
                    break;
            }
        }
#endif
    }
}
