﻿using UnityEditor;
using UnityEngine;
using UnityEditor.Build;
using System.Collections.Generic;
using UnityEditor.Compilation;
using System.Reflection;
using System.Linq;
using System;

[InitializeOnLoad]
public class ES3ScriptingDefineSymbols
{
    static ES3ScriptingDefineSymbols()
    {
        SetDefineSymbols();
    }

    static void SetDefineSymbols() 
    {
        if (Type.GetType("Unity.VisualScripting.IncludeInSettingsAttribute, Unity.VisualScripting.Core") != null)
            SetDefineSymbol("UNITY_VISUAL_SCRIPTING");

        if (Type.GetType("Ludiq.IncludeInSettingsAttribute, Ludiq.Core.Runtime") != null)
            SetDefineSymbol("BOLT_VISUAL_SCRIPTING");

        if (Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro") != null)
            SetDefineSymbol("ES3_TMPRO");

        if (Type.GetType("UnityEngine.EventSystems.EventSystem, UnityEngine.UI") != null)
            SetDefineSymbol("ES3_UGUI");
    }

    internal static bool HasDefineSymbol(string symbol)
    {
#if UNITY_2021_2_OR_NEWER
        foreach (var target in GetAllNamedBuildTargets())
        {
            string[] defines;
            try
            {
                PlayerSettings.GetScriptingDefineSymbols(target, out defines);
                if (defines.Contains(symbol))
                    return true;
            }
            catch { }
        }
#else
        string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        var allDefines = new HashSet<string>(definesString.Split(';'));
        if (allDefines.Contains(symbol))
            return true;
#endif
        return false;
    }

    internal static void SetDefineSymbol(string symbol)
    {
#if UNITY_2021_2_OR_NEWER
        foreach (var target in GetAllNamedBuildTargets())
        {
            string[] defines;
            try
            {
                PlayerSettings.GetScriptingDefineSymbols(target, out defines);
                if (!defines.Contains(symbol))
                {
                    ArrayUtility.Add(ref defines, symbol);
                    PlayerSettings.SetScriptingDefineSymbols(target, defines);
                }
            }
            catch { }
        }
#else
        string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        var allDefines = new HashSet<string>(definesString.Split(';'));
        if (!allDefines.Contains(symbol))
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, string.Join(";", allDefines.Concat(new string[] { symbol }).ToArray()));
#endif
            return;
    }

    internal static void RemoveDefineSymbol(string symbol)
    {
#if UNITY_2021_2_OR_NEWER
        foreach (var target in GetAllNamedBuildTargets())
        {
            string[] defines;
            try
            {
                PlayerSettings.GetScriptingDefineSymbols(target, out defines);
                ArrayUtility.Remove(ref defines, symbol);
                PlayerSettings.SetScriptingDefineSymbols(target, defines);
            }
            catch { }
        }
#else
        string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        definesString.Replace(symbol + ";", ""); // With semicolon
        definesString.Replace(symbol, ""); // Without semicolon
        PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, definesString);
#endif
        return;
    }

#if UNITY_2021_2_OR_NEWER
    static List<NamedBuildTarget> GetAllNamedBuildTargets()
    {
        var staticFields = typeof(NamedBuildTarget).GetFields(BindingFlags.Public | BindingFlags.Static);
        var buildTargets = new List<NamedBuildTarget>();

        foreach (var staticField in staticFields)
        {
            // We exclude 'Unknown' because this can throw errors when used with certain methods.
            if (staticField.Name == "Unknown")
                continue;

            // A bug at Unity's end means that Stadia can throw an error.
            if (staticField.Name == "Stadia")
                continue;

            if (staticField.FieldType == typeof(NamedBuildTarget))
                buildTargets.Add((NamedBuildTarget)staticField.GetValue(null));
        }

        return buildTargets;
    }
#endif
}
