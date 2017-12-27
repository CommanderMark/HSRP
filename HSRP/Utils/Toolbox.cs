﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace HSRP
{
    public static class Toolbox
    {
        public static Dictionary<string, string[]> Messages;
        public static Dictionary<string, StatusEffect> StatusEffects;

        static Toolbox()
        {
            UpdateMessages();
            UpdateStatusEffects();
        }

        public static int DiceRoll(int rolls, int dieType = 6)
        {
            if (rolls < 1 || dieType < 1) { return 1; }

            int total = 0;
            for (int i = 0; i < rolls; i++)
            {
                total += RandInt(1, dieType + 1);
            }

            return total;
        }

        private static readonly RandomNumberGenerator generator = RandomNumberGenerator.Create();
        public static int RandInt(int minimumValue, int maximumValue)
        {
            byte[] randomNumber = new byte[1];

            generator.GetBytes(randomNumber);

            double asciiValueOfRandomCharacter = Convert.ToDouble(randomNumber[0]);

            // We are using Math.Max, and substracting 0.00000000001, 
            // to ensure "multiplier" will always be between 0.0 and .99999999999
            // Otherwise, it's possible for it to be "1", which causes problems in our rounding.
            double multiplier = Math.Max(0, (asciiValueOfRandomCharacter / 255d) - 0.00000000001d);

            // We need to add one to the range, to allow for the rounding done with Math.Floor
            float range = maximumValue - minimumValue + 1;

            double randomValueInRange = Math.Floor(multiplier * range);

            return (int) (minimumValue + randomValueInRange);
        }
        public static int RandInt(int max) => RandInt(0, max);

        public static float RandFloat(float min, float max)
        {
            return RandInt((int) (min * 1000), (int) (max * 1000)) / 1000f;
        }

        /// <summary>
        /// Returns true or false randomly.
        /// </summary>
        public static bool TrueOrFalse() => RandInt(2) == 0;

        public static bool TrueOrFalse(int denominator) => RandInt(denominator) == 0;

        public static void UpdateMessages()
        {
            Messages = new Dictionary<string, string[]>();

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

                        if (ele.Elements("msg").Any())
                        {
                            foreach (XElement msg in ele.Elements())
                            {
                                value.Add(XmlToolbox.ElementInnerText(msg));
                            }
                        }
                        else
                        {
                            string singleValue = XmlToolbox.ElementInnerText(ele);
                            value.Add(singleValue);
                        }

                        Messages.Add(key, value.ToArray());
                        break;
                }
                
            }
        }

        public static void UpdateStatusEffects()
        {
            StatusEffects = new Dictionary<string, StatusEffect>();

            XDocument doc = XmlToolbox.TryLoadXml(Path.Combine(Dirs.Config, "ailments.xml"));
            if (doc == null || doc.Root == null)
            { return; }

            foreach (XElement ele in doc.Root.Elements())
            {
                StatusEffect sa = new StatusEffect(ele);
                StatusEffects.Add(sa.Name, sa);
            }
        }

        /// <summary>
        /// Generates a message from a specific category.
        /// </summary>
        public static string GetMessage(string key, params string[] args)
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

            Console.WriteLine("Failed to get config message " + key + "!");
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

        public static bool Contains(this string text, string value, StringComparison stringComparison)
        {
            return text.IndexOf(value, stringComparison) >= 0;
        }
    }
}
