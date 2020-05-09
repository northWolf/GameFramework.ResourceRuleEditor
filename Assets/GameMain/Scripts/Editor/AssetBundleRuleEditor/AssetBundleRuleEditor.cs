using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameFramework;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityGameFramework.Editor.AssetBundleTools;
using GFAssetBundle = UnityGameFramework.Editor.AssetBundleTools.AssetBundle;

namespace StarForce.Editor.AssetBundleTools
{
    /// <summary>
    /// AssetBundle 规则编辑器，支持按规则配置自动生成 AssetBundleCollection.xml
    /// </summary>
    public class AssetBundleRuleEditor : EditorWindow
    {
        private readonly string m_ConfigurationPath = "Assets/GameMain/Configs/AssetBundleRuleEditor.asset";
        private AssetBundleRuleEditorData m_Configuration;
        private AssetBundleCollection m_AssetBundleCollection;
        
        private ReorderableList m_RuleList;
        private Vector2 m_ScrollPosition = Vector2.zero;
        
        private string m_SourceAssetExceptTypeFilter = "t:Script";
        private string[] m_SourceAssetExceptTypeFilterGUIDArray;
        
        private string m_SourceAssetExceptLabelFilter = "l:AssetBundleExclusive";
        private string[] m_SourceAssetExceptLabelFilterGUIDArray;
        
        [MenuItem("Game Framework/AssetBundle Tools/AssetBundle Rule Editor", false, 50)]
        static void Open()
        {
            AssetBundleRuleEditor window = GetWindow<AssetBundleRuleEditor>(true, "AssetBundle Rule Editor", true);
            window.minSize = new Vector2(1470f, 420f);
        }

