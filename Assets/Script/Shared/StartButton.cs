using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
    public string openingSceneName = "OpeningScene";

    public void OnStartPressed()
    {
        SceneManager.LoadScene(openingSceneName);
    }
}
