using System;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using UnityEngine;

public static class ManagePackageDependencies
{
  [MenuItem("VersatileController/Import Required Packages")]
  static void ImportPackageExample()
  {
    var texturePackageNames = new[] {"BundledAssets/PUN 2 - FREE.unitypackage"};
    foreach (var package in texturePackageNames)
    {
      AssetDatabase.ImportPackage(package, false);
    }
    Debug.Log ("Photon Package installed");
    
    FileUtil.ReplaceFile ("BundledAssets/PhotonServerSettings.asset", "Assets/Photon/PhotonUnityNetworking/Resources/PhotonServerSettings.asset");
    Debug.Log ("Photon settings updated");    
  }
}
