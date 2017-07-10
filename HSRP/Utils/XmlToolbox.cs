using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace HSRP
{
    public static class XmlToolbox
    {
        public static XDocument TryLoadXml(string filePath)
        {
            XDocument doc;
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    doc = XDocument.Load(filePath);
                }

                if (doc.Root == null) return null;
                return doc;
            }
            catch (Exception e)
            {
                Console.WriteLine("[XML] " + "Couldn't load xml document \"" + filePath + "\"!\n" + e);
                return null;
            }
        }

        public static void WriteXml(string filePath, XDocument xml, XmlWriterSettings settings = null)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    if (settings == null)
                    {
                        xml.Save(fs, SaveOptions.None);
                    }
                    else
                    {
                        using (XmlWriter writer = XmlWriter.Create(fs, settings))
                        {
                            xml.Save(fs);
                        }
                    }
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine("[XML] " + "Couldn't save xml document \"" + filePath + "\"!\n" + e);
            }
        }

        public static string GetAttributeString(XElement element, string name, string defaultValue)
        {
            if (element == null || element.Attribute(name) == null) return defaultValue;
            return GetAttributeString(element.Attribute(name), defaultValue);
        }

        private static string GetAttributeString(XAttribute attribute, string defaultValue)
        {
            string value = attribute.Value;
            if (String.IsNullOrEmpty(value)) return defaultValue;
            return value;
        }

        public static float GetAttributeFloat(XElement element, string name, float defaultValue)
        {
            if (element == null || element.Attribute(name) == null) return defaultValue;

            float val = defaultValue;

            try
            {
                if (!float.TryParse(element.Attribute(name).Value, NumberStyles.Float, CultureInfo.InvariantCulture, out val))
                {
                    return defaultValue;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[XML] " + "Error in "+element+"!\n" + e);
            }
            
            return val;
        }

        private static float GetAttributeFloat(XAttribute attribute, float defaultValue)
        {
            if (attribute == null) return defaultValue;

            float val = defaultValue;

            try
            {
                val = float.Parse(attribute.Value, CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                Console.WriteLine("[XML] " + "Error in " + attribute + "! \n" + e);
            }

            return val;
        }

        public static int GetAttributeInt(XElement element, string name, int defaultValue)
        {
            if (element == null || element.Attribute(name) == null) return defaultValue;

            int val = defaultValue;

            try
            {
                val = int.Parse(element.Attribute(name).Value);
            }
            catch (Exception e)
            {
                Console.WriteLine("[XML] " + "Error in " + element + "! \n" + e);
            }

            return val;
        }

        public static byte GetAttributeByte(XElement element, string name, byte defaultValue)
        {
            if (element == null || element.Attribute(name) == null) return defaultValue;

            byte val = defaultValue;

            try
            {
                val = byte.Parse(element.Attribute(name).Value);
            }
            catch (Exception e)
            {
                Console.WriteLine("[XML] " + "Error in " + element + "! \n" + e);
            }

            return val;
        }

        public static long GetAttributeLong(XElement element, string name, long defaultValue)
        {
            if (element == null || element.Attribute(name) == null) return defaultValue;

            long val = defaultValue;

            try
            {
                val = long.Parse(element.Attribute(name).Value);
            }
            catch (Exception e)
            {
                Console.WriteLine("[XML] " + "Error in " + element + "! \n" + e);
            }

            return val;
        }

        public static ulong GetAttributeUnsignedLong(XElement element, string name, ulong defaultValue)
        {
            if (element == null || element.Attribute(name) == null) return defaultValue;

            ulong val = defaultValue;

            try
            {
                val = ulong.Parse(element.Attribute(name).Value);
            }
            catch (Exception e)
            {
                Console.WriteLine("[XML] " + "Error in " + element + "! \n" + e);
            }

            return val;
        }

        public static int[] GetAttributeIntArray(XElement element, string name, int[] defaultValue)
        {
            if (element == null || element.Attribute(name) == null) return defaultValue;

            return GetAttributeIntArray(element.Attribute(name), defaultValue);
        }

        private static int[] GetAttributeIntArray(XAttribute attribute, int[] defaultValue)
        {
            if (attribute == null) return defaultValue;

            int[] val = defaultValue;
            try
            {
                string[] content = attribute.Value.Split(',');
                for (int i = 0; i < content.Length; i++)
                {
                    val[i] = int.Parse(content[i], CultureInfo.InvariantCulture);
                }
            }
            catch
            {
                // Don't bother throwing an error here because it'll spam the console.
                //Console.WriteLine("[XML] " + "Error in " + attribute + "! \n" + e);
                val = defaultValue;
            }

            return val;
        }

        public static bool GetAttributeBool(XElement element, string name, bool defaultValue)
        {
            if (element == null || element.Attribute(name) == null) return defaultValue;

            return GetAttributeBool(element.Attribute(name), defaultValue);
        }

        private static bool GetAttributeBool(XAttribute attribute, bool defaultValue)
        {
            if (attribute == null) return defaultValue;

            string val = attribute.Value.ToLowerInvariant().Trim();
            if (val == "true")
            {
                return true;
            }
            else if (val == "false")
            {
                return false;
            }
            else
            {
                Console.WriteLine("[XML] " + "Error in " + attribute.Value.ToString() + "! \"" + val + "\" is not a valid boolean value");
                return false;
            }
        }

        public static T GetAttributeEnum<T>(XElement element, string name, T defaultValue) where T : struct, IConvertible
        {
            if (!typeof(T).GetTypeInfo().IsEnum) 
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            if (element == null || element.Attribute(name) == null) return defaultValue;

            T val = defaultValue;

            try
            {
                val = (T)Enum.Parse(typeof(T), element.Attribute(name).Value);
            }
            catch (Exception e)
            {
                Console.WriteLine("[XML] " + "Error in " + element + "! \n" + e);
            }

            return val;
        }

        public static string ElementInnerText(this XElement el)
        {
            StringBuilder str = new StringBuilder();
            foreach (XNode element in el.DescendantNodes().Where(x => x.NodeType == XmlNodeType.Text))
            {
                str.Append(element.ToString());
            }
            return str.ToString();
        }

        // public static string RandomSeed(int length)
        // {
        //     var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        //     return new string(
        //         Enumerable.Repeat(chars, length)
        //                   .Select(s => s[Rand.Int(s.Length)])
        //                   .ToArray());
        // }

        // public static int StringToInt(string str)
        // {
        //     str = str.Substring(0, Math.Min(str.Length, 32));

        //     str = str.PadLeft(4, 'a');

        //     byte[] asciiBytes = Encoding.ASCII.GetBytes(str);

        //     for (int i = 4; i < asciiBytes.Length; i++)
        //     {
        //         asciiBytes[i % 4] ^= asciiBytes[i];
        //     }

        //     return BitConverter.ToInt32(asciiBytes, 0);
        // }

        /// <summary>
        /// Calculates the minimum number of single-character edits (i.e. insertions, deletions or substitutions) required to change one string into the other
        /// </summary>
        // public static int LevenshteinDistance(string s, string t)
        // {
        //     int n = s.Length;
        //     int m = t.Length;
        //     int[,] d = new int[n + 1, m + 1];

        //     if (n == 0)
        //     {
        //         return m;
        //     }

        //     if (m == 0)
        //     {
        //         return n;
        //     }

        //     for (int i = 0; i <= n; d[i, 0] = i++)
        //     {
        //     }

        //     for (int j = 0; j <= m; d[0, j] = j++)
        //     {
        //     }

        //     for (int i = 1; i <= n; i++)
        //     {
        //         for (int j = 1; j <= m; j++)
        //         {
        //             int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

        //             d[i, j] = Math.Min(
        //             Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
        //             d[i - 1, j - 1] + cost);
        //         }
        //     }

        //     return d[n, m];
        // }

        // public static string SecondsToReadableTime(float seconds)
        // {
        //     if (seconds < 60.0f)
        //     {
        //         return (int)seconds + " s";
        //     }
        //     else
        //     {
        //         int m = (int)(seconds / 60.0f);
        //         int s = (int)(seconds % 60.0f);

        //         return s == 0 ?
        //             m + " m" :
        //             m + " m " + s + " s";
        //     }
        // }
    }
}
