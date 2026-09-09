using Mirror;
using UnityEngine;

/// <summary>
/// Disables Mirror's built-in NetworkManagerHUD (Stop Host / Stop Client
/// debug overlay) in production builds. Keeps it active in the editor and
/// in development builds so it stays useful for local testing.
/// </summary>
[RequireComponent(typeof(NetworkManagerHUD))]
public class DevOnlyHud : MonoBehaviour
{
    private void Awake()
    {
        bool allowHud = Application.isEditor || Debug.isDebugBuild;
        if (!allowHud)
        {
            GetComponent<NetworkManagerHUD>().enabled = false;
        }
    }
}
