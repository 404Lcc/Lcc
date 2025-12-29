using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace LccHotfix
{
    /// <summary>
    /// Xml解析的一些辅助
    /// </summary>
    public static partial class XmlHelper
    {
        public static XmlDocument LoadXmlDocFromResPath(string resPath)
        {
            StreamReader sr = XmlResToStreamReader(resPath);
            XmlDocument doc = new XmlDocument();
            doc.Load(sr);
            return doc;
        }

        public static Stream BytesToStream(byte[] bytes)
        {
            Stream stream = new MemoryStream(bytes);
            return stream;
        }

#if ConsoleClient
        public static StreamReader XmlResToStreamReader(string resPath)
        {
            FileStream file = new FileStream(resPath, FileMode.Open, FileAccess.Read);
            return new StreamReader(file);
        }
#else
        public static UnityEngine.TextAsset GetTextAsset(string resPath)
        {
            //Resources.Load 用于非正式代码的调试
            UnityEngine.TextAsset str = UnityEngine.Resources.Load(resPath, typeof(UnityEngine.TextAsset)) as UnityEngine.TextAsset;
            CLHelper.Assert(str != null, "XmlHelper GetTextAsset == null resPath:" + resPath);
            return str;
        }

        public static StreamReader XmlResToStreamReader(string resPath)
        {
            UnityEngine.TextAsset textAssert = GetTextAsset(resPath);
            if (textAssert == null)
            {
                LogWrapper.LogError("XmlResToStreamReader textAssert == null  resPath:" + resPath);
                return null;
            }

            Stream stm = BytesToStream(textAssert.bytes);
            if (stm == null)
            {
                LogWrapper.LogError("XmlResToStreamReader BytesToStream == null  resPath:" + resPath);
                return null;
            }

            return new StreamReader(stm);
        }
#endif

        static public bool StrToEnumValue<TEnum>(string v, out TEnum result) where TEnum : struct
        {
            TEnum status1;
            try
            {
                status1 = (TEnum)System.Enum.Parse(typeof(TEnum), v);
                if (System.Enum.IsDefined(typeof(TEnum), status1))
                {
                    result = status1;
                    return true;
                }
            }
            catch (System.SystemException)
            {
                result = new TEnum();
                return false;
            }

            result = status1;
            return false;
        }

        public static void NodeInnerTextToEnumValue<TEnum>(XmlNode rootNode, string nodeName, out TEnum outValue) where TEnum : struct
        {
            XmlElement baseNode = rootNode.SelectSingleNode(nodeName) as XmlElement;
            if (baseNode == null)
            {
                LogWrapper.LogError("NodeInnerTextToEnumValue  Cant Find Node: " + nodeName);
                CLHelper.AssertBreak();
                outValue = default(TEnum);
                return;
            }

            if (!XmlHelper.StrToEnumValue(baseNode.InnerText, out outValue))
            {
                LogWrapper.LogError("NodeInnerTextToEnumValue ParseError  InnerText:" + baseNode.InnerText);
                CLHelper.AssertBreak();
            }
        }

        public static string SubNodeInnerText(XmlNode rootNode, string subNodeName)
        {
            XmlElement subNode = rootNode.SelectSingleNode(subNodeName) as XmlElement;
            if (subNode == null)
            {
                LogWrapper.LogError("SubNodeInnerText  Cant Find Node: " + subNodeName);
                CLHelper.AssertBreak();
                return "";
            }

            return subNode.InnerText;
        }

        public static string GetAttribute(XmlNode node, string attrName)
        {
            XmlElement eleNode = node as XmlElement;
            if (eleNode == null)
            {
                LogWrapper.LogError("GetAttribute  Cant as XmlElement: " + node.Name);
                CLHelper.AssertBreak();
                return "";
            }

            return eleNode.GetAttribute(attrName);
        }

        public static bool GetBool(XmlNode node, string attrName, bool defaultValue = false)
        {
            string str = XmlHelper.GetAttribute(node, attrName);
            if (!string.IsNullOrEmpty(str))
            {
                bool.TryParse(str, out var bValue);
                return bValue;
            }

            return defaultValue;
        }

        public static float GetFloat(XmlNode node, string attrName, float defaultValue = 0f)
        {
            string str = XmlHelper.GetAttribute(node, attrName);
            if (!string.IsNullOrEmpty(str))
            {
                float.TryParse(str, out var fValue);
                return fValue;
            }

            return defaultValue;
        }

        public static int GetInt(this XmlNode node, string attrName, int defaultValue = 0)
        {
            string str = XmlHelper.GetAttribute(node, attrName);
            if (!string.IsNullOrEmpty(str))
            {
                int.TryParse(str, out var iValue);
                return iValue;
            }

            return defaultValue;
        }


        public static string GetString(XmlNode node, string attrName, string defaultValue = null)
        {
            string str = XmlHelper.GetAttribute(node, attrName);
            if (str == null)
            {
                return defaultValue;
            }

            return str;
        }

        public static List<int> GetListInt(string str)
        {
            List<int> listInt = new List<int>();
            string[] strs = str.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string intStr in strs)
            {
                listInt.Add(int.Parse(intStr));
            }

            return listInt;
        }
    }
}