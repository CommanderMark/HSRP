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
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    doc = XDocument.Load(filePath);
                }

                if (doc.Root == null)
                {
                    return null;
                }

                return doc;
            }
            catch
            {
                Console.WriteLine("[XML] " + "Couldn't load xml document \"" + filePath + "\"!");
                return null;
            }
        }

        public static void WriteXml(string filePath, XDocument xml, XmlWriterSettings settings = null)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
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

        public static string GetAttributeString(this XElement element, string name, string defaultValue)
        {
            if (element == null || element.Attribute(name) == null)
            {
                return defaultValue;
            }

            return GetAttributeString(element.Attribute(name), defaultValue);
        }

        private static string GetAttributeString(XAttribute attribute, string defaultValue)
        {
            string value = attribute.Value;
            if (String.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            return value;
        }

        public static string[] GetAttributeStringArray(this XElement element, string name, string[] defaultValue)
        {
            if (element == null || element.Attribute(name) == null)
            {
                return defaultValue;
            }

            return GetAttributeStringArray(element.Attribute(name), defaultValue);
        }

        private static string[] GetAttributeStringArray(XAttribute attribute, string[] defaultValue)
        {
            if (attribute == null)
            {
                return defaultValue;
            }

            string[] val = defaultValue;
            try
            {
                string[] content = attribute.Value.Split(',');
                val = content;
            }
            catch (Exception e)
            {
                Console.WriteLine("[XML] " + "Error in " + attribute + "! \n" + e);
                val = defaultValue;
            }

            return val;
        }

        public static float GetAttributeFloat(this XElement element, string name, float defaultValue)
        {
            if (element == null || element.Attribute(name) == null)
            {
                return defaultValue;
            }

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

        public static int GetAttributeInt(this XElement element, string name, int defaultValue)
        {
            if (element == null || element.Attribute(name) == null)
            {
                return defaultValue;
            }

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

        public static byte GetAttributeByte(this XElement element, string name, byte defaultValue)
        {
            if (element == null || element.Attribute(name) == null)
            {
                return defaultValue;
            }

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

        public static long GetAttributeLong(this XElement element, string name, long defaultValue)
        {
            if (element == null || element.Attribute(name) == null)
            {
                return defaultValue;
            }

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

        public static ulong GetAttributeUnsignedLong(this XElement element, string name, ulong defaultValue)
        {
            if (element == null || element.Attribute(name) == null)
            {
                return defaultValue;
            }

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

        public static int[] GetAttributeIntArray(this XElement element, string name, int[] defaultValue)
        {
            if (element == null || element.Attribute(name) == null)
            {
                return defaultValue;
            }

            return GetAttributeIntArray(element.Attribute(name), defaultValue);
        }

        private static int[] GetAttributeIntArray(XAttribute attribute, int[] defaultValue)
        {
            if (attribute == null)
            {
                return defaultValue;
            }

            int[] val = defaultValue;
            try
            {
                string[] content = attribute.Value.Split(',');
                for (int i = 0; i < content.Length; i++)
                {
                    val[i] = int.Parse(content[i], CultureInfo.InvariantCulture);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[XML] " + "Error in " + attribute + "! \n" + e);
                val = defaultValue;
            }

            return val;
        }

        public static bool GetAttributeBool(this XElement element, string name, bool defaultValue)
        {
            if (element == null || element.Attribute(name) == null)
            {
                return defaultValue;
            }

            return GetAttributeBool(element.Attribute(name), defaultValue);
        }

        private static bool GetAttributeBool(XAttribute attribute, bool defaultValue)
        {
            if (attribute == null)
            {
                return defaultValue;
            }

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

        public static T GetAttributeEnum<T>(this XElement element, string name, T defaultValue) where T : struct, IConvertible
        {
            if (!typeof(T).GetTypeInfo().IsEnum) 
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            if (element == null || element.Attribute(name) == null)
            {
                return defaultValue;
            }

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

        public static T[] GetAttributeEnumArray<T>(this XElement element, string name, T[] defaultValue) where T : struct, IConvertible
        {
            if (!typeof(T).GetTypeInfo().IsEnum) 
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            if (element == null || element.Attribute(name) == null)
            {
                return defaultValue;
            }

            return GetAttributeEnumArray(element.Attribute(name), defaultValue);
        }

        private static T[] GetAttributeEnumArray<T>(XAttribute attribute, T[] defaultValue)
        {
            if (attribute == null)
            {
                return defaultValue;
            }

            T[] val = defaultValue;
            try
            {
                string[] content = attribute.Value.Split(',');
                for (int i = 0; i < content.Length; i++)
                {
                    val[i] = (T)Enum.Parse(typeof(T), content[i]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[XML] " + "Error in " + attribute + "! \n" + e);
                val = defaultValue;
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
    }
}
