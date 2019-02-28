using UnityEngine;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

public class MySqlAdmin : MonoBehaviour {

    public string DBServer;
    public string DBName;
    public string DBUser;
    public string DBPass;

    string connData;
    MySqlConnection conn;

    void Awake()
    {
        connData = "Server=" + DBServer + ";Database=" + DBName + ";Uid=" + DBUser + ";Pwd=" + DBPass + ";";
        DBServerConnect();
    }

    void DBServerConnect(){
        conn = new MySqlConnection(connData);
        try
        {
            conn.Open();
            print("Conexion con DB correcta");
        }
        catch (MySqlException error)
        {
            print("No se pudo conectar con DB: " + error);
            throw;
        }
    }

    public MySqlDataReader ExecuteQuery(string query){
        MySqlCommand cmd = conn.CreateCommand();
        cmd.CommandText = query;
        return cmd.ExecuteReader();
    }


    public string CreateQuery(string query, string val1){
        return query.Replace("%1", val1);
    }
    public string CreateQuery(string query, string val1, string val2){
        return query.Replace("%1", val1).Replace("%2", val2);
    }
    public string CreateQuery(string query, string val1, string val2, string val3){
        return query.Replace("%1", val1).Replace("%2", val2).Replace("%3", val3);
    }
}
