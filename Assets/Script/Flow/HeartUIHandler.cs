using UnityEngine;
using UnityEngine.UI;

public class HeartUIHandler : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Drag your heart Image components here (up to 3 for 3 lives)")]
    public Image[] heartIcons; 

    [Header("Sprite Assets")]
    [Tooltip("Drag your full heart sprite from the Drive here")]
    public Sprite fullHeart;    
    [Tooltip("Drag your empty heart sprite from the Drive here")]
    public Sprite emptyHeart;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        HideHearts();
    }

    private void Start()
    {
        ValidateSetup();
    }

    private void ValidateSetup()
    {
        if (heartIcons == null || heartIcons.Length == 0)
        {
            Debug.LogError("[HeartUIHandler] No heart icons assigned! Please assign Image components in the Inspector.");
            return;
        }

        if (fullHeart == null)
        {
            Debug.LogError("[HeartUIHandler] Full heart sprite not assigned! Please assign the sprite from your Drive.");
        }

        if (emptyHeart == null)
        {
            Debug.LogError("[HeartUIHandler] Empty heart sprite not assigned! Please assign the sprite from your Drive.");
        }

        for (int i = 0; i < heartIcons.Length; i++)
        {
            if (heartIcons[i] == null)
            {
                Debug.LogError($"[HeartUIHandler] Heart icon at index {i} is null! Please assign all Image components.");
            }
        }

        Debug.Log($"[HeartUIHandler] Setup validated: {heartIcons.Length} heart icons, Full sprite: {fullHeart != null}, Empty sprite: {emptyHeart != null}");
    }

    public void ShowHearts()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        Debug.Log("[HeartUIHandler] Hearts shown");
    }

    public void HideHearts()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        Debug.Log("[HeartUIHandler] Hearts hidden");
    }

    public void UpdateHearts(int currentLives)
    {
        if (heartIcons == null || heartIcons.Length == 0)
        {
            Debug.LogWarning("[HeartUIHandler] Cannot update hearts - no icons assigned!");
            return;
        }

        Debug.Log($"[HeartUIHandler] Updating hearts display for {currentLives} lives");

        for (int i = 0; i < heartIcons.Length; i++)
        {
            if (heartIcons[i] == null)
            {
                Debug.LogWarning($"[HeartUIHandler] Heart icon at index {i} is null!");
                continue;
            }

            if (i < currentLives)
            {
                // Show full heart
                if (fullHeart != null)
                {
                    heartIcons[i].sprite = fullHeart;
                    heartIcons[i].color = Color.white; // Ensure full opacity
                }
                heartIcons[i].enabled = true;
            }
            else
            {
                // Show empty heart
                if (emptyHeart != null)
                {
                    heartIcons[i].sprite = emptyHeart;
                    heartIcons[i].color = new Color(1f, 1f, 1f, 0.5f); // Slightly transparent
                }
                heartIcons[i].enabled = true;
            }
        }
    }
}
