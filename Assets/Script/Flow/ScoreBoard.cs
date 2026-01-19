using UnityEngine;
using TMPro;

public class ScoreBoard : MonoBehaviour
{
	[Header("UI")]
	[Tooltip("TMP text that shows the total accumulated score")]
	public TextMeshProUGUI totalScoreText;

	[Tooltip("TMP text that shows base score and bonus breakdown")]
	public TextMeshProUGUI breakdownText;

	[Header("Breakdown Background")]
	[Tooltip("Optional SpriteRenderer behind the breakdown text")]
	public SpriteRenderer breakdownBackground;

	[Tooltip("Background color for breakdown sprite")]
	public Color breakdownBgColor = new Color(0f, 0f, 0f, 0.6f);

	[Tooltip("Extra padding (x = width, y = height) added around the bonus text")]
	public Vector2 breakdownPadding = new Vector2(20f, 10f);

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

		// Display breakdown: show ONLY bonus points when available
		if (breakdownText != null)
		{
			int bonus = GameModeManager.Instance.lastMinigameBonus;

			if (bonus > 0)
			{
				breakdownText.text = $"TIME BONUS = {bonus}";
			}
			else
			{
				breakdownText.text = string.Empty;
			}
		}

		// Apply background styling if provided
		if (breakdownBackground != null)
		{
			bool showBg = breakdownText != null && !string.IsNullOrEmpty(breakdownText.text);
			breakdownBackground.enabled = showBg;
			breakdownBackground.color = breakdownBgColor;

			// Stretch background to fit text with padding when using Sliced/Tiled sprites
			if (showBg && (breakdownBackground.drawMode == SpriteDrawMode.Sliced || breakdownBackground.drawMode == SpriteDrawMode.Tiled) && breakdownText != null)
			{
				Vector2 preferred = breakdownText.GetPreferredValues(breakdownText.text);
				float targetWidth = preferred.x + breakdownPadding.x;
				float targetHeight = preferred.y + breakdownPadding.y;
				Vector2 size = breakdownBackground.size;
				size.x = Mathf.Max(size.x, targetWidth);
				size.y = Mathf.Max(size.y, targetHeight);
				breakdownBackground.size = size;
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
