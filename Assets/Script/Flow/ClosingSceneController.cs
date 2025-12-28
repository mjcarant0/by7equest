using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class ClosingSceneController : MonoBehaviour
{
    [Header("Phase 1: Game Over Display")]
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI finalDifficultyText;
    public GameObject gameOverPanel; 
    
    [Header("Phase 2: Name Input")]
    public TMP_InputField playerNameInput;
    public Button submitButton;
    public GameObject nameInputPanel; 

    [Header("Settings")]
    public float gameOverDisplayDuration = 3f; 

    [Header("Optional Animation")]
    public GameEndAnimation gameEndAnimation;

    private bool hasSubmitted = false;

    void Start()
    {
        // Phase 1: Show game over, hide name input
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        
        if (nameInputPanel != null)
            nameInputPanel.SetActive(false);

        if (playerNameInput == null)
            Debug.LogError("[ClosingScene] Player Name Input not assigned!");

        if (submitButton == null)
            Debug.LogError("[ClosingScene] Submit Button not assigned!");
        else
            submitButton.onClick.AddListener(OnSubmitClicked);

        if (gameEndAnimation != null)
        {
            gameEndAnimation.OnAnimationFinished += OnAnimationComplete;
        }
        else
        {
            OnAnimationComplete();
        }
    }

    private void OnAnimationComplete()
    {
        // Display game over info
        DisplayGameOverInfo();
        
        StartCoroutine(TransitionToNameInput());
    }

    private void DisplayGameOverInfo()
    {
        if (GameModeManager.Instance == null)
        {
            Debug.LogError("[ClosingScene] GameModeManager.Instance is null!");
            if (finalScoreText != null)
                finalScoreText.text = "Final Score: Unavailable";
            return;
        }

        int finalScore = GameModeManager.Instance.score;
        string finalDifficulty = GameModeManager.Instance.currentMode.ToString();

        Debug.Log($"[ClosingScene] Displaying - Score: {finalScore}, Difficulty: {finalDifficulty}");

        if (gameOverText != null)
            gameOverText.text = "GAME OVER";

        if (finalScoreText != null)
            finalScoreText.text = $"Final Score: {finalScore}";

        if (finalDifficultyText != null)
            finalDifficultyText.text = $"Difficulty Reached: {finalDifficulty}";
    }

    private IEnumerator TransitionToNameInput()
    {
        yield return new WaitForSeconds(gameOverDisplayDuration);

        Debug.Log("[ClosingScene] Transitioning to name input");

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (nameInputPanel != null)
            nameInputPanel.SetActive(true);

        if (playerNameInput != null)
            playerNameInput.ActivateInputField();
    }

    public void OnSubmitClicked()
    {
        if (hasSubmitted)
        {
            Debug.LogWarning("[ClosingScene] Submit already processed, ignoring");
            return;
        }

        if (playerNameInput == null)
        {
            Debug.LogError("[ClosingScene] Cannot submit - playerNameInput is null!");
            return;
        }

        string playerName = playerNameInput.text.Trim();
        
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("[ClosingScene] Empty name, using default");
            playerName = "Anonymous";
        }

        hasSubmitted = true;
        
        if (submitButton != null)
            submitButton.interactable = false;

        Debug.Log($"[ClosingScene] Submitting player name: {playerName}");

        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.FinalizeSession(playerName);
        }
        else
        {
            Debug.LogError("[ClosingScene] GameModeManager.Instance is null! Returning to landing page");
            SceneManager.LoadScene("LandingPage");
        }
    }

    private void OnDestroy()
    {
        if (gameEndAnimation != null)
            gameEndAnimation.OnAnimationFinished -= OnAnimationComplete;

        if (submitButton != null)
            submitButton.onClick.RemoveListener(OnSubmitClicked);
    }
}
