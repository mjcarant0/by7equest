using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class OpeningSceneController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string nextSceneName = "GameStart";

    void Start()
    {
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        SceneManager.LoadScene(nextSceneName);
    }

    void OnDestroy()
    {
        videoPlayer.loopPointReached -= OnVideoFinished;
    }
}
