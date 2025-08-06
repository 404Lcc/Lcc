using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace LccEditor
{
    public class MenuEditorWindow : AMenuEditorWindow<MenuEditorWindow>
    {
        protected override void OnEnable()
        {
            List<MenuTreeAttribute> menuTreeAttributeList = new List<MenuTreeAttribute>();
            Assembly assembly = GetType().Assembly;
            foreach (Type item in assembly.GetTypes())
            {
                if (item.IsAbstract)
                    continue;
                
                MenuTreeAttribute menuTreeAttribute = (MenuTreeAttribute)item.GetCustomAttribute(typeof(MenuTreeAttribute), false);
                if (menuTreeAttribute != null)
                {
                    menuTreeAttribute.type = item;
                    menuTreeAttributeList.Add(menuTreeAttribute);

                }
            }
            menuTreeAttributeList = menuTreeAttributeList.OrderBy(item => item.order).ToList();
            foreach (var item in menuTreeAttributeList)
            {
                AddEditorWindow(item.type, item.name);
            }
        }
        
        [MenuItem("Lcc框架/工具箱")]
        public static void ShowFramework()
        {
            OpenEditorWindow("工具箱");
        }
    }
}