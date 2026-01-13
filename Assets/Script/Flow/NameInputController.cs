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
        if (playerNameInput != null)
        {
            playerNameInput.characterLimit = 10;
            playerNameInput.onValueChanged.AddListener(OnNameChanged);
        }

        if (submitButton != null)
        {
            submitButton.onClick.AddListener(OnSubmitClicked);
            submitButton.interactable = false;
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

    private void OnNameChanged(string value)
    {
        if (submitButton != null)
        {
            bool hasText = !string.IsNullOrWhiteSpace(value);
            submitButton.interactable = hasText;
        }
    }
}