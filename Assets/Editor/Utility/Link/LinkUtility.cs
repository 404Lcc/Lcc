using LccModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;

namespace LccEditor
{
    public static class LinkUtility
    {
        public static void BuildLink()
        {
            List<Assembly> assemblieList = new List<Assembly>();
            assemblieList.Add(typeof(Object).Assembly);
            assemblieList.Add(typeof(UnityEngine.Object).Assembly);
            assemblieList.Add(typeof(Transform).Assembly);
            assemblieList.Add(typeof(GameObject).Assembly);
            assemblieList.Add(typeof(Image).Assembly);
            assemblieList.Add(typeof(Init).Assembly);
            string[] filePaths = Directory.GetFiles("Assets", "*.dll", SearchOption.AllDirectories);
            foreach (string item in filePaths)
            {
                if (item.ToLower().Contains("editor") || item.ToLower().Contains("plugins"))
                {
                    continue;
                }
                assemblieList.Add(Assembly.LoadFrom(item));
            }
            assemblieList = assemblieList.Distinct().ToList();
            XmlDocument xmlDocument = new XmlDocument();
            XmlElement linkerElement = xmlDocument.CreateElement("linker");
            foreach (Assembly item in assemblieList)
            {
                XmlElement assemblyElement = xmlDocument.CreateElement("assembly");
                assemblyElement.SetAttribute("fullname", item.GetName().Name);
                foreach (Type typeItem in item.GetTypes())
                {
                    if (typeItem.FullName == "Win32")
                    {
                        continue;
                    }
                    XmlElement typeElement = xmlDocument.CreateElement("type");
                    typeElement.SetAttribute("fullname", typeItem.FullName);
                    typeElement.SetAttribute("preserve", "all");
                    //增加子节点
                    assemblyElement.AppendChild(typeElement);
                }
                linkerElement.AppendChild(assemblyElement);
            }
            xmlDocument.AppendChild(linkerElement);
            string path = "Assets/link.xml";
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            xmlDocument.Save(path);
        }
    }
}