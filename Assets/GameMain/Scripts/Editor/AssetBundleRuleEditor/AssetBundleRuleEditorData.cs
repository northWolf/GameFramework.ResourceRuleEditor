using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityGameFramework.Editor.AssetBundleTools;

namespace StarForce.Editor.AssetBundleTools
{
    public class AssetBundleRuleEditorData : ScriptableObject
    {
        public List<AssetBundleRule> rules = new List<AssetBundleRule>();
    }

    [System.Serializable]
    public class AssetBundleRule
    {
        public bool valid = true;
        public string assetBundleName = string.Empty;
        public string assetBundleVariant = null;
        public string assetBundleGroups = string.Empty;
        public string assetsDirectoryPath = string.Empty;
        public AssetBundleLoadType assetBundleLoadType = AssetBundleLoadType.LoadFromFile;
        public bool packed = true;
        public AssetBundleFilterType filterType = AssetBundleFilterType.Root;
        public string searchPatterns = "*.*";
    }

    public enum AssetBundleFilterType
    {
        Root,
        Children,
        ChildrenFoldersOnly,
        ChildrenFilesOnly,
    }
}