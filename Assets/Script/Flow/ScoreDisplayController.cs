using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ScoreDisplayController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI lastGameScoreText;
    public TextMeshProUGUI totalScoreText;
    public TextMeshProUGUI difficultyText;

    [Header("Optional Animation")]
    public Animator scoreAnimator;

    void Start()
    {
        if (GameModeManager.Instance == null)
        {
            Debug.LogError("[v0] GameModeManager.Instance is null in ScoreDisplayController!");
            if (scoreText != null)
            {
                scoreText.text = "Score Unavailable";
            }
            return;
        }

        DisplayScore();
    }

    // DITO IEDIT YUNG LOGIC NG SCORE, DRAFT LANG ITO DAHIL SA NAGAWA KO LAST TIME
    private void DisplayScore()
    {
        int lastScore = GameModeManager.Instance.lastMinigameScore;
        int totalScore = GameModeManager.Instance.score;
        string difficulty = GameModeManager.Instance.currentMode.ToString();

        Debug.Log($"[v0] Displaying Score - Last Game: {lastScore}, Total: {totalScore}, Difficulty: {difficulty}");

        if (lastGameScoreText != null)
        {
            lastGameScoreText.text = $"+{lastScore}";
        }

        if (totalScoreText != null)
        {
            totalScoreText.text = $"{totalScore}";
        }

        if (difficultyText != null)
        {
            difficultyText.text = $"{difficulty} Mode";
        }

        if (scoreText != null)
        {
            scoreText.text = $"Score: +{lastScore}\n\nTotal Score: {totalScore}\n\nDifficulty: {difficulty}";
        }

        if (scoreAnimator != null)
        {
            scoreAnimator.SetTrigger("Show");
        }
    }
}