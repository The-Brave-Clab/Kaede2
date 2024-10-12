using Kaede2.Input;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Kaede2
{
    public class OpeningMoviePlayer : MonoBehaviour
    {
        [SerializeField]
        private VideoPlayer videoPlayer;

        [SerializeField]
        private RawImage image;

        [SerializeField]
        private AspectRatioFitter aspectRatioFitter;

        public UnityEvent onOpeningMovieFinished;

        private RenderTexture videoRenderTexture;

        private void Awake()
        {
            videoPlayer.loopPointReached += _ => { Stop(); };
        }

        private void OnEnable()
        {
            InputManager.InputAction.SplashScreen.Enable();
            InputManager.InputAction.SplashScreen.Skip.performed += OnSkipPerformed;
#if UNITY_IOS && !UNITY_EDITOR
            UnityEngine.iOS.Device.hideHomeButton = true;
#endif
        }

        private void OnDisable()
        {
#if UNITY_IOS && !UNITY_EDITOR
            UnityEngine.iOS.Device.hideHomeButton = false;
#endif
            if (InputManager.InputAction != null)
            {
                InputManager.InputAction.SplashScreen.Skip.performed -= OnSkipPerformed;
                InputManager.InputAction.SplashScreen.Disable();
            }
        }

        private void OnDestroy()
        {
            if (videoRenderTexture != null)
            {
                videoRenderTexture.Release();
                videoRenderTexture = null;
            }
        }

        private void OnSkipPerformed(InputAction.CallbackContext obj)
        {
            Stop();
        }

        private void RecreateVideoRenderTexture(VideoClip clip)
        {
            if (videoRenderTexture != null)
            {
                videoRenderTexture.Release();
                videoRenderTexture = null;
            }

            if (clip == null) return;

            videoRenderTexture = new RenderTexture((int)clip.width, (int)clip.height, 0);
            videoRenderTexture.Create();
        }

        public void Play(VideoClip clip)
        {
            if (clip == null)
            {
                Stop();
                return;
            }

            RecreateVideoRenderTexture(clip);

            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = videoRenderTexture;
            image.texture = videoRenderTexture;
            aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            aspectRatioFitter.aspectRatio = (float)clip.width / clip.height;

            image.gameObject.SetActive(true);

            videoPlayer.clip = clip;
            videoPlayer.Play();
        }

        public void Stop()
        {
            if (videoPlayer.isPlaying)
                videoPlayer.Pause();

            OnDisable();

            onOpeningMovieFinished?.Invoke();
        }
    }
}
