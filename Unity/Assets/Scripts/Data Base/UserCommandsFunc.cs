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
    private void Start()
    {
        _DBAdmin = FindObjectOfType<MySqlAdmin>();
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

                MySqlDataReader res2 = _DBAdmin.ExecuteQuery(
             _DBAdmin.CreateQuery(DBQueries.GET_CONNECTION_STATE,
             (string)dat.Rows[i]["user2"])
             );
                DataTable dat2 = new DataTable();
                dat2.Load(res2);
                friendsConnectionStates.Add((string)dat2.Rows[0]["connectedState"]);
                res2.Close();

            }
            else
            {
                friends.Add((string)dat.Rows[i]["user1"]);
                friendsStates.Add((string)dat.Rows[i]["state"]);

                MySqlDataReader res2 = _DBAdmin.ExecuteQuery(
                _DBAdmin.CreateQuery(DBQueries.GET_CONNECTION_STATE,
                (string)dat.Rows[i]["user1"])
);
                DataTable dat2 = new DataTable();
                dat2.Load(res2);
                friendsConnectionStates.Add((string)dat2.Rows[0]["connectedState"]);
                res2.Close();
            }
        }
        res.Close();

        return Tuple.Create<string[], string[], string[]>(friends.ToArray(), friendsStates.ToArray(),friendsConnectionStates.ToArray());
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

        List<Tuple<string, int>> allScores = new List<Tuple<string, int>>();

        for (int i = 0; i < dat.Rows.Count; i++)
        {
            allScores.Add(new Tuple<string, int>((string)dat.Rows[i]["user"],(int)dat.Rows[i]["score"]));
        }

        res.Close();

        allScores = allScores.OrderByDescending(x => x.Item2).ToList();

        foreach (var score in allScores)
        {
            scores.Add(score.Item1 + "   " + score.Item2);
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

        List<Tuple<string, int>> allScores = new List<Tuple<string, int>>();

        for (int i = 0; i < dat.Rows.Count; i++)
        {
            allScores.Add(new Tuple<string, int>((string)dat.Rows[i]["user"], (int)dat.Rows[i]["score"]));
        }

        res.Close();

        allScores = allScores.OrderByDescending(x => x.Item2).ToList();

        foreach (var score in allScores)
        {
            scores.Add(score.Item1 + "   " + score.Item2);
        }

        return scores.ToArray();
    }
}
