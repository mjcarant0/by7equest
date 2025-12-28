using UnityEngine;
using TMPro;

public class ScoreScene : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI totalScoreText;
    public TextMeshProUGUI modeText;

    void Start()
    {
        if (GameModeManager.Instance == null)
        {
            Debug.LogError("GameModeManager.Instance is null in ScoreScene!");
            
            if (scoreText != null)
            {
                scoreText.text = "Score Unavailable";
            }
            return;
        }

        if (scoreText == null)
        {
            Debug.LogError("Score Text not assigned in ScoreScene Inspector!");
        }

        if (totalScoreText == null)
        {
            Debug.LogError("Total Score Text not assigned in ScoreScene Inspector!");
        }

        if (modeText == null)
        {
            Debug.LogError("Mode Text not assigned in ScoreScene Inspector!");
        }

        int lastScore = GameModeManager.Instance.lastMinigameScore;
        int totalScore = GameModeManager.Instance.score;
        string currentMode = GameModeManager.Instance.currentMode.ToString();

        if (scoreText != null)
        {
            if (lastScore > 0)
            {
                scoreText.text = $"+{lastScore} Points!";
                scoreText.color = Color.green;
            }
            else
            {
                scoreText.text = "Failed!";
                scoreText.color = Color.red;
            }
        }

        if (totalScoreText != null)
        {
            totalScoreText.text = $"Total Score: {totalScore}";
        }

        if (modeText != null)
        {
            modeText.text = $"Mode: {currentMode}";
        }
    }
}
