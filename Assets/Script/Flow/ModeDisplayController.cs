using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ModeDisplayController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI modeText;
    public UnityEngine.UI.Image backgroundImage; 

    [Header("Settings")]
    public float displayDuration = 2f;

    void Start()
    {
        if (GameModeManager.Instance == null)
        {
            Debug.LogError("[ModeDisplay] GameModeManager not found!");
            return;
        }

        string modeName = GameModeManager.Instance.GetCurrentMode().ToString().ToUpper();
        
        if (modeText != null)
        {
            modeText.text = $"{modeName} MODE";
            
            Color modeColor = GetModeColor(GameModeManager.Instance.GetCurrentMode());
            modeText.color = modeColor;
            
            if (backgroundImage != null)
            {
                backgroundImage.color = modeColor;
            }
        }

        Invoke(nameof(LoadGameStart), displayDuration);
    }

    Color GetModeColor(GameModeManager.GameMode mode)
    {
        switch (mode)
        {
            case GameModeManager.GameMode.Easy:
                return Color.green;
            case GameModeManager.GameMode.Medium:
                return Color.yellow;
            case GameModeManager.GameMode.Hard:
                return Color.red;
            case GameModeManager.GameMode.God:
                return Color.white;
            default:
                return Color.white;
        }
    }

    void LoadGameStart()
    {
        Debug.Log("[ModeDisplay] Loading Game Start Scene");
        SceneManager.LoadScene("GameStart");
    }
}