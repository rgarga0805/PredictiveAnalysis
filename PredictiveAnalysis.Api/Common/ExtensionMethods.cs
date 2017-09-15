using System;

namespace PredictiveAnalysis.Common
{
    public static class ExtensionMethods
    {
        public static T ConvertTo<T>(this object value, T defaultValue = default(T))
        {
            T convertedValue = defaultValue;

            if (value == null || value==DBNull.Value)
                return defaultValue;

            if (typeof(T) == typeof(string))
            {
                if (!String.IsNullOrEmpty(value.ToString()))
                    convertedValue = (T)Convert.ChangeType(value, typeof(string));
            }

            if (typeof(T) == typeof(float))
            {
                convertedValue = (T)Convert.ChangeType(value, typeof(float));
            }

            return convertedValue;
        }
    }
}