using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIButtonSFX : MonoBehaviour
{
    public AudioClip clickSound; // Assign your click sound here

    public void PlayClickAndLoad(string sceneName)
    {
        // Play click sound via SoundManager
        SoundManager.Instance.PlaySFX(clickSound);

        // Load next scene immediately
        SceneManager.LoadScene(sceneName);
    }
}
