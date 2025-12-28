using UnityEngine;
using UnityEngine.UI;

public class HeartUIHandler : MonoBehaviour
{
    [Header("UI References")]
    public Image[] heartIcons; 

    [Header("Sprite Assets")]
    public Sprite fullHeart;    
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
        // Only validate if we're supposed to show hearts
    }

    public void ShowHearts()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    public void HideHearts()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void UpdateHearts(int currentLives)
    {
        if (heartIcons == null || heartIcons.Length == 0)
        {
            Debug.LogWarning("HeartUIHandler: Cannot update hearts - no icons assigned!");
            return;
        }

        for (int i = 0; i < heartIcons.Length; i++)
        {
            if (heartIcons[i] == null)
            {
                Debug.LogWarning($"HeartUIHandler: Heart icon at index {i} is null!");
                continue;
            }

            if (i < currentLives)
            {
                // Show full heart
                if (fullHeart != null)
                    heartIcons[i].sprite = fullHeart;
                heartIcons[i].enabled = true;
            }
            else
            {
                // Show empty heart
                if (emptyHeart != null)
                    heartIcons[i].sprite = emptyHeart;
                heartIcons[i].enabled = true;
            }
        }
    }
}