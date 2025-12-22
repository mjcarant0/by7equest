using UnityEngine;
using TMPro;

public class TempGameOverUI : MonoBehaviour
{
    public TextMeshProUGUI gameOverText;

    void Start()
    {
        if (GameModeManager.Instance == null)
        {
            gameOverText.text = "GAME OVER\nScore Unavailable";
            return;
        }

        int finalScore = GameModeManager.Instance.score;
        string finalMode = GameModeManager.Instance.currentMode.ToString();

        gameOverText.text =
            "GAME OVER\n\n" +
            "Final Score: " + finalScore + "\n" +
            "Difficulty Reached: " + finalMode;
    }
}
