using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class LobbyApiController : MonoBehaviour
{
    [SerializeField] private string backendBaseUrl = "https://cardgameprototype.onrender.com";
    
    private const string TokenKey = "auth_token";
    [Serializable]
    private struct LobbyData
    {
        public string hostUserId;
        public string inviteCode;
        public string[] players;
        public int maxPlayers;
        public string status;
    }

    [Serializable] private struct UserInfo
    {
        public string username;
    }

    [Serializable]
    private struct PopulatedLobbyData
    {
        public UserInfo hostUserId;
        public string inviteCode;
        public UserInfo[] players;
        public int maxPlayers;
        public string status;
    }

    [Serializable]
    private struct GetLobbyInfoResponse
    {
        public PopulatedLobbyData lobby;
        public string message;
    }
    
    [Serializable]
    private struct CreateLobbyResponse
    {
        public LobbyData lobby;
        public string hostUserId;
        public string message;
    }
    
    [Serializable]
    private struct JoinLobbyResponse
    {
        public LobbyData lobby;
        public string message;
    }

    // ---------- public API ----------

    public IEnumerator CreateLobby(Action<bool, string> onComplete)
    {
        var req = new UnityWebRequest(backendBaseUrl + "/lobbies", "POST");
        string token = PlayerPrefs.GetString(TokenKey);

        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + token);
        yield return req.SendWebRequest();
        Debug.Log($"result={req.result} code={req.responseCode} body='{req.downloadHandler.text}'");
        if (UnityWebRequest.Result.Success == req.result)
        {
            var responseData = JsonUtility.FromJson<CreateLobbyResponse>(req.downloadHandler.text);
            string code = responseData.lobby.inviteCode;
            Debug.Log($"Successfully parsed invite code: {code}");
            onComplete?.Invoke(true, code);
        }
        else
        {
            var responseData = JsonUtility.FromJson<CreateLobbyResponse>(req.downloadHandler.text);
            string message = responseData.message;
            Debug.Log(message);
            onComplete?.Invoke(false, message);
        }
    }
    
    public IEnumerator JoinLobby(string code, Action<bool, string> onComplete)
    {
        var req = new UnityWebRequest(backendBaseUrl + "/lobbies/" + code + "/join/", "POST");
        string token = PlayerPrefs.GetString(TokenKey);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + token);
        yield return req.SendWebRequest();
        if (UnityWebRequest.Result.Success == req.result)
        {
            var responseData = JsonUtility.FromJson<JoinLobbyResponse>(req.downloadHandler.text);
            string status = responseData.lobby.status;
            Debug.Log($"Lobby status: {status}");
            onComplete?.Invoke(true, responseData.message);
        }
        else
        {
            var responseData = JsonUtility.FromJson<JoinLobbyResponse>(req.downloadHandler.text);
            string message = responseData.message;
            Debug.Log(message);
            onComplete?.Invoke(false, message);
        }
    }

    public IEnumerator GetLobbyInfo(string code, Action<bool, string> onComplete)
    {
        var req = new UnityWebRequest(backendBaseUrl + "/lobbies/" + code, "GET");
        string token = PlayerPrefs.GetString(TokenKey);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + token);
        yield return req.SendWebRequest();
        if (UnityWebRequest.Result.Success == req.result)
        {
            var responseData = JsonUtility.FromJson<GetLobbyInfoResponse>(req.downloadHandler.text);
            var players = responseData.lobby.players;
            onComplete?.Invoke(true, players.Length.ToString());
        }
        else
        {
            var responseData = JsonUtility.FromJson<GetLobbyInfoResponse>(req.downloadHandler.text);
            string message = responseData.message;
            Debug.Log(message);
            onComplete?.Invoke(false, message);
        }
    }

    public IEnumerator StartLobby(string code, Action<bool, string> onComplete)
    {
        var req = new UnityWebRequest(backendBaseUrl + "/lobbies/" + code + "/start/",  "POST");
        string token = PlayerPrefs.GetString(TokenKey);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + token);
        yield return req.SendWebRequest();
        if (UnityWebRequest.Result.Success == req.result)
        {
            var responseData = JsonUtility.FromJson<JoinLobbyResponse>(req.downloadHandler.text);
            var lobby = responseData.lobby;
            string message = responseData.message;
            Debug.Log(lobby);
            Debug.Log(message);
            onComplete?.Invoke(true, message);
        }
        else
        {
            var responseData = JsonUtility.FromJson<JoinLobbyResponse>(req.downloadHandler.text);
            string message = responseData.message;
            Debug.Log(message);
            onComplete?.Invoke(false, message);
        }
    }

    public IEnumerator ValidateMembership(string code, string userName, string token, Action<bool> onComplete)
    {
        var req = new UnityWebRequest(backendBaseUrl + "/lobbies/" + code, "GET");
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + token);
        yield return req.SendWebRequest();
        if (UnityWebRequest.Result.Success == req.result)
        {
            var responseData = JsonUtility.FromJson<GetLobbyInfoResponse>(req.downloadHandler.text);
            
            bool isMember = responseData.lobby.players.Any(p => p.username == userName);
            onComplete?.Invoke(isMember);
        }
        else
        {
            var responseData = JsonUtility.FromJson<GetLobbyInfoResponse>(req.downloadHandler.text);
            string message = responseData.message;
            Debug.Log(message);
            onComplete?.Invoke(false);
        }
    }
}
