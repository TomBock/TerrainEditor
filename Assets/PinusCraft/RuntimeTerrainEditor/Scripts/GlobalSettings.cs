using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Threading;

namespace RuntimeTerrainEditor
{
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(GlobalSettings))]
    public class TestScriptableEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var script = (GlobalSettings)target;

            GUILayout.Space(20);
        
            if(GUILayout.Button("Create Object Thumbnails", GUILayout.Height(40)))
            {
                script.CreateThumbnails();
            }
        }
    }
#endif

    [CreateAssetMenu(menuName = "RuntimeTerrainEditor/Settings")]
    public class GlobalSettings : ScriptableObject
    {
        [Header("Size")]
        public int brushSizeMin;
        public int brushSizeMax;
        public int brushSizeDefault;
        
        [Header("Strength")]
        public float brushStrengthDefault;
        
        [Header("Flatten")]
        public float flattenHeightMin;
        public float flattenHeightMax;
        public float flattenHeightDefault;
        
        public TerrainLayer[] paintLayers;
        public Texture2D[] brushTextures;
        
        [Header("Objects")]
        public float randomObjectHeightMin;
        public float randomObjectHeightMax;
        public float randomObjectWidthMin;
        public float randomObjectWidthtMax;
        public GameObject[] objectPrefabs;
        

        [Header("Thumbnails")]

        public string saveFolderPath = "RuntimeTerrainEditor/Thumbnails/";
        public Texture2D[] objectThumbnails;

        public Texture2D GetThumbnailAtIndex(int index)
        {
            if (index < objectThumbnails.Length)
            {
                return objectThumbnails[index];
            }

            return null;
        }

#if UNITY_EDITOR
        public void CreateThumbnails()
        {
            var folderPath = Application.dataPath + "/" + saveFolderPath;
            if (Directory.Exists(folderPath) == false)
            {
                Directory.CreateDirectory(folderPath);
            }

            var tempPaths = new List<string>();
            for (int i = 0; i < objectPrefabs.Length; i++)
            {
                var preview = AssetPreview.GetAssetPreview(objectPrefabs[i]);
                while (preview == null)
                {
                    preview = AssetPreview.GetAssetPreview(objectPrefabs[i]);
                    Thread.Sleep(100);
                }

                var assetPath = saveFolderPath + i + "."+ objectPrefabs[i].name + ".png";
                tempPaths.Add(assetPath);
                
                SaveToFile(preview, Application.dataPath + "/" + assetPath);
            }

            AssetDatabase.Refresh();

            objectThumbnails = new Texture2D[objectPrefabs.Length];
            for (int i = 0; i < tempPaths.Count; i++)
            {
                var tex = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/" + tempPaths[i], typeof(Texture2D));
                objectThumbnails[i] = tex;
            }
        }

        private void SaveToFile(Texture2D t, string path)
        {
            t.Apply();
            File.WriteAllBytes(path, ImageConversion.EncodeToPNG(t));
        }
#endif
    }
}