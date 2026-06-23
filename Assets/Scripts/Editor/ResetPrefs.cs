#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class ResetPrefs
{
    [MenuItem("NoTrollHere/Reset PlayerPrefs")]
    static void Reset()
    {
        if (EditorUtility.DisplayDialog(
            "Reset PlayerPrefs",
            "Xóa toàn bộ progress, deaths, unlock, skin, achievement?",
            "Xóa hết", "Hủy"))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("[ResetPrefs] Đã xóa toàn bộ PlayerPrefs.");
        }
    }
}
#endif
