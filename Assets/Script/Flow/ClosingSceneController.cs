using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class ClosingSceneController : MonoBehaviour
{
    [Header("Video")]
    public VideoPlayer videoPlayer;
    public string videoFileName = "closing-scene.mp4";
    public string nextSceneName = "NameInput";

    private void Start()
    {
        Debug.Log("[ClosingScene] Closing scene started - playing video");

        if (videoPlayer != null)
        {
            // Force clear any hardcoded URL from Inspector
            videoPlayer.url = string.Empty;
            
            // Set video source to URL mode for WebGL compatibility
            videoPlayer.source = VideoSource.Url;
            
            // Build path to video in StreamingAssets (dynamic, works on any computer)
            string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName);
            videoPlayer.url = videoPath;
            
            // Validate that we're not using a hardcoded absolute path
            if (videoPath.Contains(":") && !videoPath.StartsWith("http"))
            {
                Debug.LogWarning($"[ClosingScene] Detected absolute path: {videoPath}. This may cause issues on other computers.");
            }
            
            // Subscribe to events
            videoPlayer.prepareCompleted += OnVideoPrepared;
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.errorReceived += OnVideoError;
            
            // Prepare the video
            videoPlayer.Prepare();
            
            Debug.Log($"[ClosingScene] Video player initialized with URL: {videoPath}");
            
            // Fallback: if video doesn't play after 8 seconds, skip to next scene
            Invoke(nameof(FallbackSkip), 8f);
        }
        else
        {
            Debug.LogError("[ClosingScene] VideoPlayer not assigned! Skipping to next scene.");
            Invoke(nameof(LoadNextScene), 0.5f);
        }
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        Debug.Log("[ClosingScene] Video prepared, starting playback");
        vp.Play();
    }

    private void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogError($"[ClosingScene] Video error: {message}");
        LoadNextScene();
    }

    private void FallbackSkip()
    {
        if (videoPlayer != null && !videoPlayer.isPlaying)
        {
            Debug.LogWarning("[ClosingScene] Video didn't play, skipping to next scene");
            LoadNextScene();
        }
    }

    private void LoadNextScene()
    {
        CancelInvoke(nameof(FallbackSkip));
        
        if (GameModeManager.Instance != null)
        {
            SceneManager.LoadScene(GameModeManager.Instance.nameInputScene);
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("[ClosingScene] Video finished, loading next scene");
        LoadNextScene();
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.errorReceived -= OnVideoError;
        }
        
        CancelInvoke();
    }
}
