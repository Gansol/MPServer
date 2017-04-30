using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System.Data;
using System;
using System.IO;
using System.Data.Common;

public class MPDB
{
    static string dbName = "MPDB.db";

    static string dbConnString =
        #if UNITY_EDITOR
 Application.dataPath + "/"
#elif UNITY_ANDROID
 "URI=file:" + Application.persistentDataPath + "/"

#elif UNITY_IOS
         @"Data Source=" + Application.persistentDataPath + "/"
#endif
 + dbName;

    public static DataTable GetSQLAllData(string tableName)
    {
        DataTable dt = new DataTable(); // 儲存資料庫資料用 資料列
        //若是在Android平台
#if UNITY_ANDROID
        // 如果檔案不存在 下載
        if (!File.Exists(Application.persistentDataPath + "/" + dbName))
        {
            WWW loadDB = new WWW(Global.serverPath + "/" + dbName);
            while (!loadDB.isDone)
            { }
            File.WriteAllBytes(Application.persistentDataPath + "/" + dbName, loadDB.bytes);    // 如果寫入到這就要重這讀
        }
#endif

        using (SqliteConnection sqlConn = new SqliteConnection(dbConnString))     // 建立連線 結束後自動關閉
        {
            sqlConn.Open();                                                     // 開啟連線

            SqliteCommand sqlCmd = new SqliteCommand("SELECT * FROM " + tableName, sqlConn);
            using (SqliteDataReader reader = sqlCmd.ExecuteReader())
            {
                FillDatatable(reader, dt);
            }
        }
        return dt;
    }

    private static void FillDatatable(SqliteDataReader reader, DataTable dt)
    {
        int len = reader.FieldCount;

        // Create the DataTable columns
        for (int i = 0; i < len; i++)
            dt.Columns.Add(reader.GetName(i), reader.GetFieldType(i));

        dt.BeginLoadData();

        var values = new object[len];

        // reader.Read(); // 只讀一筆資料存在 reader (每讀一次會累加row)
        // Add data rows
        while (reader.Read())
        {
            for (int i = 0; i < len; i++)
                values[i] = reader[i];

            dt.Rows.Add(values);
        }
        dt.EndLoadData();
    }
}

// SQLite 無法使用DataSet
//public static DataSet GetSQLAllData2(string tableName)
//{
//    DataSet ds = new DataSet();


//    using (SqliteConnection sqlConn = new SqliteConnection(filePath))
//    {
//        SqliteCommand sqlCommand = new SqliteCommand();
//        sqlCommand.Connection = sqlConn;
//        sqlConn.Open();

//        SqliteDataAdapter adapter = new SqliteDataAdapter("SELECT * FROM " + tableName, sqlConn);
//        adapter.FillSchema(ds,SchemaType.Source, tableName);
//        adapter.Fill(ds,tableName);
//    }
//    return ds;
//}
