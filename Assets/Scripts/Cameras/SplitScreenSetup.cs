using UnityEngine;

public class SplitScreenSetup : MonoBehaviour
{
    public Camera player1Camera; // �߿�
    public Camera player2Camera; // ����

    void Start()
    {
        // ���� ����: Player1
        player1Camera.rect = new Rect(0f, 0f, 0.5f, 1f);

        // ������ ����: Player2
        player2Camera.rect = new Rect(0.5f, 0f, 0.5f, 1f);
    }
}
