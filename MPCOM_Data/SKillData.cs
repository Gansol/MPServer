using System;
using System.Collections.Generic;
using System.Text;
//using System.Runtime.InteropServices; //＊＊＊＊Web 和 Mobile不能用!!!!!＊＊＊＊＊

namespace MPCOM
{
    [Serializable()]
    //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]　//＊＊＊＊Web 和 Mobile不能用!!!!!＊＊＊＊＊
    public struct SkillData
    {
        public string ReturnCode; //回傳值
        //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]　//＊＊＊＊Web 和 Mobile不能用!!!!!＊＊＊＊＊
        public string ReturnMessage; //回傳說明文字<30全形字

        public Int16 SkillID;
        public string SkillName;
        public byte SkillLeve;
        public byte SkillType;
        public byte SkillTime;
        public byte ColdDown;
        public byte Delay;
        public byte Energy;
        public byte ProbValue;
        public byte ProbDice;
        public byte Attr;
        public byte AttrDoce;

        public string skillProperty;
    }
}
