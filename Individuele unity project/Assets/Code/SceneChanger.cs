using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public string sceneName;
    private ApiClient apiClient;

    private void Start()
    {
        apiClient = ApiClient.instance;
    }
    public void AnoniemSceneChange()
    {
        SceneManager.LoadScene(sceneName);
    }
    public void ChangeScene()
    {
        if (IsUserLoggedIn())
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("Je bent niet ingelogd");
        }
    }

    private bool IsUserLoggedIn()
    {
        return apiClient != null && apiClient.responseDto != null && !string.IsNullOrEmpty(apiClient.responseDto.accessToken);
    }
}