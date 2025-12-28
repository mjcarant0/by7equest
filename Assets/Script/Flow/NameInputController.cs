using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.FinalizeSession(playerName);
        }
        else
        {
            Debug.LogError("[NameInput] GameModeManager.Instance is null!");
        }
    }
}