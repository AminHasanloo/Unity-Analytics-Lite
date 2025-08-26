// Copyright (c) 2025 Amin Hasanloo
// Simple JSON serializer (encode-only) for Dictionary/List/string/number/bool/null

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace AHL.AnalyticsLite
{
    public static class SimpleJson
    {
        public static string Serialize(object obj)
        {
            var sb = new StringBuilder(256);
            SerializeValue(obj, sb);
            return sb.ToString();
        }

        static void SerializeValue(object value, StringBuilder sb)
        {
            if (value == null) { sb.Append("null"); return; }
            if (value is string s) { SerializeString(s, sb); return; }
            if (value is bool b) { sb.Append(b ? "true" : "false"); return; }

            if (value is IDictionary dict) { SerializeObject(dict, sb); return; }

            if (value is IEnumerable enumerable && !(value is string))
            {
                SerializeArray(enumerable, sb);
                return;
            }

            if (IsNumeric(value))
            {
                sb.Append(Convert.ToString(value, CultureInfo.InvariantCulture));
                return;
            }

            // fallback
            SerializeString(value.ToString(), sb);
        }

        static bool IsNumeric(object value)
        {
            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        static void SerializeObject(IDictionary obj, StringBuilder sb)
        {
            sb.Append('{');
            bool first = true;
            foreach (DictionaryEntry e in obj)
            {
                if (!(e.Key is string)) continue; // only string keys
                if (!first) sb.Append(',');
                SerializeString((string)e.Key, sb);
                sb.Append(':');
                SerializeValue(e.Value, sb);
                first = false;
            }
            sb.Append('}');
        }

        static void SerializeArray(IEnumerable array, StringBuilder sb)
        {
            sb.Append('[');
            bool first = true;
            foreach (var o in array)
            {
                if (!first) sb.Append(',');
                SerializeValue(o, sb);
                first = false;
            }
            sb.Append(']');
        }

        static void SerializeString(string str, StringBuilder sb)
        {
            sb.Append('\"');
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                switch (c)
                {
                    case '\"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 32 || c > 126)
                        {
                            sb.Append("\\u");
                            sb.Append(((int)c).ToString("x4"));
                        }
                        else sb.Append(c);
                        break;
                }
            }
            sb.Append('\"');
        }
    }
}
