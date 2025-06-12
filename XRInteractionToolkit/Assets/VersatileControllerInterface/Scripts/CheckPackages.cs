#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.Build;
using UnityEngine;

using System;

[InitializeOnLoad]
public static class CheckPackages
{
    static CheckPackages()
    {
        var listRequest = Client.List(true); // true = include indirect dependencies
        EditorApplication.update += () => CheckPackage(listRequest, "com.unity.xr.management", "UNITY_XR_INSTALLED");
    }

    private static void CheckPackage(ListRequest request, string package, string define)
    {
        if (!request.IsCompleted)
            return;

        bool installed = false;
        if (request.Status == StatusCode.Success)
        {
            foreach (var pkg in request.Result)
            {
                if (pkg.name == package)
                {
                    installed = true;
                    break;
                }
            }
        }
        else
        {
            Debug.LogError("Failed to retrieve package list: " + request.Error.message);
        }

        SetDefine (define, installed);
        EditorApplication.update -= () => CheckPackage (request, package, define);
    }

    private static void SetDefine(string symbol, bool enable)
    {
        foreach (BuildTargetGroup group in System.Enum.GetValues(typeof(BuildTargetGroup)))
        {
            if (group == BuildTargetGroup.Unknown) continue;
            try 
            {
                NamedBuildTarget target = NamedBuildTarget.FromBuildTargetGroup (group);
                string defines = PlayerSettings.GetScriptingDefineSymbols (target);
                var symbols = new System.Collections.Generic.HashSet<string>(defines.Split(';'));
            
                bool changed = false;
                if (enable && symbols.Add(symbol))
                {
                    changed = true;
                }
                else if (!enable && symbols.Remove(symbol))
                {
                    changed = true;
                }
                if (changed)
                {
                    string updatedDefines = string.Join(";", symbols);
                    PlayerSettings.SetScriptingDefineSymbols (target, updatedDefines);
                    Debug.Log($"Updated define symbols for {group}: {(enable ? "Added" : "Removed")} {symbol}");
                }
            }
            catch (ArgumentException)
            {
                // No build target, ignore.
            }
        }
        return;
        
        // Process all build target groups
        // foreach (NamedBuildTarget group in NamedBuildTarget.All)
        // {
        //     Debug.Log ("Setting define: " + group + " " + symbol + " " + enable);
        //     // if (group == BuildTargetGroup.Unknown) continue;
        //     string defines = PlayerSettings.GetScriptingDefineSymbols(group);
        //     var symbols = new System.Collections.Generic.HashSet<string>(defines.Split(';'));
        // 
        //     bool changed = false;
        //     if (enable && symbols.Add(symbol))
        //     {
        //         changed = true;
        //     }
        //     else if (!enable && symbols.Remove(symbol))
        //     {
        //         changed = true;
        //     }
        // 
        //     if (changed)
        //     {
        //         string updatedDefines = string.Join(";", symbols);
        //         PlayerSettings.SetScriptingDefineSymbols(group, updatedDefines);
        //         Debug.Log($"Updated define symbols for {group}: {(enable ? "Added" : "Removed")} {symbol}");
        //     }
        // }
    }
}
#endif
