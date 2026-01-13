using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NameInputController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField playerNameInput;
    public Button submitButton;

    void Start()
    {
        if (submitButton != null)
        {
            submitButton.onClick.AddListener(OnSubmitClicked);
        }
    }

    public void OnSubmitClicked()
    {
        string playerName = playerNameInput != null ? playerNameInput.text : "Anonymous";
        
        if (string.IsNullOrWhiteSpace(playerName))
        {
            playerName = "Anonymous";
        }

        Debug.Log($"[NameInput] Player name submitted: {playerName}");

        // Ensure database is connected before saving
        if (PlayFabDatabase.Instance != null && !PlayFabDatabase.Instance.IsConnected())
        {
            Debug.Log("[NameInput] Connecting to database...");
            PlayFabDatabase.Instance.ConnectToDatabase(playerName);
            // Wait a moment for connection, then proceed
            StartCoroutine(WaitForConnectionAndSave(playerName));
            return;
        }

        // Get the final score from GameModeManager
        if (GameModeManager.Instance != null)
        {
            int finalScore = GameModeManager.Instance.score;
            
            // Save to database (ONLY happens when Enter button is clicked)
            if (PlayFabDatabase.Instance != null && PlayFabDatabase.Instance.IsConnected())
            {
                PlayFabDatabase.Instance.SaveToDatabase(playerName, finalScore, (success) =>
                {
                    if (success)
                    {
                        Debug.Log("[NameInput] ✓ Data saved to database successfully!");
                    }
                    else
                    {
                        Debug.LogError("[NameInput] ✗ Failed to save data to database!");
                    }
                    
                    // Continue to finalize session regardless of save result
                    GameModeManager.Instance.FinalizeSession(playerName);
                });
            }
            else
            {
                Debug.LogWarning("[NameInput] Database not connected - data not saved");
                GameModeManager.Instance.FinalizeSession(playerName);
            }
        }
        else
        {
            Debug.LogError("[NameInput] GameModeManager.Instance is null!");
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
                
                PlayFabDatabase.Instance.SaveToDatabase(playerName, finalScore, (success) =>
                {
                    if (success)
                    {
                        Debug.Log("[NameInput] ✓ Data saved to database successfully!");
                    }
                    else
                    {
                        Debug.LogError("[NameInput] ✗ Failed to save data to database!");
                    }
                    
                    // Continue to finalize session regardless of save result
                    GameModeManager.Instance.FinalizeSession(playerName);
                });
            }
        }
        else
        {
            Debug.LogError("[NameInput] Database connection timeout - data not saved");
            if (GameModeManager.Instance != null)
            {
                GameModeManager.Instance.FinalizeSession(playerName);
            }
        }
    }
}