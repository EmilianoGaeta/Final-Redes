using MySql.Data.MySqlClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Data;

public class LoginAndRegister : MonoBehaviour
{
    MySqlAdmin _DBAdmin;

    private void Start()
    {
        _DBAdmin = FindObjectOfType<MySqlAdmin>();
    }

    public bool LogIn(string user, string pass)
    {
        MySqlDataReader res = _DBAdmin.ExecuteQuery(
                              _DBAdmin.CreateQuery(DBQueries.LOG_IN_REQUEST,
                              user, pass));
        if (res.HasRows)
        {
            res.Close();
            return false;
        }
        else
        {
            res.Close();
            MySqlDataReader res2 = _DBAdmin.ExecuteQuery(
            _DBAdmin.CreateQuery(DBQueries.GET_CONNECTION_STATE,
            user));

            DataTable dat2 = new DataTable();
            dat2.Load(res2);
            var state = ((string)dat2.Rows[0]["connectedState"]);
            res2.Close();

            return state != "C";
        }
 
    }

    public void Register(string user, string pass)
    {
        MySqlDataReader res = _DBAdmin.ExecuteQuery(
                              _DBAdmin.CreateQuery(DBQueries.REGISTER_USER,
                              user, pass));
        res.Close();
    }
}
