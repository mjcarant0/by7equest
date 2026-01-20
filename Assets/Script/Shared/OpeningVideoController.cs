using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

/// <summary>
/// Controls the opening scene video playback.
/// Uses URL mode for WebGL compatibility.
/// </summary>
public class OpeningVideoController : MonoBehaviour
{
    [Header("Video")]
    public VideoPlayer videoPlayer;
    public string videoFileName = "opening-scene.mp4";
    
    [Header("Scene Flow")]
    public string nextSceneName = "ModeDisplay";

    private void Start()
    {
        Debug.Log("[OpeningVideo] Opening video scene started");

        // Ensure next minigame is determined
        if (MinigameRandomizer.Instance == null)
        {
            GameObject temp = new GameObject("MinigameRandomizer");
            temp.AddComponent<MinigameRandomizer>();
        }

        string nextMinigame = MinigameRandomizer.Instance.GetNextMinigameName();
        Debug.Log($"[OpeningVideo] Next minigame will be: {nextMinigame}");

        if (videoPlayer != null)
        {
            // Force clear any hardcoded URL from Inspector
            videoPlayer.url = string.Empty;
            
            // Set video source to URL mode for WebGL compatibility
            videoPlayer.source = VideoSource.Url;
            
            // Build path to video in StreamingAssets (dynamic, works on any computer)
            string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName);
            videoPlayer.url = videoPath;
            
            // Subscribe to events
            videoPlayer.prepareCompleted += OnVideoPrepared;
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.errorReceived += OnVideoError;
            
            // Prepare the video
            videoPlayer.Prepare();
            
            Debug.Log($"[OpeningVideo] Video player initialized with URL: {videoPath}");
            
            // Fallback: if video doesn't play after 8 seconds, skip to next scene
            Invoke(nameof(FallbackSkip), 8f);
        }
        else
        {
            Debug.LogError("[OpeningVideo] VideoPlayer not assigned! Skipping to next scene.");
            Invoke(nameof(LoadNextScene), 0.5f);
        }
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        Debug.Log("[OpeningVideo] Video prepared, starting playback");
        vp.Play();
    }

    private void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogError($"[OpeningVideo] Video error: {message}");
        LoadNextScene();
    }

    private void FallbackSkip()
    {
        if (videoPlayer != null && !videoPlayer.isPlaying)
        {
            Debug.LogWarning("[OpeningVideo] Video didn't play, skipping to next scene");
            LoadNextScene();
        }
    }

    private void LoadNextScene()
    {
        CancelInvoke(nameof(FallbackSkip));
        SceneManager.LoadScene(nextSceneName);
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("[OpeningVideo] Video finished, loading next scene");
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
