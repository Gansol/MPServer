using ExitGames.Logging;
using ExitGames.Logging.Log4Net;
using log4net.Config;
using Photon.SocketServer;
using System;
using System.IO;
using System.Collections.Generic;

/*
using MPCOM;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using MiniJSON;
*/

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
 * 管理 全部 伺服器物件實體化 建立與Client的連線
 * 
 * ***************************************************************/

namespace MPServer
{
    public class MPServerApplication :ApplicationBase
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        public ActorCollection Actors = new ActorCollection();
        public PeerBase peer = null;
        public Room room = new Room();
        //public Dictionary<int,object> MiceData = new Dictionary<int,object>();      // 待測試

        /*
        public Dictionary<string, object> dictMiceProperty = new Dictionary<string,object>();
        MiceData miceData = new MiceData();
        */

        // 建立peer連線時 取得Client的Protocol、Peer資料 
        protected override PeerBase CreatePeer(InitRequest initRequest) 
        {
            return new MPServerPeer(initRequest.Protocol, initRequest.PhotonPeer,this);
            
        }

        // 伺服器初始化時
        protected override void Setup()
        {
            log4net.GlobalContext.Properties["Photon:ApplicationLogPath"] = Path.Combine(this.ApplicationRootPath, "log");

            // log4net
            string path = Path.Combine(this.BinaryPath, "log4net.config");
            var file = new FileInfo(path);
            if (file.Exists)
            {
                LogManager.SetLoggerFactory(Log4NetLoggerFactory.Instance);
                XmlConfigurator.ConfigureAndWatch(file);
            }

            Log.Info("MPServer is running...");
        }

        // 伺服器關閉時 實作釋放資源
        protected override void TearDown()
        {
            Log.Debug("Shutdown MPServer Server ...");
        }
    }
}
