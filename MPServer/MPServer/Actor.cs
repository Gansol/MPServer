using System;

namespace MPServer
{
    public class Actor
    {
        public Guid guid { get; set; }                  // 唯一識別碼
        public int PrimaryID { get; set; }              // 主索引
        public string Account { get; set; }             // 帳號
        public string Nickname { get; set; }            // 暱稱
        public byte Age { get; set; }                    // 年齡
        public byte Sex { get; set; }                    // 性別
        public string IP { get; set; }                  // 註冊IP位址

        public DateTime LoginTime { get; set; }         // 登入時間
        public string LoginStatus { get; set; }         // 登入狀態 0:登出  1:登入
        public int GameStatus { get; set; }             // 目前狀態 0:閒置  1:遊戲中

        public Actor(Guid guid, int primaryID, string account, string nickname, byte age, byte sex, string IP)
        {
            this.guid = guid;                           // this.Guid = class那邊的值 Guid = 建構式的值
            this.PrimaryID = primaryID;
            this.Account = account;
            this.Nickname = nickname;
            this.Age = age;
            this.Sex = sex;
            this.IP = IP;

        }
    }
}