        private void OnGUI()
        {
            if (m_Configuration == null)
            {
                Load();
            }

            if (m_RuleList == null)
            {
                InitRuleListDrawer();
            }

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                if (GUILayout.Button("Add", EditorStyles.toolbarButton))
                {
                    Add();
                }

                if (GUILayout.Button("Save", EditorStyles.toolbarButton))
                {
                    Save();
                }

                if (GUILayout.Button("Refresh AssetBundleCollection.xml", EditorStyles.toolbarButton))
                {
                    RefreshAssetBundleCollection();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(10);
                OnListElementLabelGUI();
            }
            GUILayout.EndHorizontal();


            GUILayout.BeginVertical();
            {
                GUILayout.Space(30);

                m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition);
                {
                    m_RuleList.DoLayoutList();
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();

            if (GUI.changed)
                EditorUtility.SetDirty(m_Configuration);
        }

        private void Load()
        {
            m_Configuration = LoadAssetAtPath<AssetBundleRuleEditorData>(m_ConfigurationPath);
            if (m_Configuration == null)
            {
                m_Configuration = ScriptableObject.CreateInstance<AssetBundleRuleEditorData>();
            }
        }

        private T LoadAssetAtPath<T>(string path) where T : Object
        {
#if UNITY_5
            return AssetDatabase.LoadAssetAtPath<T>(path);
#else
            return (T) AssetDatabase.LoadAssetAtPath(path, typeof(T));
#endif
        }

        private void InitRuleListDrawer()
        {
            m_RuleList = new ReorderableList(m_Configuration.rules, typeof(AssetBundleRule));
            m_RuleList.drawElementCallback = OnListElementGUI;
            m_RuleList.drawHeaderCallback = OnListHeaderGUI;
            m_RuleList.draggable = true;
            m_RuleList.elementHeight = 22;
            m_RuleList.onAddCallback = (list) => Add();
        }

        private void Add()
        {
            string path = SelectFolder();
            if (!string.IsNullOrEmpty(path))
            {
                var rule = new AssetBundleRule();
                rule.assetsDirectoryPath = path;
                m_Configuration.rules.Add(rule);
            }
        }

        private void OnListElementGUI(Rect rect, int index, bool isactive, bool isfocused)
        {
            const float GAP = 5;

            AssetBundleRule rule = m_Configuration.rules[index];
            rect.y++;

            Rect r = rect;
            r.width = 16;
            r.height = 18;
            rule.valid = EditorGUI.Toggle(r, rule.valid);

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMax + 425;
            float assetBundleNameLength = r.width;
            rule.assetBundleName = EditorGUI.TextField(r, rule.assetBundleName);

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMin + 100;
            rule.assetBundleLoadType = (AssetBundleLoadType) EditorGUI.EnumPopup(r, rule.assetBundleLoadType);

            r.xMin = r.xMax + GAP + 15;
            r.xMax = r.xMin + 30;
            rule.packed = EditorGUI.Toggle(r, rule.packed);

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMin + 85;
            rule.assetBundleGroups = EditorGUI.TextField(r, rule.assetBundleGroups);

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMin + 85;
            rule.assetBundleVariant = EditorGUI.TextField(r, rule.assetBundleVariant);

            r.xMin = r.xMax + GAP;
            r.width = assetBundleNameLength - 15;
            GUI.enabled = false;
            rule.assetsDirectoryPath = EditorGUI.TextField(r, rule.assetsDirectoryPath);
            GUI.enabled = true;

            r.xMin = r.xMax + GAP;
            r.width = 50;
            if (GUI.Button(r, "Select"))
            {
                var path = SelectFolder();
                if (path != null)
                    rule.assetsDirectoryPath = path;
            }

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMin + 85;
            rule.filterType = (AssetBundleFilterType) EditorGUI.EnumPopup(r, rule.filterType);

            r.xMin = r.xMax + GAP;
            r.xMax = rect.xMax;
            rule.searchPatterns = EditorGUI.TextField(r, rule.searchPatterns);
        }

        private string SelectFolder()
        {
            string dataPath = Application.dataPath;
            string selectedPath = EditorUtility.OpenFolderPanel("Path", dataPath, "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                if (selectedPath.StartsWith(dataPath))
                {
                    return "Assets/" + selectedPath.Substring(dataPath.Length + 1);
                }
                else
                {
                    ShowNotification(new GUIContent("Can not be outside of 'Assets/'!"), 2);
                }
            }

            return null;
        }

        private void OnListHeaderGUI(Rect rect)
        {
            EditorGUI.LabelField(rect, "Rules");
        }

        private void OnListElementLabelGUI()
        {
            Rect rect = new Rect();
            const float GAP = 5;
            GUI.enabled = false;

            Rect r = new Rect(0, 20, rect.width, rect.height);
            r.width = 45;
            r.height = 18;
            EditorGUI.TextField(r, "Active");

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMax + 415;
            float assetBundleNameLength = r.width;
            EditorGUI.TextField(r, "AssetBundleName");

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMin + 100;
            EditorGUI.TextField(r, "Load Type");

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMin + 50;
            EditorGUI.TextField(r, "Packed");

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMin + 85;
            EditorGUI.TextField(r, "Groups");

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMin + 85;
            EditorGUI.TextField(r, "Variant");

            r.xMin = r.xMax + GAP;
            r.width = assetBundleNameLength + 50;
            EditorGUI.TextField(r, "AssetDirectory");

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMin + 85;
            EditorGUI.TextField(r, "Filter Type");

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMin + 250;
            EditorGUI.TextField(r, "Patterns");
            GUI.enabled = true;
        }

        private void Save()
        {
            if (LoadAssetAtPath<AssetBundleRuleEditorData>(m_ConfigurationPath) == null)
            {
                AssetDatabase.CreateAsset(m_Configuration, m_ConfigurationPath);
            }
            else
            {
                EditorUtility.SetDirty(m_Configuration);
            }
        }

        #region Refresh AssetBundleCollection.xml

        public void RefreshAssetBundleCollection()
        {
            if (m_Configuration == null)
            {
                Load();
            }
            m_SourceAssetExceptTypeFilterGUIDArray = AssetDatabase.FindAssets(m_SourceAssetExceptTypeFilter);
            m_SourceAssetExceptLabelFilterGUIDArray = AssetDatabase.FindAssets(m_SourceAssetExceptLabelFilter);
            AnalysisAssetBundleFilters();
            if (SaveCollection())
            {
                Debug.Log("Refresh AssetBundleCollection.xml success");
            }
            else
            {
                Debug.Log("Refresh AssetBundleCollection.xml fail");
            }
        }

        private GFAssetBundle[] GetAssetBundles()
        {
            return m_AssetBundleCollection.GetAssetBundles();
        }

        private bool HasAssetBundle(string assetBundleName, string assetBundleVariant)
        {
            return m_AssetBundleCollection.HasAssetBundle(assetBundleName, assetBundleVariant);
        }

        private bool AddAssetBundle(string assetBundleName, string assetBundleVariant,
            AssetBundleLoadType assetBundleLoadType, bool assetBundlePacked, string[] assetBundleResourceGroups)
        {
            return m_AssetBundleCollection.AddAssetBundle(assetBundleName, assetBundleVariant, assetBundleLoadType,
                assetBundlePacked, assetBundleResourceGroups);
        }

        private bool RenameAssetBundle(string oldAssetBundleName, string oldAssetBundleVariant,
            string newAssetBundleName, string newAssetBundleVariant)
        {
            return m_AssetBundleCollection.RenameAssetBundle(oldAssetBundleName, oldAssetBundleVariant,
                newAssetBundleName, newAssetBundleVariant);
        }

        private bool AssignAsset(string assetGuid, string assetBundleName, string assetBundleVariant)
        {
            if (m_AssetBundleCollection.AssignAsset(assetGuid, assetBundleName, assetBundleVariant))
            {
                return true;
            }

            return false;
        }

        private void AnalysisAssetBundleFilters()
        {
            m_AssetBundleCollection = new AssetBundleCollection();
            List<string> signedAssetBundleList = new List<string>();

            foreach (AssetBundleRule assetBundleRule in m_Configuration.rules)
            {
                if (assetBundleRule.assetBundleVariant == "")
                    assetBundleRule.assetBundleVariant = null;

                if (assetBundleRule.valid)
                {
                    switch (assetBundleRule.filterType)
                    {
                        case AssetBundleFilterType.Root:
                        {
                            if (string.IsNullOrEmpty(assetBundleRule.assetBundleName))
                            {
                                string relativeDirectoryName =
                                    assetBundleRule.assetsDirectoryPath.Replace("Assets/", "");
                                ApplyAssetBundleFilter(ref signedAssetBundleList, assetBundleRule,
                                    Utility.Path.GetRegularPath(relativeDirectoryName));
                            }
                            else
                            {
                                ApplyAssetBundleFilter(ref signedAssetBundleList, assetBundleRule,
                                    assetBundleRule.assetBundleName);
                            }
                        }
                            break;

                        case AssetBundleFilterType.Children:
                        {
                            string[] patterns = assetBundleRule.searchPatterns.Split(';', ',', '|');
                            for (int i = 0; i < patterns.Length; i++)
                            {
                                FileInfo[] assetFiles =
                                    new DirectoryInfo(assetBundleRule.assetsDirectoryPath).GetFiles(patterns[i],
                                        SearchOption.AllDirectories);
                                foreach (FileInfo file in assetFiles)
                                {
                                    if (file.Extension.Contains("meta"))
                                        continue;

                                    string relativeAssetName = file.FullName.Substring(Application.dataPath.Length + 1);
                                    string relativeAssetNameWithoutExtension =
                                        Utility.Path.GetRegularPath(
                                            relativeAssetName.Substring(0, relativeAssetName.IndexOf('.')));

                                    string assetName = Path.Combine("Assets", relativeAssetName);
                                    string assetGUID = AssetDatabase.AssetPathToGUID(assetName);

                                    if (!m_SourceAssetExceptTypeFilterGUIDArray.Contains(assetGUID) && !m_SourceAssetExceptLabelFilterGUIDArray.Contains(assetGUID))
                                    {
                                        ApplyAssetBundleFilter(ref signedAssetBundleList, assetBundleRule,
                                            relativeAssetNameWithoutExtension, assetGUID);
                                    }
                                }
                            }
                        }
                            break;

                        case AssetBundleFilterType.ChildrenFoldersOnly:
                        {
                            DirectoryInfo[] assetDirectories =
                                new DirectoryInfo(assetBundleRule.assetsDirectoryPath).GetDirectories();
                            foreach (DirectoryInfo directory in assetDirectories)
                            {
                                string relativeDirectoryName =
                                    directory.FullName.Substring(Application.dataPath.Length + 1);

                                ApplyAssetBundleFilter(ref signedAssetBundleList, assetBundleRule,
                                    Utility.Path.GetRegularPath(relativeDirectoryName), string.Empty,
                                    directory.FullName);
                            }
                        }
                            break;

                        case AssetBundleFilterType.ChildrenFilesOnly:
                        {
                            DirectoryInfo[] assetDirectories =
                                new DirectoryInfo(assetBundleRule.assetsDirectoryPath).GetDirectories();
                            foreach (DirectoryInfo directory in assetDirectories)
                            {
                                string[] patterns = assetBundleRule.searchPatterns.Split(';', ',', '|');
                                for (int i = 0; i < patterns.Length; i++)
                                {
                                    FileInfo[] assetFiles =
                                        new DirectoryInfo(directory.FullName).GetFiles(patterns[i],
                                            SearchOption.AllDirectories);
                                    foreach (FileInfo file in assetFiles)
                                    {
                                        if (file.Extension.Contains("meta"))
                                            continue;

                                        string relativeAssetName =
                                            file.FullName.Substring(Application.dataPath.Length + 1);
                                        string relativeAssetNameWithoutExtension =
                                            Utility.Path.GetRegularPath(
                                                relativeAssetName.Substring(0, relativeAssetName.IndexOf('.')));

                                        string assetName = Path.Combine("Assets", relativeAssetName);
                                        string assetGUID = AssetDatabase.AssetPathToGUID(assetName);

                                        if (!m_SourceAssetExceptTypeFilterGUIDArray.Contains(assetGUID) && !m_SourceAssetExceptLabelFilterGUIDArray.Contains(assetGUID))
                                        {
                                            ApplyAssetBundleFilter(ref signedAssetBundleList, assetBundleRule,
                                                relativeAssetNameWithoutExtension, assetGUID);
                                        }
                                    }
                                }
                            }
                        }
                            break;
                    }
                }
            }
        }

        private void ApplyAssetBundleFilter(ref List<string> signedAssetBundleList, AssetBundleRule assetBundleRule,
            string assetBundleName, string singleAssetGUID = "", string childDirectoryPath = "")
        {
            if (!signedAssetBundleList.Contains(Path.Combine(assetBundleRule.assetsDirectoryPath, assetBundleName)))
            {
                signedAssetBundleList.Add(Path.Combine(assetBundleRule.assetsDirectoryPath, assetBundleName));

                foreach (GFAssetBundle oldAssetBundle in GetAssetBundles())
                {
                    if (oldAssetBundle.Name == assetBundleName)
                    {
                        RenameAssetBundle(oldAssetBundle.Name, oldAssetBundle.Variant,
                            assetBundleName, assetBundleRule.assetBundleVariant);
                        break;
                    }
                }

                if (!HasAssetBundle(assetBundleName, assetBundleRule.assetBundleVariant))
                {
                    AddAssetBundle(assetBundleName, assetBundleRule.assetBundleVariant,
                        assetBundleRule.assetBundleLoadType, assetBundleRule.packed,
                        assetBundleRule.assetBundleGroups.Split(';', ',', '|'));
                }

                switch (assetBundleRule.filterType)
                {
                    case AssetBundleFilterType.Root:
                    case AssetBundleFilterType.ChildrenFoldersOnly:
                        string[] patterns = assetBundleRule.searchPatterns.Split(';', ',', '|');
                        if (childDirectoryPath == "")
                        {
                            childDirectoryPath = assetBundleRule.assetsDirectoryPath;
                        }

                        for (int i = 0; i < patterns.Length; i++)
                        {
                            FileInfo[] assetFiles =
                                new DirectoryInfo(childDirectoryPath).GetFiles(patterns[i],
                                    SearchOption.AllDirectories);
                            foreach (FileInfo file in assetFiles)
                            {
                                if (file.Extension.Contains("meta"))
                                    continue;

                                string assetName = Path.Combine("Assets",
                                    file.FullName.Substring(Application.dataPath.Length + 1));

                                string assetGUID = AssetDatabase.AssetPathToGUID(assetName);

                                if (!m_SourceAssetExceptTypeFilterGUIDArray.Contains(assetGUID) && !m_SourceAssetExceptLabelFilterGUIDArray.Contains(assetGUID))
                                {
                                    AssignAsset(assetGUID, assetBundleName,
                                        assetBundleRule.assetBundleVariant);
                                }
                            }
                        }

                        break;

                    case AssetBundleFilterType.Children:
                    case AssetBundleFilterType.ChildrenFilesOnly:
                    {
                        AssignAsset(singleAssetGUID, assetBundleName,
                                assetBundleRule.assetBundleVariant);
                    }
                        break;
                }
            }
        }

        private bool SaveCollection()
        {
            return m_AssetBundleCollection.Save();
        }

        #endregion
    }
}