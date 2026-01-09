using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIButtonSFX : MonoBehaviour
{
    public AudioClip clickSound;

    public void PlayClickAndLoad(string sceneName)
    {
        // Play click sound via SoundManager
        SoundManager.Instance.PlaySFX(clickSound);

        // Load next scene immediately
        SceneManager.LoadScene(sceneName);
    }
}
