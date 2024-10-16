using UnityEngine;

namespace Kaede2.Audio
{
    [RequireComponent(typeof(AudioListener))]
    public class AudioListenerKeepAlive : MonoBehaviour
    {
        private void Awake()
        {
            if (gameObject.GetComponent<AudioListener>() == null)
                gameObject.AddComponent<AudioListener>();
            DontDestroyOnLoad(gameObject);
        }
    }
}