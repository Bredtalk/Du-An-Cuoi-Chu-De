using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject menu;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }
    public void ShowMenu()
    {
        
        if (!menu.activeSelf)
        {
            Time.timeScale = 0f; // Pause game 
            menu.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f; // Pause game 
            menu.SetActive(false);
        }
      
    }

    // Alias matching Unity button OnClick event
    public void ShowMainMenu()
    {
        ShowMenu();
    }

    public void ResumeGame()
    {
        menu.SetActive(false);
        Time.timeScale = 1f; // Active game
    }

    // Alias matching lowercase 'resumeGame' from Unity Inspector
    public void resumeGame()
    {
        ResumeGame();
    }

    public void OpenSettings()
    {
        SceneManager.LoadScene("Settings", LoadSceneMode.Additive); // tao scene setting sau 
        // LoadSceneMode.Additive : tham số để scene Settings mở nhưng scene game vẫn tồn tại phía dưới 
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}