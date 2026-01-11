using UnityEngine;

public class HideUIOnClick : MonoBehaviour
{
    [Header("UI Elements to Hide")]
    public GameObject[] uiElementsToHide;

    void Start()
    {
        Debug.Log($"[{name}] HideUIOnClick initialized");
    }

    // Call this method via Button OnClick in the Inspector
    public void OnButtonPressed()
    {
        Debug.Log($"[{name}] Button pressed, hiding UI elements...");

        if (uiElementsToHide != null && uiElementsToHide.Length > 0)
        {
            foreach (GameObject element in uiElementsToHide)
            {
                if (element != null)
                {
                    element.SetActive(false);
                    Debug.Log($"[{name}] Hiding: {element.name}");
                }
            }
        }
        else
        {
            Debug.LogWarning($"[{name}] No UI elements assigned to hide!");
        }
    }
}
