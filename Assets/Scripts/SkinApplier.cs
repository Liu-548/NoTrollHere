using UnityEngine;

// Gắn lên Player — tự áp skin đang dùng khi scene load
public class SkinApplier : MonoBehaviour
{
    void Start()
    {
        if (SkinManager.instance != null)
            SkinManager.instance.ApSkinLenPlayer();
    }
}