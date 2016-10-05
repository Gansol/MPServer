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
        /// <param name="jString">JsonString</param>
        /// <returns>string[,]</returns>
        public string[,] Json2Array(string jString)
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
    }
}
