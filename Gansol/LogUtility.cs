using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Security.Cryptography;
using System.IO;


/* ***************************************************************
 * -----Copyright © 2015 Gansol Studio.  All Rights Reserved.-----
 * -----------            CC BY-NC-SA 4.0            -------------
 * -----------  @Website:  EasyUnity@blogspot.com    -------------
 * -----------  @Email:    GansolTW@gmail.com        -------------
 * -----------  @Author:   Krola.                    -------------
 * ***************************************************************
 *                          Description
 * ***************************************************************
 * 
 * 這個檔案是用來產生 LOG資料所使用
 * 
 * ***************************************************************/

namespace Gansol
{
    public partial class LogUtility : Component
    {
        protected String SavePathString;
        protected String FileNameString;
        protected String ObjNameString;

        public LogUtility()
        {
            InitializeComponent();

            SavePathString = "C:\\GansolLOG\\";
            FileNameString = "Log";
            ObjNameString = "NotSet";
            Directory.CreateDirectory(SavePathString);
        }

        public LogUtility(IContainer container)
        {
            container.Add(this);

            InitializeComponent();

            SavePathString = "C:\\GansolLOG\\";
            FileNameString = "Log";
            ObjNameString = "NotSet";
            Directory.CreateDirectory(SavePathString);
        }

        public LogUtility(String SavePath)
        {
            try
            {
                SavePathString = SavePath.Trim();
                if (SavePathString[SavePathString.Length - 1] != '\\') SavePathString += "\\";

                FileNameString = "Log";
                ObjNameString = "NotSet";

                Directory.CreateDirectory(SavePathString);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public LogUtility(String SavePath, String FileName)
        {
            try
            {
                SavePathString = SavePath;
                if (SavePathString[SavePathString.Length - 1] != '\\') SavePathString += "\\";

                FileNameString = FileName;
                ObjNameString = "NotSet";

                Directory.CreateDirectory(SavePathString);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public LogUtility(String SavePath, String FileName, String ObjName)
        {
            try
            {
                SavePathString = SavePath;
                if (SavePathString[SavePathString.Length - 1] != '\\') SavePathString += "\\";

                FileNameString = FileName;
                ObjNameString = ObjName;

                Directory.CreateDirectory(SavePathString);

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void WriteLog(string LogMessage)
        {
            try
            {
                string LogFileName = SavePathString + FileNameString + System.DateTime.Now.ToString("yyyy-MM-dd-HH") + ".txt";


                FileStream Logfile = new FileStream(LogFileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                //StreamWriter writer = new System.IO.StreamWriter(Logfile, System.Text.Encoding.GetEncoding("Big5"));
                StreamWriter writer = new System.IO.StreamWriter(Logfile, System.Text.Encoding.UTF8);

                writer.WriteLine("-----------------------------------------------------");
                writer.WriteLine(System.DateTime.Now.ToString());
                writer.WriteLine(String.Format("元件：{0}", ObjNameString));
                writer.WriteLine(LogMessage);
                writer.WriteLine("-----------------------------------------------------\r\n\r\n");
                writer.Close();
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public void WriteLogFormat(string format, params object[] args)
        {
            try
            {
                string LogFileName = SavePathString + FileNameString + System.DateTime.Now.ToString("yyyy-MM-dd-HH") + ".txt";


                FileStream Logfile = new FileStream(LogFileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                //StreamWriter writer = new System.IO.StreamWriter(Logfile, System.Text.Encoding.GetEncoding("Big5"));
                StreamWriter writer = new System.IO.StreamWriter(Logfile, System.Text.Encoding.UTF8);

                writer.WriteLine("-----------------------------------------------------");
                writer.WriteLine(System.DateTime.Now.ToString());
                writer.WriteLine(String.Format("元件：{0}", ObjNameString));
                writer.WriteLine(String.Format(format, args));
                writer.WriteLine("-----------------------------------------------------\r\n\r\n");
                writer.Close();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
