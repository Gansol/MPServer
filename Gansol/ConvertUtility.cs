using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MiniJSON;

namespace Gansol
{
    public partial class ConvertUtility : Component
    {
        public ConvertUtility()
        {
            InitializeComponent();
        }

        public ConvertUtility(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        /// <summary>
        /// 轉換Json字串為2維陣列 e.g. {A1,A2},{B1,B2} 
        /// </summary>
        /// <param name="MiniJsonString">JsonString</param>
        /// <returns>string[,]</returns>
        public string[,] MiniJson2Array(string jString)
        {
            Dictionary<string, object> dictData = Json.Deserialize(jString) as Dictionary<string, object>;
            int arrayY = 0;
            foreach (KeyValuePair<string, object> item in dictData)
            {
                var innDict = item.Value as Dictionary<string, object>;
                arrayY = innDict.Count;
                break;
            }

            string[,] storageArray = new string[dictData.Count, arrayY];
            int i = 0;

            foreach (KeyValuePair<string, object> item in dictData)
            {
                int j = 0;
                var innDict = item.Value as Dictionary<string, object>;

                foreach (KeyValuePair<string, object> inner in innDict)
                {
                    storageArray[i, j] = inner.Value.ToString();
                    j++;
                }
                i++;
            }
            return storageArray;
        }

        //#region -- Rename Dictionary Key --
        ///// <summary>
        ///// Rename Dictionary Key
        ///// </summary>
        ///// <typeparam name="TKey"></typeparam>
        ///// <typeparam name="TValue"></typeparam>
        ///// <param name="dic"></param>
        ///// <param name="fromKey"></param>
        ///// <param name="toKey"></param>
        //public static void RenameKey<TKey, TValue>(this IDictionary<TKey, TValue> dic,
        //                          TKey fromKey, TKey toKey)
        //{
        //    TValue value = dic[fromKey];
        //    dic.Remove(fromKey);
        //    dic[toKey] = value;
        //} 
        //#endregion

        //#region -- SwapDictElemts --
        ///// <summary>
        ///// 交換字典物件
        ///// </summary>
        ///// <param name="vaule1"></param>
        ///// <param name="value2"></param>
        ///// <param name="dict"></param>
        //public void SwapDictElemts(string vaule1, string value2, Dictionary<string, object> dict)
        //{
        //    string myKey = "", otherKey = "";
        //    object myValue = "", otherValue = "";

        //    foreach (KeyValuePair<string, object> item in dict)
        //    {
        //        if (item.Value.ToString() == vaule1) myKey = item.Key;
        //        if (item.Value.ToString() == value2) otherKey = item.Key;
        //    }

        //    dict[myKey] = value2;
        //    dict[otherKey] = vaule1;
        //    RenameKey(dict, myKey, "x");
        //    RenameKey(dict, otherKey, myKey);
        //    RenameKey(dict, "x", otherKey);
        //} 
        //#endregion
    }
}
