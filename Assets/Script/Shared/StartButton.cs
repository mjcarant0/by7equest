using UnityEngine;
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

        // Reset game state for new session
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.lives = 3;
            GameModeManager.Instance.score = 0;
            GameModeManager.Instance.lastMinigameScore = 0;
            GameModeManager.Instance.currentMode = GameModeManager.GameMode.Easy;
            GameModeManager.Instance.minigamesCompletedInMode = 0;
            Debug.Log("[StartButton] Game state reset to Easy mode");
        }

        // Load Opening Scene (train animation - only at game start)
        Debug.Log("[StartButton] Loading Opening Scene");
        SceneManager.LoadScene("OpeningScene");
    }
}