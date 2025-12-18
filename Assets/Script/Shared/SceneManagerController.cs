/// This script manages scene transitions in the Unity application.

using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerController : MonoBehaviour
{
    // Function to load the ScorePage
    public void LoadScorePage()
    {
        SceneManager.LoadScene("ScorePage");
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
}
