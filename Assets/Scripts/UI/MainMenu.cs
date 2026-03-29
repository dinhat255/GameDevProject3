using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private const string GameplaySceneName = "GamePlayScene";

    public void PlayGame()
    {
        SceneManager.LoadScene(GameplaySceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
