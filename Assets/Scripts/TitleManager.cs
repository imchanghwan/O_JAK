using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PressAnyKeyHandler_Text : MonoBehaviour
{
    public GameObject pressAnyKeyText; // �����̴� Text ������Ʈ
    public GameObject menuGroup;       // Play/Option/Quit ����(ó�� ��Ȱ��ȭ)
    public Button firstButton;         // ó�� ���õ� ��ư(Play)
    public string StageSceneName;

    bool activated = false;

    void Start()
    {
        if (menuGroup) menuGroup.SetActive(false);
        if (pressAnyKeyText) pressAnyKeyText.SetActive(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) return; // esc�� ����
        if (activated) return;

        if (Input.anyKeyDown)
        {
            activated = true;

            if (pressAnyKeyText) pressAnyKeyText.SetActive(false);
            if (menuGroup) menuGroup.SetActive(true);

            if (firstButton)
                EventSystem.current.SetSelectedGameObject(firstButton.gameObject);

            Time.timeScale = 1f;
            SceneManager.LoadScene(StageSceneName);
        }
    }
}
