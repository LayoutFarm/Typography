//BSD, 2014-present, WinterDev

using System;
using System.Collections.Generic;
#if PORTABLE
using System.Reflection;
using System.Linq;
#endif
namespace LayoutFarm.WebDom
{
    public class ValueMap<T>
    {
        static Type s_mapNameAttrType = typeof(MapAttribute);
        readonly Dictionary<string, T> _stringToValue;
        readonly Dictionary<T, string> _valueToString;
        public ValueMap()
        {
            LoadAndAssignValues(out _stringToValue, out _valueToString);
        }


        static void LoadAndAssignValues(out Dictionary<string, T> stringToValue, out Dictionary<T, string> valueToString)
        {
            stringToValue = new Dictionary<string, T>();
            valueToString = new Dictionary<T, string>();
#if PORTABLE
            var fields = typeof(T).GetTypeInfo().DeclaredFields.ToArray();
#else
            var fields = typeof(T).GetFields();
#endif

            for (int i = fields.Length - 1; i >= 0; --i)
            {
                var field = fields[i];
                MapAttribute cssNameAttr = null;
#if PORTABLE
                var customAttrs = field.GetCustomAttributes(mapNameAttrType, false).ToArray();
#else
                var customAttrs = field.GetCustomAttributes(s_mapNameAttrType, false);
#endif
                if (customAttrs != null && customAttrs.Length > 0 &&
                   (cssNameAttr = customAttrs[0] as MapAttribute) != null)
                {
                    T value = (T)field.GetValue(null);
                    stringToValue.Add(cssNameAttr.Name, value);//1.
                    valueToString.Add(value, cssNameAttr.Name);//2.                   
                }
            }
        }
        public string GetStringFromValue(T value)
        {
            string found;
            _valueToString.TryGetValue(value, out found);
            return found;
        }
        public T GetValueFromString(string str, T defaultIfNotFound)
        {
            T found;
            if (_stringToValue.TryGetValue(str, out found))
            {
                return found;
            }
            return defaultIfNotFound;
        }
        public int Count
        {
            get
            {
                return _valueToString.Count;
            }
        }
    }
}