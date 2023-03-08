using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class TextureToMaterialBatch : EditorWindow
{
    private string inputFolderPath = "Assets/Textures";
    private string outputFolderPath = "Assets/Materials";

    [MenuItem("Tools/Texture to Material Batch")]
    static void Init()
    {
        TextureToMaterialBatch window = (TextureToMaterialBatch)EditorWindow.GetWindow(typeof(TextureToMaterialBatch));
        window.Show();
    }
    void OnEnable()
    {
        inputFolderPath = EditorPrefs.GetString("TextureToMaterial_inputPath", "Assets/Textures");
        outputFolderPath = EditorPrefs.GetString("TextureToMaterial_outputPath", "Assets/Materials");
    }

    void OnGUI()
    {
        GUILayout.Label("Select Folders for Conversion", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox("Select a folder with textures to convert and a folder to save the materials to. The texture file names should end with \"_BaseColor\", \"_Normal\", or \"_MetallicSmooth\", or \"_Height\" to be recognized.", MessageType.Info);

        inputFolderPath = EditorGUILayout.TextField("Input Folder Path:", inputFolderPath);
        outputFolderPath = EditorGUILayout.TextField("Output Folder Path:", outputFolderPath);

        if (GUILayout.Button("Convert Textures to Materials"))
        {
            // 存储输入和输出路径
            EditorPrefs.SetString("TextureToMaterial_inputPath", inputFolderPath);
            EditorPrefs.SetString("TextureToMaterial_outputPath", outputFolderPath);

            Dictionary<string, Dictionary<string, Texture2D>> textureDictionary = new Dictionary<string, Dictionary<string, Texture2D>>();
            string[] textureFilePaths = Directory.GetFiles(inputFolderPath, "*.png", SearchOption.AllDirectories);

            foreach (string textureFilePath in textureFilePaths)
            {
                string textureFileName = Path.GetFileNameWithoutExtension(textureFilePath);

                if (textureFileName.EndsWith("_BaseColor"))
                {
                    string key = textureFileName.Replace("_BaseColor", "");
                    if (!textureDictionary.ContainsKey(key))
                    {
                        textureDictionary.Add(key, new Dictionary<string, Texture2D>());
                    }

                    textureDictionary[key]["_MainTex"] = AssetDatabase.LoadAssetAtPath<Texture2D>(textureFilePath);
                }
                else if (textureFileName.EndsWith("_Normal"))
                {
                    string key = textureFileName.Replace("_Normal", "");
                    if (!textureDictionary.ContainsKey(key))
                    {
                        textureDictionary.Add(key, new Dictionary<string, Texture2D>());
                    }

                    textureDictionary[key]["_BumpMap"] = AssetDatabase.LoadAssetAtPath<Texture2D>(textureFilePath);
                    textureDictionary[key]["_NORMALMAP"] = null;
                }
                else if (textureFileName.EndsWith("_MetallicSmooth"))
                {
                    string key = textureFileName.Replace("_MetallicSmooth", "");
                    if (!textureDictionary.ContainsKey(key))
                    {
                        textureDictionary.Add(key, new Dictionary<string, Texture2D>());
                    }

                    textureDictionary[key]["_MetallicGlossMap"] = AssetDatabase.LoadAssetAtPath<Texture2D>(textureFilePath);
                    textureDictionary[key]["_METALLICGLOSSMAP"] = null;
                }
                else if (textureFileName.EndsWith("_Height"))
                {
                    string key = textureFileName.Replace("_Height", "");
                    if (!textureDictionary.ContainsKey(key))
                    {
                        textureDictionary.Add(key, new Dictionary<string, Texture2D>());
                    }

                    textureDictionary[key]["_ParallaxMap"] = AssetDatabase.LoadAssetAtPath<Texture2D>(textureFilePath);
                }
            }

            foreach (KeyValuePair<string, Dictionary<string, Texture2D>> kvp in textureDictionary)
            {
                Material material = new Material(Shader.Find("Standard"));
                if (kvp.Value.ContainsKey("_MainTex"))
                {
                    material.SetTexture("_MainTex", kvp.Value["_MainTex"]);
                }
                if (kvp.Value.ContainsKey("_BumpMap"))
                {
                    material.SetTexture("_BumpMap", kvp.Value["_BumpMap"]);
                    material.EnableKeyword("_NORMALMAP");
                }

                if (kvp.Value.ContainsKey("_MetallicGlossMap"))
                {
                    material.SetTexture("_MetallicGlossMap", kvp.Value["_MetallicGlossMap"]);
                    material.EnableKeyword("_METALLICGLOSSMAP");
                }
                if (kvp.Value.ContainsKey("_ParallaxMap"))
                {
                    material.SetTexture("_ParallaxMap", kvp.Value["_ParallaxMap"]);
                }
                string materialPath = Path.Combine(outputFolderPath, kvp.Key + ".mat");
                AssetDatabase.CreateAsset(material, materialPath);
                Debug.Log("Material created at " + materialPath);

            }
        }
    }
}
