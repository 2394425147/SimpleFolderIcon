using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SimpleFolderIcon.Editor
{
    public sealed class IconDictionaryCreator : AssetPostprocessor
    {
        [MenuItem("Tools/SimpleFolderIcon/Rebuild Icon Dictionary")]
        public static void RebuildDictionaryFromMenu() => BuildDictionary();

        private const           string                      AssetsPath     = "SimpleFolderIcon/Icons";
        private static readonly Dictionary<string, Texture> IconDictionary = new();

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            BuildDictionary();
            EditorApplication.projectWindowItemOnGUI -= DrawFolderIcon;
            EditorApplication.projectWindowItemOnGUI += DrawFolderIcon;
        }

        private static void BuildDictionary()
        {
            IconDictionary.Clear();

            var files = Directory.GetFiles(Path.Combine(Application.dataPath, AssetsPath), "*.png");
            foreach (var path in files)
            {
                var texture =
                    AssetDatabase.LoadAssetAtPath<Texture2D>(
                        Path.GetRelativePath(Path.GetDirectoryName(Application.dataPath), path));
                IconDictionary.Add(Path.GetFileNameWithoutExtension(path), texture);
            }
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
                                                   string[] movedFromAssetPaths)
        {
            foreach (var str in importedAssets)
                AppendIcons(str);

            foreach (var str in deletedAssets)
                RemoveIcons(str);

            foreach (var str in movedFromAssetPaths)
                RemoveIcons(str);

            foreach (var str in movedAssets)
                AppendIcons(str);
        }

        private static void AppendIcons(string assetPath)
        {
            if (Path.GetExtension(assetPath) != ".png"                                                       ||
                assetPath.StartsWith(Path.Combine("Assets", AssetsPath), StringComparison.OrdinalIgnoreCase) ||
                IconDictionary.ContainsKey(Path.GetFileNameWithoutExtension(assetPath)))
                return;

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            IconDictionary.Add(Path.GetFileNameWithoutExtension(assetPath), texture);
        }

        private static void RemoveIcons(string assetPath)
        {
            if (Path.GetExtension(assetPath) != ".png"                                                       ||
                assetPath.StartsWith(Path.Combine("Assets", AssetsPath), StringComparison.OrdinalIgnoreCase) ||
                !IconDictionary.ContainsKey(Path.GetFileNameWithoutExtension(assetPath)))
                return;
            IconDictionary.Remove(Path.GetFileNameWithoutExtension(assetPath));
        }

        private static void DrawFolderIcon(string guid, Rect rect)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            var path = AssetDatabase.GUIDToAssetPath(guid);

            if (string.IsNullOrEmpty(path)                                           ||
                !IconDictionary.TryGetValue(Path.GetFileName(path), out var texture) ||
                (File.GetAttributes(path) & FileAttributes.Directory) == 0)
            {
                return;
            }

            Rect imageRect;

            if (rect.height > 20) // Project grid view size
                imageRect = new Rect(rect.x - 1, rect.y - 1, rect.width + 2, rect.width + 2);
            else if (rect.x > 20) // Project navigation list view size
                imageRect = new Rect(rect.x - 1, rect.y - 1, rect.height + 2, rect.height + 2);
            else // Project list view size
                imageRect = new Rect(rect.x + 2, rect.y - 1, rect.height + 2, rect.height + 2);

            GUI.DrawTexture(imageRect, texture);
        }
    }
}
