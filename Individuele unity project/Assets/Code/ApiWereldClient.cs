using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class ApiWereldClient : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public TMP_InputField naamInput;
    public TMP_Text naamText;
    public GameObject wereldPrefab; // Reference to the prefab
    public RectTransform wereldContainer; // Reference to the container\
    public Button createWereld; // Reference to the create button
    public static ApiWereldClient instance { get; private set; }
    void Awake()
    {
        LoadWorld();
        // hier controleren we of er al een instantie is van deze singleton
        // als dit zo is dan hoeven we geen nieuwe aan te maken en verwijderen we deze
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(this);
    }
    private async Task<string> PerformApiCall(string url, string method, string jsonData = null, string token = null)
    {
        using (UnityWebRequest request = new UnityWebRequest(url, method))
        {
            if (!string.IsNullOrEmpty(jsonData))
            {
                byte[] jsonToSend = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            }

            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            if (!string.IsNullOrEmpty(token))
            {
                request.SetRequestHeader("Authorization", "Bearer " + token);
            }

            await request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                return request.downloadHandler.text;
            }
            else
            {
                Debug.LogError("Fout bij API-aanroep: " + request.error);
                return null;
            }
        }
    }
    public async void RegisterWorld(WereldPrefabController controller)
    {
        Debug.Log($"naamInput: {naamInput}");


        if (SessionData.token != null)
        {
            var registerDto = new PostWereldRegisterRequestDto()
            {
                name = controller.naamInput.text,
                ownerUserId = SessionData.ownerUserId,
                maxLength = 200,
                maxHeight = 200
            };
            string jsonData = JsonUtility.ToJson(registerDto);
            var response = await PerformApiCall("https://avansict2228255.azurewebsites.net/WebApi", "POST", jsonData, SessionData.token);
            Debug.Log(response);
        }
        else
        {
            Debug.LogError("SessionData token is null");
        }
        Debug.Log(SessionData.token);
        Debug.Log(naamInput.text);
        LoadWorld();

    }

    public async void LoadWorld()
    {

            if (SessionData.token != null)
            {
                foreach (Transform child in wereldContainer)
                {
                    Destroy(child.gameObject);
                }
                string url = $"https://avansict2228255.azurewebsites.net/WebApi/getwereld/{SessionData.ownerUserId}";
                var response = await PerformApiCall(url, "GET", null, SessionData.token);

                if (response == null)
                {
                    Debug.LogError("API response is null");
                    return;
                }

                Debug.Log("API Response: " + response);

                try
                {
                    List<PostWereldLoadResponseDto> worlds = JsonConvert.DeserializeObject<List<PostWereldLoadResponseDto>>(response);
                    if (worlds != null)
                    {
                        int worldCount = worlds.Count;
                        for (int i = 0; i < worldCount && i < 5; i++)
                        {
                            var world = worlds[i];
                            CreateWorldPrefab(world);
                        }

                        for (int i = worldCount; i < 5; i++)
                        {
                            CreateEmptyWorldPrefab();
                        }
                    }
                    else
                    {
                        Debug.LogError("Failed to load worlds or no worlds found.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error deserializing JSON response: " + ex.Message);
                }
            }
            else
            {
                Debug.LogError("SessionData token is null");
            }
        }
    

    private void CreateWorldPrefab(PostWereldLoadResponseDto world)
    {
        GameObject worldObject = Instantiate(wereldPrefab, wereldContainer);
        WereldPrefabController controller = worldObject.GetComponent<WereldPrefabController>();
        controller.Initialize(this, world);
    }

    private void CreateEmptyWorldPrefab()
    {
        GameObject worldObject = Instantiate(wereldPrefab, wereldContainer);
        WereldPrefabController controller = worldObject.GetComponent<WereldPrefabController>();
        controller.Initialize(this);
    }

    public void LoadSpecificWorld(string worldId)
    {
        SessionData.worldId = worldId;
        Debug.Log($"Loading world with ID: {SessionData.worldId}");
        // Implement the logic to load the specific world
    }
}
