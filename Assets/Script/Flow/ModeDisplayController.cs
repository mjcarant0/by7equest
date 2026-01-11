using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Unified Mode Display Controller - Handles all Mode Display Scene logic
/// 
/// FEATURES:
/// - Detects current game mode from GameModeManager
/// - Displays mode name with animated color transitions
/// - Fade-in and fade-out animations using Lerp
/// - Automatically loads next minigame after delay
/// 
/// COLOR MAPPING:
/// - Easy:   Green
/// - Medium: Yellow
/// - Hard:   Red
/// - God:    White
/// </summary>
public class ModeDisplayController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text component to display mode name (e.g., 'EASY MODE')")]
    public TextMeshProUGUI modeText;
    
    [Tooltip("Background Image component to animate color transitions")]
    public Image backgroundImage;
    
    [Tooltip("Canvas Group for fade animations (optional - will be added if missing)")]
    public CanvasGroup canvasGroup;

    [Header("Animation Settings")]
    [Tooltip("Total display duration in seconds")]
    [SerializeField] private float displayDuration = 2.5f;
    
    [Tooltip("Duration of fade-in animation in seconds")]
    [SerializeField] private float fadeInDuration = 0.8f;
    
    [Tooltip("Duration of fade-out animation in seconds")]
    [SerializeField] private float fadeOutDuration = 0.8f;
    
    [Tooltip("Delay between fade-in and fade-out")]
    [SerializeField] private float holdDuration = 0.5f;

    [Header("Mode Colors")]
    [SerializeField] private Color easyColor = Color.green;
    [SerializeField] private Color mediumColor = Color.yellow;
    [SerializeField] private Color hardColor = Color.red;
    [SerializeField] private Color godColor = Color.white;

    [Header("Next Scene")]
    [Tooltip("Scene to load after transition completes")]
    [SerializeField] private string nextScene = "GameStart";

    private void Awake()
    {
        if (canvasGroup == null && backgroundImage != null)
        {
            canvasGroup = backgroundImage.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
            {
                canvasGroup = backgroundImage.gameObject.AddComponent<CanvasGroup>();
                Debug.Log("[ModeDisplay] Added CanvasGroup to BackgroundImage");
            }
        }

        // Start invisible for fade-in effect
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    void Start()
    {
        // Validate GameModeManager exists
        if (GameModeManager.Instance == null)
        {
            Debug.LogError("[ModeDisplay] GameModeManager.Instance is null! Cannot display mode.");
            // Fallback: load next scene anyway after delay
            StartCoroutine(FallbackTransition());
            return;
        }

        // Get current mode from GameModeManager
        GameModeManager.GameMode currentMode = GameModeManager.Instance.GetCurrentMode();
        Debug.Log($"[ModeDisplay] Starting transition for mode: {currentMode}");

        // Start animated transition
        StartCoroutine(AnimatedTransition(currentMode));
    }

    /// <summary>
    /// Main animated transition coroutine
    /// Handles fade-in, display, and fade-out with color changes
    /// </summary>
    private IEnumerator AnimatedTransition(GameModeManager.GameMode mode)
    {
        // Get mode-specific properties
        Color modeColor = GetModeColor(mode);
        string modeName = mode.ToString().ToUpper();

        // Update text content and color
        if (modeText != null)
        {
            modeText.text = $"{modeName} MODE";
            modeText.color = modeColor;
            Debug.Log($"[ModeDisplay] Set text to '{modeText.text}' with color {modeColor}");
        }
        else
        {
            Debug.LogWarning("[ModeDisplay] modeText is not assigned!");
        }

        // Set background color (will be revealed by fade-in)
        if (backgroundImage != null)
        {
            backgroundImage.color = modeColor;
            Debug.Log($"[ModeDisplay] Set background color to {modeColor}");
        }
        else
        {
            Debug.LogWarning("[ModeDisplay] backgroundImage is not assigned!");
        }

        // === FADE IN ANIMATION ===
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            }
            
            yield return null;
        }

        // Ensure fully visible
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        Debug.Log($"[ModeDisplay] Fade-in complete, holding for {holdDuration}s");

        // === HOLD DURATION ===
        yield return new WaitForSeconds(holdDuration);

        // === FADE OUT ANIMATION ===
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            }
            
            yield return null;
        }

        // Ensure fully invisible
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        Debug.Log($"[ModeDisplay] Fade-out complete, loading {nextScene}");

        // Load next scene
        LoadNextScene();
    }

    /// <summary>
    /// Fallback transition without animations (if GameModeManager is missing)
    /// </summary>
    private IEnumerator FallbackTransition()
    {
        Debug.LogWarning("[ModeDisplay] Using fallback transition");
        
        if (modeText != null)
        {
            modeText.text = "LOADING...";
        }

        yield return new WaitForSeconds(displayDuration);
        LoadNextScene();
    }

    /// <summary>
    /// Get the color associated with a specific game mode
    /// </summary>
    private Color GetModeColor(GameModeManager.GameMode mode)
    {
        switch (mode)
        {
            case GameModeManager.GameMode.Easy:
                return easyColor;
            case GameModeManager.GameMode.Medium:
                return mediumColor;
            case GameModeManager.GameMode.Hard:
                return hardColor;
            case GameModeManager.GameMode.God:
                return godColor;
            default:
                Debug.LogWarning($"[ModeDisplay] Unknown mode: {mode}, using white");
                return Color.white;
        }
    }

    /// <summary>
    /// Load the next scene (typically GameStart which loads a random minigame)
    /// </summary>
    private void LoadNextScene()
    {
        Debug.Log($"[ModeDisplay] Loading {nextScene} scene");
        SceneManager.LoadScene(nextScene);
    }

    // === EDITOR TESTING METHODS ===
    [ContextMenu("Test Easy Mode")]
    private void TestEasy()
    {
        if (Application.isPlaying)
        {
            StopAllCoroutines();
            StartCoroutine(AnimatedTransition(GameModeManager.GameMode.Easy));
        }
    }

    [ContextMenu("Test Medium Mode")]
    private void TestMedium()
    {
        if (Application.isPlaying)
        {
            StopAllCoroutines();
            StartCoroutine(AnimatedTransition(GameModeManager.GameMode.Medium));
        }
    }

    [ContextMenu("Test Hard Mode")]
    private void TestHard()
    {
        if (Application.isPlaying)
        {
            StopAllCoroutines();
            StartCoroutine(AnimatedTransition(GameModeManager.GameMode.Hard));
        }
    }

    [ContextMenu("Test God Mode")]
    private void TestGod()
    {
        if (Application.isPlaying)
        {
            StopAllCoroutines();
            StartCoroutine(AnimatedTransition(GameModeManager.GameMode.God));
        }
    }
}