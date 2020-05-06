using System.Collections.Generic;
using System.IO;
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

        private ReorderableList m_FilterList;
        private Vector2 m_ScrollPosition = Vector2.zero;

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

            if (m_FilterList == null)
            {
                InitFilterListDrawer();
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
                    m_FilterList.DoLayoutList();
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

        private void InitFilterListDrawer()
        {
            m_FilterList = new ReorderableList(m_Configuration.filters, typeof(AssetBundleFilter));
            m_FilterList.drawElementCallback = OnListElementGUI;
            m_FilterList.drawHeaderCallback = OnListHeaderGUI;
            m_FilterList.draggable = true;
            m_FilterList.elementHeight = 22;
            m_FilterList.onAddCallback = (list) => Add();
        }

        private void Add()
        {
            string path = SelectFolder();
            if (!string.IsNullOrEmpty(path))
            {
                var filter = new AssetBundleFilter();
                filter.assetsDirectoryPath = path;
                m_Configuration.filters.Add(filter);
            }
        }

        private void OnListElementGUI(Rect rect, int index, bool isactive, bool isfocused)
        {
            const float GAP = 5;

            AssetBundleFilter filter = m_Configuration.filters[index];
            rect.y++;

            Rect r = rect;
            r.width = 16;
            r.height = 18;
            filter.valid = EditorGUI.Toggle(r, filter.valid);

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMax + 425;
            float assetBundleNameLength = r.width;
            filter.assetBundleName = EditorGUI.TextField(r, filter.assetBundleName);

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMin + 100;
            filter.assetBundleLoadType = (AssetBundleLoadType) EditorGUI.EnumPopup(r, filter.assetBundleLoadType);

            r.xMin = r.xMax + GAP + 15;
            r.xMax = r.xMin + 30;
            filter.packed = EditorGUI.Toggle(r, filter.packed);

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMin + 85;
            filter.assetBundleGroups = EditorGUI.TextField(r, filter.assetBundleGroups);

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMin + 85;
            filter.assetBundleVariant = EditorGUI.TextField(r, filter.assetBundleVariant);

            r.xMin = r.xMax + GAP;
            r.width = assetBundleNameLength - 15;
            GUI.enabled = false;
            filter.assetsDirectoryPath = EditorGUI.TextField(r, filter.assetsDirectoryPath);
            GUI.enabled = true;

            r.xMin = r.xMax + GAP;
            r.width = 50;
            if (GUI.Button(r, "Select"))
            {
                var path = SelectFolder();
                if (path != null)
                    filter.assetsDirectoryPath = path;
            }

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMin + 85;
            filter.filterType = (AssetBundleFilterType) EditorGUI.EnumPopup(r, filter.filterType);

            r.xMin = r.xMax + GAP;
            r.xMax = rect.xMax;
            filter.filter = EditorGUI.TextField(r, filter.filter);
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

            foreach (AssetBundleFilter assetBundleFilter in m_Configuration.filters)
            {
                if (assetBundleFilter.assetBundleVariant == "")
                    assetBundleFilter.assetBundleVariant = null;

                if (assetBundleFilter.valid)
                {
                    switch (assetBundleFilter.filterType)
                    {
                        case AssetBundleFilterType.Root:
                        {
                            if (string.IsNullOrEmpty(assetBundleFilter.assetBundleName))
                            {
                                string relativeDirectoryName =
                                    assetBundleFilter.assetsDirectoryPath.Replace("Assets/","");
                                ApplyAssetBundleFilter(ref signedAssetBundleList, assetBundleFilter,
                                   Utility.Path.GetRegularPath(relativeDirectoryName));
                            }
                            else
                            {
                                ApplyAssetBundleFilter(ref signedAssetBundleList, assetBundleFilter,
                                    assetBundleFilter.assetBundleName);
                            }
                          
                        }
                            break;

                        case AssetBundleFilterType.Children:
                        {
                            string[] patterns = assetBundleFilter.filter.Split(';', ',', '|');
                            for (int i = 0; i < patterns.Length; i++)
                            {
                                FileInfo[] assetFiles =
                                    new DirectoryInfo(assetBundleFilter.assetsDirectoryPath).GetFiles(patterns[i],
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

                                    ApplyAssetBundleFilter(ref signedAssetBundleList, assetBundleFilter,
                                        relativeAssetNameWithoutExtension, assetGUID);
                                }
                            }
                        }
                            break;

                        case AssetBundleFilterType.ChildrenFoldersOnly:
                        {
                            DirectoryInfo[] assetDirectories =
                                new DirectoryInfo(assetBundleFilter.assetsDirectoryPath).GetDirectories();
                            foreach (DirectoryInfo directory in assetDirectories)
                            {
                                string relativeDirectoryName =
                                    directory.FullName.Substring(Application.dataPath.Length + 1);

                                ApplyAssetBundleFilter(ref signedAssetBundleList, assetBundleFilter,
                                    Utility.Path.GetRegularPath(relativeDirectoryName), string.Empty,
                                    directory.FullName);
                            }
                        }
                            break;

                        case AssetBundleFilterType.ChildrenFilesOnly:
                        {
                            DirectoryInfo[] assetDirectories =
                                new DirectoryInfo(assetBundleFilter.assetsDirectoryPath).GetDirectories();
                            foreach (DirectoryInfo directory in assetDirectories)
                            {
                                string[] patterns = assetBundleFilter.filter.Split(';', ',', '|');
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

                                        ApplyAssetBundleFilter(ref signedAssetBundleList, assetBundleFilter,
                                            relativeAssetNameWithoutExtension, assetGUID);
                                    }
                                }
                            }
                        }
                            break;
                    }
                }
            }
        }

        private void ApplyAssetBundleFilter(ref List<string> signedAssetBundleList, AssetBundleFilter assetBundleFilter,
            string assetBundleName, string singleAssetGUID = "", string childDirectoryPath = "")
        {
            if (!signedAssetBundleList.Contains(Path.Combine(assetBundleFilter.assetsDirectoryPath, assetBundleName)))
            {
                signedAssetBundleList.Add(Path.Combine(assetBundleFilter.assetsDirectoryPath, assetBundleName));

                foreach (GFAssetBundle oldAssetBundle in GetAssetBundles())
                {
                    if (oldAssetBundle.Name == assetBundleName)
                    {
                        RenameAssetBundle(oldAssetBundle.Name, oldAssetBundle.Variant,
                            assetBundleName, assetBundleFilter.assetBundleVariant);
                        break;
                    }
                }

                if (!HasAssetBundle(assetBundleName, assetBundleFilter.assetBundleVariant))
                {
                    AddAssetBundle(assetBundleName, assetBundleFilter.assetBundleVariant,
                        assetBundleFilter.assetBundleLoadType, assetBundleFilter.packed,
                        assetBundleFilter.assetBundleGroups.Split(';', ',', '|'));
                }

                switch (assetBundleFilter.filterType)
                {
                    case AssetBundleFilterType.Root:
                    case AssetBundleFilterType.ChildrenFoldersOnly:
                        string[] patterns = assetBundleFilter.filter.Split(';', ',', '|');
                        if (childDirectoryPath == "")
                        {
                            childDirectoryPath = assetBundleFilter.assetsDirectoryPath;
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
                                AssignAsset(assetGUID, assetBundleName,
                                    assetBundleFilter.assetBundleVariant);
                            }
                        }

                        break;

                    case AssetBundleFilterType.Children:
                    case AssetBundleFilterType.ChildrenFilesOnly:
                    {
                        AssignAsset(singleAssetGUID, assetBundleName,
                            assetBundleFilter.assetBundleVariant);
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