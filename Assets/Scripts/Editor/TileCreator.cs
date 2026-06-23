using UnityEngine;
using UnityEditor;
using System.IO;

public class TileCreator : MonoBehaviour
{
    [MenuItem("NoTrollHere/Create Ground Sprites")]
    static void TaoGroundSprites()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Sprites/Tiles"))
            AssetDatabase.CreateFolder("Assets/Sprites", "Tiles");

        // Tile đặc không viền
        TaoSpriteMau("TileGround", 0x2E2E2E);
        TaoSpriteMau("TileGround2", 0x3A3A3A);
        TaoSpriteMau("TileWall", 0x2E2E2E);

        AssetDatabase.Refresh();
        Debug.Log("✅ Đã tạo Ground Sprites (không viền)");
    }

    static void TaoSpriteMau(string ten, int hexColor)
    {
        string duongDan = $"Assets/Sprites/Tiles/{ten}.png";

        // Xóa file cũ nếu có
        if (File.Exists(Application.dataPath + $"/Sprites/Tiles/{ten}.png"))
            File.Delete(Application.dataPath + $"/Sprites/Tiles/{ten}.png");

        Texture2D tex = new Texture2D(16, 16);

        float r = ((hexColor >> 16) & 0xFF) / 255f;
        float g = ((hexColor >> 8) & 0xFF) / 255f;
        float b = (hexColor & 0xFF) / 255f;
        Color mau = new Color(r, g, b, 1f);

        // Fill đặc hoàn toàn — không viền
        for (int x = 0; x < 16; x++)
            for (int y = 0; y < 16; y++)
                tex.SetPixel(x, y, mau);

        tex.Apply();

        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(
            Application.dataPath + $"/Sprites/Tiles/{ten}.png", bytes);

        AssetDatabase.Refresh();

        TextureImporter importer =
            AssetImporter.GetAtPath(duongDan) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 16;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression =
                TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        Debug.Log($"✅ Tạo {ten}.png thành công");
    }

    [MenuItem("NoTrollHere/Create Chapter 2 Sprites")]
    static void TaoSpriteChuong2()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Sprites/Tiles/Chuong2"))
            AssetDatabase.CreateFolder("Assets/Sprites/Tiles", "Chuong2");

        TaoSpriteMau("Chuong2/TileGround_C2", 0x1E3A25);
        TaoSpriteMau("Chuong2/TileGround2_C2", 0x2A4A30);
        TaoSpriteMau("Chuong2/TileWall_C2", 0x1E3A25);

        AssetDatabase.Refresh();
        Debug.Log("✅ Đã tạo tile Chương 2");
    }
}