using UnityEngine;
using TMPro;

public class ScoreBoard : MonoBehaviour
{
	[Header("UI")]
	[Tooltip("TMP text that shows the total accumulated score")]
	public TextMeshProUGUI totalScoreText;

	[Tooltip("TMP text that shows base score and bonus breakdown")]
	public TextMeshProUGUI breakdownText;

	[Tooltip("Minimum auto-size for TMP text (helps fit large numbers in the frame)")]
	public float minFontSize = 18f;

	[Tooltip("Maximum auto-size for TMP text")]
	public float maxFontSize = 72f;

	private void OnEnable()
	{
		Refresh();
	}

	public void Refresh()
	{
		if (GameModeManager.Instance == null)
		{
			Debug.LogError("[ScoreBoard] GameModeManager.Instance is null. Cannot display scores.");
			SafeSetText(totalScoreText, "Score unavailable");
			if (breakdownText != null) breakdownText.text = "";
			return;
		}

		int total = GameModeManager.Instance.score;
		SafeSetText(totalScoreText, total.ToString());

		// Display breakdown of last minigame score
		if (breakdownText != null)
		{
			int lastScore = GameModeManager.Instance.lastMinigameScore;
			int baseScore = GameModeManager.Instance.lastMinigameBaseScore;
			int bonus = GameModeManager.Instance.lastMinigameBonus;

			// Only show breakdown if there was a recent score
			if (lastScore > 0)
			{
				string modeName = GameModeManager.Instance.GetCurrentMode().ToString().ToLower();
				breakdownText.text = $"{modeName} = {baseScore}\nbonus = {bonus}";
			}
			else
			{
				breakdownText.text = "";
			}
		}
	}

	private void SafeSetText(TextMeshProUGUI tmp, string value)
	{
		if (tmp == null) return;

		tmp.enableAutoSizing = true;
		tmp.fontSizeMin = minFontSize;
		tmp.fontSizeMax = maxFontSize;
		tmp.text = value;
	}
}
