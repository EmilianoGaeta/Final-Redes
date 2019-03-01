using MySql.Data.MySqlClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Data;
using System;
using System.Linq;

public class UserCommandsFunc : MonoBehaviour
{
    MySqlAdmin _DBAdmin;
    private void Awake()
    {
        _DBAdmin = FindObjectOfType<MySqlAdmin>();
    }

    public void AddFriend(string user1, string user2)
    {
        MySqlDataReader res = _DBAdmin.ExecuteQuery(
          _DBAdmin.CreateQuery(DBQueries.ADD_FRIEND,
          user1, user2));
        res.Close();
    }

    public void AcceptRejectFriendship(string user1, string user2, string AorR)
    {
        MySqlDataReader res = _DBAdmin.ExecuteQuery(
          _DBAdmin.CreateQuery(DBQueries.ACCEPT_REJECT_FRIENDSHIP,
          user1, user2,AorR));
        res.Close();
    }

    public void DeleteFriend(string user1, string user2)
    {
        MySqlDataReader res = _DBAdmin.ExecuteQuery(
          _DBAdmin.CreateQuery(DBQueries.DELETE_FRIEND,
          user1, user2));
        res.Close();
    }


    public void SetConnectionState(string user, string state, int id)
    {
        MySqlDataReader res = _DBAdmin.ExecuteQuery(
           _DBAdmin.CreateQuery(DBQueries.SET_CONNECTION_STATE,
           user,state,id.ToString()));

        res.Close();
    }

    public void SetAllToDisconected()
    {
        MySqlDataReader res = _DBAdmin.ExecuteQuery(
          _DBAdmin.CreateQuery(DBQueries.SET_ALL_TO_DISCONECTED,""));
        res.Close();
    }


    public Tuple<string[], string[], string[]> GetUserFriends(string user)
    {
        List<string> friends = new List<string>();
        List<string> friendsStates = new List<string>();
        List<string> friendsConnectionStates = new List<string>();

        MySqlDataReader res = _DBAdmin.ExecuteQuery(
              _DBAdmin.CreateQuery(DBQueries.GET_USER_FRIENDS,
              user)
              );
        DataTable dat = new DataTable();
        dat.Load(res);

        for (int i = 0; i < dat.Rows.Count; i++)
        {
            if ((string)dat.Rows[i]["user1"] == user)
            {
                friends.Add((string)dat.Rows[i]["user2"]);
                friendsStates.Add((string)dat.Rows[i]["state"]);
            }
            else
            {
                friends.Add((string)dat.Rows[i]["user1"]);
                friendsStates.Add((string)dat.Rows[i]["state"]);
            }
        }
        res.Close();

        for (int i = 0; i < friends.Count; i++)
        {
            MySqlDataReader res2 = _DBAdmin.ExecuteQuery(
           _DBAdmin.CreateQuery(DBQueries.GET_CONNECTION_STATE,
           friends[i]));

            DataTable dat2 = new DataTable();
            dat2.Load(res2);
            friendsConnectionStates.Add((string)dat2.Rows[0]["connectedState"]);
            res2.Close();
        }

        return Tuple.Create<string[], string[], string[]>(friends.ToArray(), friendsStates.ToArray(), friendsConnectionStates.ToArray());
    }

    public Tuple<string,string,int> GetUserConnectionStateAndID(string user)
    {
        MySqlDataReader res = _DBAdmin.ExecuteQuery(
           _DBAdmin.CreateQuery(DBQueries.GET_CONNECTION_STATE,
           user));
        if (res.HasRows)
        {
            DataTable dat = new DataTable();
            dat.Load(res);
            var userName = (string)dat.Rows[0]["user"];
            var connectionState = (string)dat.Rows[0]["connectedState"];
            var id = (int)dat.Rows[0]["connectionID"];
            res.Close();
            return new Tuple<string,string, int>(userName,connectionState, id);
        }
        res.Close();
        return new Tuple<string, string, int>("", "", -1);
    }

    public string[] GetUserHighScores(string user)
    {
        List<string> scores = new List<string>();

        MySqlDataReader res = _DBAdmin.ExecuteQuery(
              _DBAdmin.CreateQuery(DBQueries.GET_USER_HIGHSCORE,
              user)
              );
        DataTable dat = new DataTable();
        dat.Load(res);

        List<Tuple<string, int,int>> allScores = new List<Tuple<string, int,int>>();

        for (int i = 0; i < dat.Rows.Count; i++)
        {
            allScores.Add(new Tuple<string, int, int>((string)dat.Rows[i]["user"], (int)dat.Rows[i]["wins"], (int)dat.Rows[i]["lost"]));
        }

        res.Close();

        allScores = allScores.OrderByDescending(x => (x.Item2 - x.Item3)).ToList();

        foreach (var score in allScores)
        {
            scores.Add(score.Item1.PadRight(35) + ("Wins: " + score.Item2).PadRight(10) + ("Lost: " + score.Item3).PadRight(10) + "Total: " + (score.Item2 - score.Item3));
        }

        return scores.ToArray();
    }

    public string[] GetHighScores()
    {
        List<string> scores = new List<string>();

        MySqlDataReader res = _DBAdmin.ExecuteQuery(
              _DBAdmin.CreateQuery(DBQueries.GET_HIGHSCORE,"")
              );
        DataTable dat = new DataTable();
        dat.Load(res);

        List<Tuple<string, int, int>> allScores = new List<Tuple<string, int, int>>();

        for (int i = 0; i < dat.Rows.Count; i++)
        {
            allScores.Add(new Tuple<string, int, int>((string)dat.Rows[i]["user"], (int)dat.Rows[i]["wins"], (int)dat.Rows[i]["lost"]));
        }

        res.Close();

        allScores = allScores.OrderByDescending(x => (x.Item2 - x.Item3)).ToList();

        foreach (var score in allScores)
        {
            scores.Add(score.Item1.PadRight(35) + ("Wins: " + score.Item2).PadRight(10) + ("Lost: " + score.Item3).PadRight(10) + "Total: " + (score.Item2 - score .Item3));
        }

        return scores.ToArray();
    }

    public void AddLostToUser(string user)
    {
        MySqlDataReader res = _DBAdmin.ExecuteQuery(
            _DBAdmin.CreateQuery(DBQueries.ADD_LOST_TO_USER, user)
            );
        res.Close();
    }
    public void AddWinToUser(string user)
    {
        MySqlDataReader res = _DBAdmin.ExecuteQuery(
            _DBAdmin.CreateQuery(DBQueries.ADD_WIN_TO_USER, user)
            );
        res.Close();
    }
}
