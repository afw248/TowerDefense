#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class AudioImportOptimization
{
    private const string SfxFolder = "Assets/Audio/SFX";
    private const string BgmFolder = "Assets/Audio/BGM";

    [MenuItem("TowerDefense/Optimize Audio Import Settings")]
    public static void OptimizeFromMenu()
    {
        int count = OptimizeAll();
        Debug.Log($"오디오 import 설정 최적화 완료 ({count}개).");
    }

    public static int OptimizeAll()
    {
        int count = 0;
        count += ApplySfxSettings(SfxFolder);
        count += ApplyBgmSettings(BgmFolder);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return count;
    }

    private static int ApplySfxSettings(string folder)
    {
        int count = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:AudioClip", new[] { folder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (importer == null)
                continue;

            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = 0.35f;
            importer.forceToMono = true;
            importer.defaultSampleSettings = settings;
            importer.SaveAndReimport();
            count++;
        }

        return count;
    }

    private static int ApplyBgmSettings(string folder)
    {
        int count = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:AudioClip", new[] { folder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (importer == null)
                continue;

            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            settings.loadType = AudioClipLoadType.Streaming;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = 0.45f;
            importer.forceToMono = false;
            importer.defaultSampleSettings = settings;
            importer.SaveAndReimport();
            count++;
        }

        return count;
    }
}
#endif
