using UnityEngine;
using TMPro;

public class ScoreBoard : MonoBehaviour
{
	[Header("UI")]
	[Tooltip("TMP text that shows the total accumulated score")]
	public TextMeshProUGUI totalScoreText;

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
			return;
		}

		int total = GameModeManager.Instance.score;
		SafeSetText(totalScoreText, total.ToString());
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
