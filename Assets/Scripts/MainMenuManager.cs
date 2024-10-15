using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : Singleton<MainMenuManager>
{
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button endGameButton;

    private readonly string _gameSceneString = "GameScene";
    private void Start()
    {
        startGameButton.onClick.AddListener(StartGame);
        endGameButton.onClick.AddListener(EndGame);
    }

    private void EndGame()
    {
        Application.Quit();
    }

    private void StartGame()
    {
        
        SceneManager.LoadScene(_gameSceneString);
    }
}
