using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace HSRP
{
    public static class Toolbox
    {
        public static Dictionary<string, string[]> Messages;
        public static Dictionary<string, string> SingleMsg;

        public static int DiceRoll(int rolls, int dieType = 6)
        {
            if (rolls < 1 || dieType < 1) { return 1; }

            int total = 0;
            for (int i = 0; i < rolls; i++)
            {
                total += RandInt(1, dieType, true);
            }

            return total;
        }

        // Random int generator that can output different
        // numbers in the same tick.
        private static readonly Random rand = new Random();
        private static readonly object randLock = new object();
        public static int RandInt(int min, int max, bool inclusive = false)
        {
            // Locking allows different numbers per tick.
            lock (randLock)
            {
                return inclusive
                    ? rand.Next(max - min + 1) + min
                    : rand.Next(min, max);
            }
        }

        public static int RandInt(int max, bool inclusive = false) => RandInt(0, max, inclusive);

        public static void UpdateMessages()
        {
            Messages = new Dictionary<string, string[]>();
            SingleMsg = new Dictionary<string, string>();

            XDocument doc = XmlToolbox.TryLoadXml(Path.Combine(Dirs.Config, "messages.xml"));
            if (doc == null || doc.Root == null) { return; }

            foreach (XElement ele in doc.Root.Elements())
            {
                switch (ele.Name.LocalName)
                {
                    case "messages":
                        string key = XmlToolbox.GetAttributeString(ele, "trigger", string.Empty);
                        if (string.IsNullOrEmpty(key)) { continue; }

                        List<string> value = new List<string>();
                        foreach (XElement msg in ele.Elements())
                        {
                            value.Add(XmlToolbox.ElementInnerText(msg));
                        }

                        Messages.Add(key, value.ToArray());
                        break;
                    
                    case "msg":
                        string singleKey = XmlToolbox.GetAttributeString(ele, "trigger", string.Empty);
                        string singleValue = XmlToolbox.ElementInnerText(ele);
                        SingleMsg.Add(singleKey, singleValue);
                        break;
                }
                
            }
        }
        /// <summary>
        /// Generates a random message from a specific category.
        /// </summary>
        public static string GetRandomMessage(string key, params string[] args)
        {
            if (Messages.TryGetValue(key, out string[] value))
            {
                if (args == null)
                {
                    return value[RandInt(value.Length)];
                }
                
                string msg = value[RandInt(value.Length)];
                for (int i = 0; i < args.Length; i++)
                {
                    msg = msg.Replace("{" + i + "}", args[i]);
                }
                return msg;
            }

            return string.Empty;
        }
        /// <summary>
        /// Gets a message from a specific category.
        /// </summary>
        public static string GetSingleMessage(string key, params string[] args)
        {
            if (SingleMsg.TryGetValue(key, out string value))
            {
                if (args == null)
                {
                    return value;
                }
                
                for (int i = 0; i < args.Length; i++)
                {
                    value = value.Replace("{" + i + "}", args[i]);
                }
                return value;
            }

            return string.Empty;
        }

        public static void DebugWriteLine(params object[] args)
        {
            foreach (object obj in args)
            {
                Console.WriteLine(obj.ToString());
            }
        }

        // Extensions.
        public static string AddLine(this string str, string appended)
        {
            return str += appended + "\n";
        }

        public static string ToApostrophe(this string str)
        {
            return str.EndsWith("s", StringComparison.CurrentCultureIgnoreCase)
                ? str + "'"
                : str + "'s";
        }

        public static string FirstCharUpper(this string text)
        {
            return text.Substring(0, 1).ToUpper() + text.Substring(1, text.Length - 1).ToLower();
        }

        public static bool Contains(this string text, string value,
            StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        {
            return text.IndexOf(value, stringComparison) >= 0;
        }
    }
}
