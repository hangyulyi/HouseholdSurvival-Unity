using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class API : MonoBehaviour
{
    string baseURL = "http://localhost:4000"; // TODO: Change after deployment

    public void Login(string email, string password)
    {
        StartCoroutine(LoginRequest(email, password));
    }

    IEnumerator LoginRequest(string  email, string password)
    {
        string json = JsonUtility.ToJson(new LoginData
        {
            email = email,
            password = password
        });

        UnityWebRequest request = new UnityWebRequest(
            baseURL + "/login",
            "POST"
        );

        byte[] body = Encoding.UTF8.GetBytes( json );

        request.uploadHandler = new UploadHandlerRaw( body );
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader( "Content-Type", "application/json" );

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log(request.downloadHandler.text);
        }
        else
        {
            Debug.Log(request.error);
        }
    }

    [System.Serializable]
    public class LoginData
    {
        public string email;
        public string password;
    }
    
}
