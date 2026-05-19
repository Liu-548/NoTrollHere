using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    [Header("=== CHUYỂN MÀN ===")]
    public string tenManTiepTheo = "LevelSelect";

    [Header("=== ANIMATION ===")]
    public float thoiGianChoAnimation = 0.4f;

    private Animator animator;
    private bool daThang = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void OnTriggerEnter2D(Collider2D vatTheChamVao)
    {
        if (vatTheChamVao.CompareTag("Player") && !daThang)
        {
            daThang = true;

            // SFX thắng — đúng chỗ, sau khi confirm là Player
            if (SoundManager.instance != null)
                SoundManager.instance.PlayThang();

            Time.timeScale = 0f;

            if (animator != null)
                animator.SetTrigger("Open");

            StartCoroutine(ChuyenManSauAnimation());
        }
    }

    IEnumerator ChuyenManSauAnimation()
    {
        // Dùng WaitForSecondsRealtime vì Time.timeScale = 0
        yield return new WaitForSecondsRealtime(thoiGianChoAnimation);

        // Restore timeScale trước khi chuyển màn
        Time.timeScale = 1f;
        GameManager.instance.ThangMan(tenManTiepTheo);
    }
}