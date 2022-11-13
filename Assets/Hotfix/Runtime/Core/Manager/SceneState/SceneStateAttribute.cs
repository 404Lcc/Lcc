using System;
using System.Collections.Generic;
using System.Reflection;

namespace LccHotfix
{
    public class SceneStateAttribute : Attribute
    {
        public string sceneName;

        public List<string> paramList = new List<string>();

        public List<string> targetNameList = new List<string>();
        public List<Func<bool>> conditionList = new List<Func<bool>>();
        public SceneStateAttribute(string sceneName, params string[] paramList)
        {
            this.sceneName = sceneName;
            this.paramList.AddRange(paramList);
        }
    }
}