﻿using UnityEditor;
using UnityEngine;

namespace LccEditor
{
    public class LogConfig
    {
        public string path;
        public string name;
        public int instanceId;
        public LogConfig()
        {
        }
        public LogConfig(string path, string name)
        {
            this.path = path;
            this.name = name;
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            instanceId = asset.GetInstanceID();
        }
    }
}