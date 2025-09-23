using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartStage : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            // ���� Ȱ��ȭ�� �� �̸� ��������
            string currentScene = SceneManager.GetActiveScene().name;

            // �� �ٽ� �ε�
            SceneManager.LoadScene(currentScene);
        }
    }
}
