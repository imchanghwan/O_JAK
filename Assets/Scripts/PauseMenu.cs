using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public Animator animator;
    public Animator bgAnimator;
    public string titleSceneName = "TitleScene";
    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        animator.SetTrigger("Open"); // SlideIn ����
        bgAnimator.SetTrigger("FadeOut");
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void Resume()
    {
        animator.SetTrigger("Close"); // SlideOut ����
        bgAnimator.SetTrigger("FadeIn");
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void ReturnToTitle()
    {
        SceneManager.LoadScene(titleSceneName);
    }

    public void QuitGame()
    {
        // Ȥ�ó� ���� ���·� ���� �ʵ��� ����
        Time.timeScale = 1f;

    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }

}
