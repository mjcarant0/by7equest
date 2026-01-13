using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TimerUIController : MonoBehaviour
{
    [Header("Sprites (ordered start→end)")]
    public Sprite[] frames; // Expect length=9

    [Header("Optional Overrides")]
    [Tooltip("If true, will read initial duration from GameModeManager.timer on enable; otherwise uses GetTimeLimitForExternalCall.")]
    public bool useCurrentTimerValue = true;

    private SpriteRenderer sr;
    private float startDuration = 1f;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        CacheDuration();
        UpdateFrame();
    }

    private void Update()
    {
        UpdateFrame();
    }

    private void CacheDuration()
    {
        if (GameModeManager.Instance == null)
        {
            Debug.LogWarning("[TimerUI] GameModeManager is null; defaulting duration to 1s");
            startDuration = 1f;
            return;
        }

        if (useCurrentTimerValue && GameModeManager.Instance.timer > 0f)
        {
            startDuration = GameModeManager.Instance.timer;
        }
        else
        {
            startDuration = GameModeManager.Instance.GetTimeLimitForExternalCall();
        }

        if (startDuration <= 0f) startDuration = 1f;
    }

    private void UpdateFrame()
    {
        if (frames == null || frames.Length == 0 || sr == null)
        {
            return;
        }

        float remaining = 0f;
        if (GameModeManager.Instance != null)
        {
            remaining = Mathf.Max(0f, GameModeManager.Instance.timer);
        }

        // When timer hits exactly 0, show the last frame (frame 9)
        if (remaining == 0f)
        {
            sr.sprite = frames[frames.Length - 1];
            return;
        }

        float normalized = 1f - Mathf.Clamp01(remaining / startDuration); // 0 at start, 1 at end
        int idx = Mathf.Clamp(Mathf.FloorToInt(normalized * frames.Length), 0, frames.Length - 1);

        Sprite target = frames[idx];
        if (sr.sprite != target)
        {
            sr.sprite = target;
        }
    }
}
