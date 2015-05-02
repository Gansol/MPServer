using System;
using System.Collections.Generic;
using System.Text;
//using System.Runtime.InteropServices; //＊＊＊＊Web 和 Mobile不能用!!!!!＊＊＊＊＊

namespace MPCOM
{
    [Serializable()]
    //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]　//＊＊＊＊Web 和 Mobile不能用!!!!!＊＊＊＊＊
    public struct MiceData
    {
        public string ReturnCode; //回傳值
        //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]　//＊＊＊＊Web 和 Mobile不能用!!!!!＊＊＊＊＊
        public string ReturnMessage; //回傳說明文字<30全形字


		/*
        //public byte[] miceID;		// 老鼠ID
        public float[] eatingRate;	// 吃東西速度
        public float[] miceSpeed;		// 老鼠 速度
        public float[] eatFull;		// 飽度
        public byte[] bossSkill;		// Boss 技能
        public Int16[] bossHP;		// Boss 血量
        public byte[] miceCost;		// 老鼠 花費
        public Int16[] price;		// 老鼠 價格

        //public Dictionary<byte, string> miceProperty;
         * */

        public string miceProperty;
    }
}
