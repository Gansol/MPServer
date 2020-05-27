using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Gansol
{
    public static class GansolExtensions
    {
        #region List Extensions
        /// <summary>
        /// 隨機打亂清單值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void Shuffle<T>(this IList<T> list)
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            int n = list.Count;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        #endregion

        #region RNGCryptoServiceProvider Extensions
        private static byte[] rb = new byte[4];

        /// <summary>
        /// 產生一個非負數的亂數
        /// </summary>
        public static int Next(this RNGCryptoServiceProvider rngp)
        {
            rngp.GetBytes(rb);
            int value = BitConverter.ToInt32(rb, 0);
            return (value < 0) ? -value : value;
        }
        /// <summary>
        /// 產生一個非負數且最大值 max 以下的亂數
        /// </summary>
        /// <param name="max">最大值</param>
        public static int Next(this RNGCryptoServiceProvider rngp, int max)
        {
            return Next(rngp) % (max + 1);
        }
        /// <summary>
        /// 產生一個非負數且最小值在 min 以上最大值在 max 以下的亂數
        /// </summary>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        public static int Next(this RNGCryptoServiceProvider rngp, int min, int max)
        {
            return Next(rngp, max - min) + min;
        }
        #endregion

        #region DictionaryExtensions TryGet

        public static T Get<T>(this IDictionary<string, object> dictionary, string key)
        {
            return (T)dictionary[key];
        }

        public static bool TryGet<T>(this IDictionary<string, object> dictionary,
                                     string key, out T value)
        {
            object result;
            if (dictionary.TryGetValue(key, out result) && result is T)
            {
                value = (T)result;
                return true;
            }
            value = default(T);
            return false;
        }

        public static void Set(this IDictionary<string, object> dictionary,
                               string key, object value)
        {
            dictionary[key] = value;
        }

        #endregion
    }
}
