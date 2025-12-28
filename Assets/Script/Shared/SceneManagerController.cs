using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerController : MonoBehaviour
{
    // Function to load the ScorePage
    public void LoadScorePage()
    {
        SceneManager.LoadScene("ScoreScene");
    }

    // Function to load the CreditsPage
    public void LoadCreditsPage()
    {
        SceneManager.LoadScene("CreditsPage");
    }

    // Function to load the LandingPage
    public void LoadLandingPage()
    {
        SceneManager.LoadScene("LandingPage");
    }

    // Function to load the game
    public void StartGame()
    {
        SceneManager.LoadScene("GameModeBootstrap");
    }
}
