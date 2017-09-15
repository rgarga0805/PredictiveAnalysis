using PredictiveAnalysis.Common;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace PredictiveAnalysis
{
    public static class CacheManager
    {
        private static Dictionary<string, object> _items;

        static CacheManager()
        {
            _items = new Dictionary<string, object>();
            WarmUp();
        }

        private static void WarmUp()
        {
            AddItem(CacheKeys.IndependantVariables, ConfigurationManager.AppSettings[CacheKeys.IndependantVariables]);
            AddItem(CacheKeys.OutputVariable, ConfigurationManager.AppSettings[CacheKeys.OutputVariable]);
            AddItem(CacheKeys.Rule, ConfigurationManager.AppSettings[CacheKeys.Rule]);
        }

        public static object GetItem(string key)
        {
            if (!_items.ContainsKey(key))
                throw new ArgumentOutOfRangeException();
            return _items[key];
        }

        public static void AddItem(string key, object value)
        {
            if (_items.ContainsKey(key))
                throw new ArgumentOutOfRangeException();
            _items.Add(key, value);
        }
    }
}