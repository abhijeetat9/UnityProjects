using System;
using UnityEngine;
using Mirror;

public class ServerBootstrap : MonoBehaviour
{
    public void Start()
    {

#if UNITY_SERVER
        NetworkManager.singleton.StartServer(); 
#endif
    }
}