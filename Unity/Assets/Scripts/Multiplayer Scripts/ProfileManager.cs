using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class ProfileManager : MonoBehaviour {

    public Dictionary<string, GameObject> allTabs = new Dictionary<string, GameObject>();

    public InputField searchFriendsField;

	void Awake()
    {
        var TabsHolder = transform.Find("Tabs");
        foreach (Transform tab in TabsHolder)
        {
            allTabs.Add(tab.name, tab.gameObject);
        }
    }
	
	public void OpenTab(string tabname)
    {
        foreach (var item in allTabs)
            item.Value.SetActive(item.Key == tabname);
    }

    public void WriteFriendList(string[] friendList)
    {
        List<string> friendsNames = new List<string>();
        List<string> friendsStates = new List<string>();
        List<string> friendsConnectionStates = new List<string>();

        int i = 1;

        while (i < friendList.Length && friendList[i] != "/")
        {
            friendsNames.Add(friendList[i]);
            i++;
        }
        i++;
        while (i < friendList.Length && friendList[i] != "/")
        {
            friendsStates.Add(friendList[i]);
            i++;
        }
        i++;
        while (i < friendList.Length && friendList[i] != "/")
        {
            friendsConnectionStates.Add(friendList[i]);
            i++;
        }

        var friends = friendsNames.Zip(friendsConnectionStates, (x, y) => " " + y.ToUpper() + "   " + x).Zip(friendsStates, (x, y) => x.PadRight(50) + "Estado: " + y.ToUpper());

        string text = "";

        foreach (var friend in friends)
        {
            text += friend;
            text += "\r\n";
        }

        GameObject friendListGO;
        if (allTabs.TryGetValue("FriendList", out friendListGO))
        {
            var oldState = friendListGO.activeSelf;
            friendListGO.SetActive(true);
            friendListGO.transform.GetComponentsInChildren<Text>().Where(x => x.gameObject.name == "ScrollText").First().text = text;
            friendListGO.SetActive(oldState);
        }
    }

    public void WriteHighScore(string[] highScores)
    {

        string text = "";

        foreach (var score in highScores)
        {
            if (score == "/") continue;
            text += score;
            text += "\r\n";
        }

        GameObject highScoresGO;
        if (allTabs.TryGetValue("HighScores", out highScoresGO))
        {
            var oldState = highScoresGO.activeSelf;
            highScoresGO.SetActive(true);
            highScoresGO.transform.GetComponentsInChildren<Text>().Where(x=> x.gameObject.name == "ScrollText").First().text = text;
            highScoresGO.SetActive(oldState);
        }
    }

    public void AskForUserScore()
    {
        GameObject highScoresGO;
        if (allTabs.TryGetValue("HighScores", out highScoresGO))
        {
            ClientManager.instance.AskForUserScore(highScoresGO);
        }
    }

    public void StartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
    }
    public void AddFriend()
    {
        if (searchFriendsField!= null)
             ClientManager.instance.AddFriend(searchFriendsField.text);
    }
    public void DeleteFriend()
    {
        if (searchFriendsField != null)
            ClientManager.instance.DeleteFriend(searchFriendsField.text);
    }
    public void AcceptRejectFriend(string AorR)
    {
        if (searchFriendsField != null)
            ClientManager.instance.AcceptRejectFriend(searchFriendsField.text,AorR);
    }

}
