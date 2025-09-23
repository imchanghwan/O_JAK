using UnityEngine;

public class Door : MonoBehaviour, IInteractable {

    [Header("�� �Ŵ���")]
    public DoorManager doorManager;     // DoorManager ����

    public void Interact(GameObject interactor, Vector2 direction) {
        Vector2 currentPos = transform.position;
        PlayerMovement player = interactor.GetComponent<PlayerMovement>();

        // �÷��̾� ��Ʈ�ѷ����� ���� ���� Ȯ��
        if (player != null && player.HasKey()) {
            player.UseKey();        // ���� ���
            Destroy(gameObject);    // �� ����
            player.MoveTo(currentPos); // �÷��̾� �̵�

            // DoorManager�� �� ����
            if (doorManager != null)
                doorManager.DoorOpened();
        }
    }
}
