using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonFunctions : MonoBehaviour
{
    public void DisconnectPlayer()
    {
        ClientManager.myClient.connection.Disconnect();
    }
    public void RestartGame()
    {
        ClientManager.instance.RestartButton();
    }
    public void LogIn()
    {
        ClientManager.instance.SetupClient(false);
    }
    public void Register()
    {
        ClientManager.instance.SetupClient(true);
    }

    public void QuitGame()
    {
        ClientManager.instance.QuitPlayer();
    }
}
