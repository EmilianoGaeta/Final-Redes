using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DBQueries
{
    public const string LOG_IN_REQUEST = "call final.LoginUser('%1', '%2');";
    public const string REGISTER_USER = "call final.RegisterUser('%1', '%2');";
    public const string GET_USER_FRIENDS = "call final.GetFriends('%1');";
    public const string GET_CONNECTION_STATE = "call final.GetUserConnectionState('%1');";
    public const string GET_USER_HIGHSCORE = "call final.GetUserHighScore('%1');";
    public const string GET_HIGHSCORE = "call final.GetHighScores('%1');";
}
