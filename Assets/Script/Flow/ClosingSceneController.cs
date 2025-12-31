using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class ClosingSceneController : MonoBehaviour
{
    [Header("Video")]
    public VideoPlayer videoPlayer;
    public string nextSceneName = "NameInput";

    private void Start()
    {
        Debug.Log("[ClosingScene] Closing scene started - playing video");

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
            Debug.Log("[ClosingScene] Video player initialized");
        }
        else
        {
            Debug.LogError("[ClosingScene] VideoPlayer not assigned!");
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("[ClosingScene] Video finished, loading next scene: " + nextSceneName);

        if (GameModeManager.Instance != null)
        {
            SceneManager.LoadScene(GameModeManager.Instance.nameInputScene);
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}
