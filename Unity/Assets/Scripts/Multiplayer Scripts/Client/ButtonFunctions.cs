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
    public void SetuClient()
    {
        ClientManager.instance.SetupClient();
    }
    public void QuitGame()
    {

    }
}
