using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// Add this to your VideoPlayer GameObject to diagnose video playback issues.
/// Check the Console for detailed information.
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public class VideoPlayerDiagnostics : MonoBehaviour
{
    private VideoPlayer vp;

    void Start()
    {
        vp = GetComponent<VideoPlayer>();
        
        Debug.Log("=== VIDEO PLAYER DIAGNOSTICS ===");
        Debug.Log($"VideoPlayer found: {vp != null}");
        
        if (vp != null)
        {
            Debug.Log($"Source: {vp.source}");
            Debug.Log($"URL: {vp.url}");
            Debug.Log($"Render Mode: {vp.renderMode}");
            Debug.Log($"Play On Awake: {vp.playOnAwake}");
            Debug.Log($"Is Playing: {vp.isPlaying}");
            Debug.Log($"Is Prepared: {vp.isPrepared}");
            Debug.Log($"Frame Count: {vp.frameCount}");
            Debug.Log($"Clip: {vp.clip}");
            
            if (vp.renderMode == VideoRenderMode.CameraNearPlane || vp.renderMode == VideoRenderMode.CameraFarPlane)
            {
                Debug.Log($"Target Camera: {vp.targetCamera}");
                if (vp.targetCamera == null)
                {
                    Debug.LogError("ISSUE: Render Mode is Camera but Target Camera is NULL!");
                }
            }
            else if (vp.renderMode == VideoRenderMode.RenderTexture)
            {
                Debug.Log($"Target Texture: {vp.targetTexture}");
                if (vp.targetTexture == null)
                {
                    Debug.LogError("ISSUE: Render Mode is Render Texture but Target Texture is NULL!");
                }
            }
            else if (vp.renderMode == VideoRenderMode.MaterialOverride)
            {
                Debug.Log($"Target Material Renderer: {vp.targetMaterialRenderer}");
                if (vp.targetMaterialRenderer == null)
                {
                    Debug.LogError("ISSUE: Render Mode is Material Override but Target Material Renderer is NULL!");
                }
            }
        }
        
        Debug.Log("=== END DIAGNOSTICS ===");
    }

    void Update()
    {
        if (vp != null && Time.frameCount % 60 == 0) // Log every 60 frames
        {
            Debug.Log($"[VideoStatus] Playing: {vp.isPlaying} | Frame: {vp.frame}/{vp.frameCount} | Time: {vp.time:F2}s");
        }
    }
}
