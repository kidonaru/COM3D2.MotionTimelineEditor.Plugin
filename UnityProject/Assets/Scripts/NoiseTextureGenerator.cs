#if UNITY_EDITOR
using UnityEngine;
using System.IO;
using UnityEditor;

public class NoiseTextureGenerator : EditorWindow
{
    private int textureSize = 512;
    private float scale = 50f;
    private int octaves = 8;
    private float persistence = 0.5f;
    private float lacunarity = 2f;
    private bool seamless = true;
    private string fileName = "Resources/noise_texture";

    // プリセット用の列挙型
    private enum NoisePreset
    {
        カスタム,
        デフォルト,
        自然な地形,
        雲模様,
        柔らかい光,
        ほこりっぽい光,
        霧の中の光
    }
    private NoisePreset currentPreset = NoisePreset.カスタム;


    [MenuItem("Tools/Noise Texture Generator")]
    static void Init()
    {
        NoiseTextureGenerator window = (NoiseTextureGenerator)EditorWindow.GetWindow(typeof(NoiseTextureGenerator));
        window.Show();
    }

    // プリセットの設定を定義
    private void ApplyPreset(NoisePreset preset)
    {
        switch (preset)
        {
            case NoisePreset.デフォルト:
                textureSize = 512;
                scale = 50f;
                octaves = 8;
                persistence = 0.5f;
                lacunarity = 2.0f;
                seamless = true;
                break;

            case NoisePreset.自然な地形:
                textureSize = 512;
                scale = 100f;
                octaves = 6;
                persistence = 0.5f;
                lacunarity = 2.0f;
                seamless = true;
                break;

            case NoisePreset.雲模様:
                textureSize = 512;
                scale = 150f;
                octaves = 8;
                persistence = 0.7f;
                lacunarity = 2.5f;
                seamless = true;
                break;

            case NoisePreset.柔らかい光:
                textureSize = 512;
                scale = 250f;
                octaves = 4;
                persistence = 0.3f;
                lacunarity = 1.8f;
                seamless = true;
                break;

            case NoisePreset.ほこりっぽい光:
                textureSize = 512;
                scale = 180f;
                octaves = 6;
                persistence = 0.4f;
                lacunarity = 2.2f;
                seamless = true;
                break;

            case NoisePreset.霧の中の光:
                textureSize = 512;
                scale = 300f;
                octaves = 3;
                persistence = 0.25f;
                lacunarity = 1.5f;
                seamless = true;
                break;
        }
    }

    void OnGUI()
    {
        GUILayout.Label("Noise Texture Generator", EditorStyles.boldLabel);
        
        // プリセット選択用のドロップダウン
        EditorGUI.BeginChangeCheck();
        currentPreset = (NoisePreset)EditorGUILayout.EnumPopup("Preset", currentPreset);
        if (EditorGUI.EndChangeCheck())
        {
            ApplyPreset(currentPreset);
        }

        textureSize = EditorGUILayout.IntField("Texture Size", textureSize);
        scale = EditorGUILayout.FloatField("Scale", scale);
        octaves = EditorGUILayout.IntField("Octaves", octaves);
        persistence = EditorGUILayout.FloatField("Persistence", persistence);
        lacunarity = EditorGUILayout.FloatField("Lacunarity", lacunarity);
        seamless = EditorGUILayout.Toggle("Seamless", seamless);
        fileName = EditorGUILayout.TextField("File Name", fileName);

        if (GUILayout.Button("Generate Texture"))
        {
            GenerateNoiseTexture();
        }
    }

    void GenerateNoiseTexture()
    {
        Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        Color[] pix = new Color[textureSize * textureSize];
        
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;
        
        // ノイズ値の生成
        float[,] noiseMap = new float[textureSize, textureSize];
        
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float fBm = 0;
                
                for (int i = 0; i < octaves; i++)
                {
                    if (seamless)
                    {
                        float sampleX = x / scale * frequency;
                        float sampleY = y / scale * frequency;
                        float w = textureSize / scale * frequency;

                        float u = sampleX / w;
                        float v = sampleY / w;

                        float perlinValue = 
                            Mathf.PerlinNoise(sampleX, sampleY) * u * v
                            + Mathf.PerlinNoise(sampleX, sampleY + w) * u * (1.0f - v)
                            + Mathf.PerlinNoise(sampleX + w, sampleY) * (1.0f - u) * v
                            + Mathf.PerlinNoise(sampleX + w, sampleY + w) * (1.0f - u) * (1.0f - v);

                        fBm += perlinValue * amplitude;
                    }
                    else
                    {
                        float sampleX = x / scale * frequency;
                        float sampleY = y / scale * frequency;
                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                        fBm += perlinValue * amplitude;
                    }
                    
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }
                
                noiseMap[x, y] = fBm;
                maxNoiseHeight = Mathf.Max(maxNoiseHeight, fBm);
                minNoiseHeight = Mathf.Min(minNoiseHeight, fBm);
            }
        }
        
        // ノイズ値の正規化とテクスチャへの適用
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float normalizedHeight = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
                pix[y * textureSize + x] = new Color(normalizedHeight, normalizedHeight, normalizedHeight, 1);
            }
        }
        
        texture.SetPixels(pix);
        texture.Apply();
        
        // テクスチャの保存
        byte[] bytes = texture.EncodeToPNG();
        string path = Path.Combine(Application.dataPath, fileName + ".png");
        File.WriteAllBytes(path, bytes);
        AssetDatabase.Refresh();
        
        // テクスチャ設定の調整
        string assetPath = "Assets/" + fileName + ".png";
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Default;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();
        }
        
        Debug.Log("Texture saved to: " + path);
    }
}
#endif
