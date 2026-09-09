using System.Collections;
using Mirror;
using UnityEngine;

public class LobbyAuthenticator : NetworkAuthenticator
{
    [SerializeField] private string inviteCode;
    public void SetInviteCode(string code) => inviteCode = code;
    [SerializeField] private string username;
    [SerializeField] private string authToken;
    public void SetAuthToken(string token) => authToken = token;
    private LobbyApiController _api;

    // ---------- the message sent client -> server ----------

    public struct AuthRequestMessage : NetworkMessage
    {
        public string inviteCode;
        public string username;
        public string authToken;
    }
    
    public struct AuthResponseMessage : NetworkMessage
    {
        public bool success;
        public string message;
    }

    // ---------- client side ----------

    public override void OnStartClient()
    {
        username = PlayerPrefs.GetString("auth_username", username);
        authToken = PlayerPrefs.GetString("auth_token", authToken);
        NetworkClient.RegisterHandler<AuthResponseMessage>(OnAuthResponseMessage, false);
    }

    private void OnAuthResponseMessage( AuthResponseMessage msg)
    {
        if (msg.success)
        {
            ClientAccept();
        }
        else
        {
            ClientReject();
        }

    }

    public override void OnClientAuthenticate()
    {
        Debug.Log($"fired along with the invite {inviteCode}/{username} you're about to send");
        AuthRequestMessage authRequestMessage = new AuthRequestMessage()
        {
            inviteCode = inviteCode,
            username = username,
            authToken = authToken
        };
        NetworkClient.connection.Send(authRequestMessage);
        
    }

    // ---------- server side ----------

    public override void OnStartServer()
    {
        _api = FindAnyObjectByType<LobbyApiController>();
        if (_api)
        {
            NetworkServer.RegisterHandler<AuthRequestMessage>(OnAuthRequestMessage, false);
        }
        else
        {
            Debug.LogError("No LobbyApiController found");
            enabled = false;
            return;
        }
    }

    private void OnAuthRequestMessage(NetworkConnectionToClient conn, AuthRequestMessage msg)
    {
        Debug.Log($"logging {msg.inviteCode}: {msg.username}");
        StartCoroutine(_api.ValidateMembership(msg.inviteCode, msg.username, msg.authToken, isMember =>
        {
            
            if (isMember)
            {
                CabbooNetworkManager _cabbooNetworkManager = NetworkManager.singleton as CabbooNetworkManager;
                _cabbooNetworkManager.RegisterConnectionToRoom(conn, msg.inviteCode);
                conn.Send(new AuthResponseMessage
                {
                    success = true,
                    
                });
                ServerAccept(conn);
            }
            else
            {
                
                conn.Send(new AuthResponseMessage
                {
                    success = false,
                });
                StartCoroutine(DelayedReject(conn));
            }
        }));
    }

    private IEnumerator DelayedReject(NetworkConnectionToClient conn)
    {
        yield return new WaitForSeconds(1f);
        ServerReject(conn);
    }
}
