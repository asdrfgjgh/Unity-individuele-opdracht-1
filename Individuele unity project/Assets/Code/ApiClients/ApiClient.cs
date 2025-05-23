using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class ApiClient : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_Text errorText;
    public TMP_Text warningText;
    public TMP_Text foutEmail;

    public static ApiClient instance { get; private set; }
    public PostLoginResponseDto responseDto { get; private set; }
    void Awake()
    {
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
            foutEmail.text = "";
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
                foutEmail.text = "Email bestaat al.";
                return null;
            }
        }
    }

    public async void Register()
    {
        bool wachtwoordValidatie = await WachtwoordValidatieAsync(passwordInput.text);
        if (wachtwoordValidatie)
        {
            var registerDto = new PostRegisterRequestDto()
            {
                email = emailInput.text.ToString(),
                password = passwordInput.text.ToString()
            };

            string jsonData = JsonUtility.ToJson(registerDto);

            var response = await PerformApiCall("https://avansict2228255.azurewebsites.net/account/register", "POST", jsonData);
            Debug.Log(response);
            Debug.Log(emailInput.text);
            Debug.Log(passwordInput.text);
        }
    }

    public async void Login()
    {
        var loginDto = new PostLoginRequestDto()
        {
            email = emailInput.text.ToString(),
            password = passwordInput.text.ToString()
        };

        string jsonData = JsonUtility.ToJson(loginDto);

        var response = await PerformApiCall("https://avansict2228255.azurewebsites.net/account/login", "POST", jsonData);
        warningText.text = "Email of Wachtwoord is fout.";

        bool responseSuccess = response != null && response.Contains("token");
        if (responseSuccess)
        {
            warningText.text = "Email en Wachtwoord is juist.";
        
    
    responseDto = JsonUtility.FromJson<PostLoginResponseDto>(response);
            if (responseDto != null)
            {
                Debug.Log(responseDto.accessToken);
                SessionData.token = responseDto.accessToken; // Opslaan in SessionData
                string userId = await GetUserId(responseDto.accessToken);

                if (!string.IsNullOrEmpty(userId))
                {
                    SessionData.ownerUserId = userId; // Opslaan in SessionData
                }
                else
{
    Debug.LogError("Gefaald om User ID te krijgen");
}
Debug.Log(SessionData.ownerUserId);
            }
            else
{
    Debug.LogError("Gefaald om de respons te parsen");
}
        }
        else
{
    Debug.LogError("Login mislukt: " + response);
}
Debug.Log(response);
Debug.Log(emailInput.text);
Debug.Log(passwordInput.text);
    }

    public async Task<string> GetUserId(string token)
{
    var response = await PerformApiCall("https://avansict2228255.azurewebsites.net/WebApi/GetUserId", "GET", null, token);

    if (!string.IsNullOrEmpty(response))
    {
        // Assuming the response is a plain string containing the userId
        return response;
    }
    else
    {
        Debug.LogError("Empty response from GetUserId API");
    }

    return null;
}


    public async Task<bool> WachtwoordValidatieAsync(string wachtwoord)
    {
        errorText.text = "";
        string password = wachtwoord;
        bool isValid = true;
        StringBuilder errorMessages = new StringBuilder();

        if (password.Length < 10)
        {
            errorMessages.AppendLine("Wachtwoord moet minimaal 10 karakters lang zijn.");
            isValid = false;
        }

        if (!Regex.IsMatch(password, "[A-Z]"))
        {
            errorMessages.AppendLine("Wachtwoord moet minstens 1 hoofdletter bevatten.");
            isValid = false;
        }

        if (!Regex.IsMatch(password, "[a-z]"))
        {
            errorMessages.AppendLine("Wachtwoord moet minstens 1 kleine letter bevatten.");
            isValid = false;
        }

        if (!Regex.IsMatch(password, "[0-9]"))
        {
            errorMessages.AppendLine("Wachtwoord moet minstens 1 cijfer bevatten.");
            isValid = false;
        }

        if (!Regex.IsMatch(password, "[^a-zA-Z0-9]"))
        {
            errorMessages.AppendLine("Wachtwoord moet minstens 1 niet-alfanumeriek teken bevatten.");
            isValid = false;
        }

        errorText.text = errorMessages.ToString();
        return isValid;
    }
}