using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SLD.Net10.Common
{
    public static class ObjectToDictExtension
    {
        /// <summary>
        /// 对象转 Dictionary&lt;string, string&gt;  key:属性名，value:值ToString()
        /// </summary>
        public static Dictionary<string, string> ToDictString(this object obj)
        {
            var dict = new Dictionary<string, string>();
            if (obj == null) return dict;

            PropertyInfo[] props = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in props)
            {
                if (!prop.CanRead) continue;

                object val = prop.GetValue(obj);
                string strVal = val == null ? string.Empty : val.ToString();
                dict.Add(prop.Name, strVal);
            }
            return dict;
        }
    }
}
