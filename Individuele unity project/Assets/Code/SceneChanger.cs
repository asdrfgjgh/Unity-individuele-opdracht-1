using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneChanger : MonoBehaviour
{
    public string sceneName;


    public void ChangeScene()
    {
        SceneManager.LoadScene(sceneName);
    }
    public void ExitApplication()
    {
        Application.Quit();
    }
}