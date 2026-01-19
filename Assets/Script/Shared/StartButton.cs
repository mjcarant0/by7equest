using UnityEngine;
using PlayFab;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
    [Header("UI Elements to Hide on Start")]
    public GameObject[] uiElementsToHide;

    void Start()
    {
        Debug.Log("[StartButton] StartButton initialized");
    }

    public void OnStartPressed()
    {
        Debug.Log("[StartButton] Start button pressed!");
        
        // Hide UI elements if assigned
        if (uiElementsToHide != null && uiElementsToHide.Length > 0)
        {
            foreach (GameObject element in uiElementsToHide)
            {
                if (element != null)
                {
                    element.SetActive(false);
                }
            }
        }

        // Create managers if they don't exist
        if (MinigameRandomizer.Instance == null)
        {
            GameObject temp = new GameObject("MinigameRandomizer");
            temp.AddComponent<MinigameRandomizer>();
            Debug.Log("[StartButton] Created MinigameRandomizer");
        }

        if (GameModeManager.Instance == null)
        {
            GameObject gmm = new GameObject("GameModeManager");
            gmm.AddComponent<GameModeManager>();
            Debug.Log("[StartButton] Created GameModeManager");
        }

        // Reset game state for new session (clears flags like isGameOver)
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.ResetForNewRun();
            Debug.Log("[StartButton] New run initialized via GameModeManager.ResetForNewRun()");
        }

        // Reset hearts/lives for a fresh run
        HeartUIHandler.StaticResetLives();

        // Reset PlayFab authentication so a new player can start clean
        try
        {
            PlayFabClientAPI.ForgetAllCredentials();
        }
        catch { /* ignore if SDK not initialized yet */ }

        // If using device-stable CustomId, clear it to avoid reusing the last player's account
        PlayerPrefs.DeleteKey("PlayFabCustomId");
        PlayerPrefs.Save();

        // Load Opening Scene (train animation - only at game start)
        Debug.Log("[StartButton] Loading Opening Scene");
        SceneManager.LoadScene("OpeningScene");
    }
}