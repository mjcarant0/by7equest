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

        // Always create fresh connection to ensure unique account for each submission
        if (PlayFabDatabase.Instance != null)
        {
            Debug.Log("[NameInput] Creating fresh database connection...");
            PlayFabDatabase.Instance.ConnectToDatabase(playerName);
            StartCoroutine(WaitForConnectionAndSave(playerName));
        }
        else
        {
            Debug.LogError("[NameInput] PlayFabDatabase.Instance is null!");
        }
    }

    private IEnumerator WaitForConnectionAndSave(string playerName)
    {
        // Wait up to 5 seconds for connection
        float timeout = 5f;
        float elapsed = 0f;

        while (!PlayFabDatabase.Instance.IsConnected() && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.5f);
            elapsed += 0.5f;
        }

        if (PlayFabDatabase.Instance.IsConnected())
        {
            Debug.Log("[NameInput] Database connected, saving data...");
            
            // Get the final score from GameModeManager
            if (GameModeManager.Instance != null)
            {
                int finalScore = GameModeManager.Instance.score;
                Debug.Log($"[NameInput] Score to save: {finalScore}");
                
                if (finalScore <= 0)
                {
                    Debug.LogWarning("[NameInput] WARNING: Score is 0 or negative! Check if GameModeManager.score was set properly.");
                }
                
                PlayFabDatabase.Instance.SaveToDatabase(playerName, finalScore, (success) =>
                {
                    if (success)
                    {
                        Debug.Log($"[NameInput] ✓ Data saved to database successfully! ({playerName} - {finalScore})");
                    }
                    else
                    {
                        Debug.LogError($"[NameInput] ✗ Failed to save data to database! ({playerName} - {finalScore})");
                    }
                    
                    // Reset submit flag before finalizing
                    submitInProgress = false;
                    if (submitButton != null) submitButton.interactable = true;
                    
                    // Destroy old GameModeManager instance now that score is saved
                    GameModeManager.Instance.DestroyOldInstance();
                    
                    // Continue to finalize session regardless of save result
                    GameModeManager.Instance.FinalizeSession(playerName);
                });
            }
            else
            {
                Debug.LogError("[NameInput] GameModeManager.Instance is null!");
                submitInProgress = false;
                if (submitButton != null) submitButton.interactable = true;
            }
        }
        else
        {
            Debug.LogError("[NameInput] Database connection timeout - data not saved");
            
            // Reset submit flag
            submitInProgress = false;
            if (submitButton != null) submitButton.interactable = true;
            
            if (GameModeManager.Instance != null)
            {
                GameModeManager.Instance.FinalizeSession(playerName);
            }
        }
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