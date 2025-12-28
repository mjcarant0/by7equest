using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TempTransitionController : MonoBehaviour
{
    public static TempTransitionController Instance { get; private set; }

    [Header("UI References")]
    public Image backgroundImage; 
    public CanvasGroup canvasGroup; 

    [Header("Transition Settings")]
    [SerializeField] private float displayDuration = 2.5f;

    [Header("Mode Colors (Task 3)")]
    [SerializeField] private Color easyColor = Color.green;
    [SerializeField] private Color mediumColor = Color.yellow;
    [SerializeField] private Color hardColor = Color.red;
    [SerializeField] private Color godColor = Color.white;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Validate
        if (backgroundImage == null)
            Debug.LogError("TempTransition: Background Image not assigned!");
        
        // Get or add CanvasGroup
        if (canvasGroup == null)
        {
            canvasGroup = GetComponentInChildren<CanvasGroup>();
            if (canvasGroup == null)
            {
                Canvas canvas = GetComponentInChildren<Canvas>();
                if (canvas != null)
                    canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
            }
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    public void ShowTransition(GameModeManager.GameMode mode, Action onComplete = null)
    {
        if (backgroundImage == null)
        {
            Debug.LogError("Cannot show transition - Background Image not assigned");
            onComplete?.Invoke();
            return;
        }

        Color modeColor = GetModeColor(mode);
        StartCoroutine(TransitionCoroutine(modeColor, onComplete));
    }

    private Color GetModeColor(GameModeManager.GameMode mode)
    {
        switch (mode)
        {
            case GameModeManager.GameMode.Easy: return easyColor;
            case GameModeManager.GameMode.Medium: return mediumColor;
            case GameModeManager.GameMode.Hard: return hardColor;
            case GameModeManager.GameMode.God: return godColor;
            default: return Color.white;
        }
    }

    private IEnumerator TransitionCoroutine(Color modeColor, Action onComplete)
    {
        backgroundImage.color = modeColor;

        float elapsed = 0f;
        float fadeInDuration = displayDuration / 2f;
        
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }
        
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(0.3f);

        elapsed = 0f;
        float fadeOutDuration = displayDuration / 2f;
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            yield return null;
        }
        
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        onComplete?.Invoke();
    }

    [ContextMenu("Test Easy Transition")]
    private void TestEasy() => ShowTransition(GameModeManager.GameMode.Easy);

    [ContextMenu("Test Medium Transition")]
    private void TestMedium() => ShowTransition(GameModeManager.GameMode.Medium);

    [ContextMenu("Test Hard Transition")]
    private void TestHard() => ShowTransition(GameModeManager.GameMode.Hard);

    [ContextMenu("Test God Transition")]
    private void TestGod() => ShowTransition(GameModeManager.GameMode.God);
}
