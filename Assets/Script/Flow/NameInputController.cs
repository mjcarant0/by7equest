using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NameInputController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField playerNameInput;
    public Button submitButton;
    public TextMeshProUGUI warningText;

    private bool submitInProgress = false;

    void Start()
    {
        // Reset state for new player session
        submitInProgress = false;
        
        if (playerNameInput != null)
        {
            playerNameInput.text = ""; // Clear previous player's name
            playerNameInput.characterLimit = 10;
            playerNameInput.onValueChanged.AddListener(OnNameChanged);
        }

        if (submitButton != null)
        {
            submitButton.onClick.AddListener(OnSubmitClicked);
            submitButton.interactable = false; // Start disabled until name is entered
        }
    }

    public void OnSubmitClicked()
    {
        if (submitInProgress) return;
        submitInProgress = true;
        if (submitButton != null) submitButton.interactable = false;

        string playerName = playerNameInput != null ? playerNameInput.text : "Anonymous";
        
        if (string.IsNullOrWhiteSpace(playerName))
        {
            playerName = "Anonymous";
        }

        // Ensure name does not exceed 10 characters
        if (playerName.Length > 10)
        {
            playerName = playerName.Substring(0, 10);
        }

        Debug.Log($"[NameInput] Player name submitted: {playerName}");

        // CRITICAL: Cache score NOW before GameModeManager might be destroyed
        int cachedScore = 0;
        if (GameModeManager.Instance != null)
        {
            cachedScore = GameModeManager.Instance.score;
            Debug.Log($"[NameInput] Cached score from GameModeManager: {cachedScore}");
        }
        else
        {
            Debug.LogWarning("[NameInput] GameModeManager.Instance is already null, score will be 0");
        }

        // Always create fresh connection to ensure unique account for each submission
        if (PlayFabDatabase.Instance != null)
        {
            Debug.Log("[NameInput] Creating fresh database connection...");
            
            // Pass save logic as callback to execute immediately after connection
            PlayFabDatabase.Instance.ConnectToDatabase(playerName, () => {
                Debug.Log("[NameInput] ✓ Connection callback fired! Saving data...");
                OnConnectionSuccess(playerName, cachedScore);
            });
        }
        else
        {
            Debug.LogError("[NameInput] PlayFabDatabase.Instance is null!");
        }
    }

    private void OnConnectionSuccess(string playerName, int cachedScore)
    {
        Debug.Log($"[NameInput] Executing save with name={playerName}, score={cachedScore}");
        
        if (cachedScore <= 0)
        {
            Debug.LogWarning("[NameInput] WARNING: Score is 0 or negative! Check if game was played.");
        }
        
        PlayFabDatabase.Instance.SaveToDatabase(playerName, cachedScore, (success) =>
        {
            if (success)
            {
                Debug.Log($"[NameInput] ✓ Data saved to database successfully! ({playerName} - {cachedScore})");
                
                // Refresh leaderboard if the scene is already loaded
                ScoreSceneLeaderboard leaderboardUI = UnityEngine.Object.FindFirstObjectByType<ScoreSceneLeaderboard>();
                if (leaderboardUI != null)
                {
                    Debug.Log("[NameInput] Refreshing leaderboard UI after save...");
                    leaderboardUI.RefreshLeaderboard();
                }
            }
            else
            {
                Debug.LogError($"[NameInput] ✗ Failed to save data to database! ({playerName} - {cachedScore})");
            }
            
            // Reset submit flag before finalizing
            submitInProgress = false;
            if (submitButton != null) submitButton.interactable = true;
            
            // Destroy old GameModeManager instance now that score is saved (safe, we cached the score)
            if (GameModeManager.Instance != null)
            {
                GameModeManager.Instance.DestroyOldInstance();
            }
            
            // Continue to finalize session regardless of save result
            if (GameModeManager.Instance != null)
            {
                GameModeManager.Instance.FinalizeSession(playerName);
            }
            else
            {
                Debug.Log("[NameInput] GameModeManager already destroyed, returning to landing page manually");
                UnityEngine.SceneManagement.SceneManager.LoadScene("LandingPage");
            }
        });
    }

    private void OnNameChanged(string value)
    {
        if (submitButton != null)
        {
            bool hasText = !string.IsNullOrWhiteSpace(value);
            submitButton.interactable = hasText;
        }
    }
}