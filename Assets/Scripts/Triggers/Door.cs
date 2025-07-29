using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [Header("�� ���� ȿ��")]
    public GameObject openEffect; // ���� ���� �� ����Ʈ (����)

    public void Interact(GameObject interactor, Vector2 direction)
    {
        // �÷��̾� ��Ʈ�ѷ����� ���� ���� Ȯ��
        PlayerMovement player = interactor.GetComponent<PlayerMovement>();
        if (player != null && player.HasKey())
        {
            // ���� ���
            player.UseKey();

            // ����Ʈ ��� (����)
            if (openEffect != null)
                Instantiate(openEffect, transform.position, Quaternion.identity);

            // �� ����
            Destroy(gameObject);
            Debug.Log(" ���� ���Ƚ��ϴ�!");
        }
        else
        {
            Debug.Log(" ���谡 �����ϴ�!");
        }
    }
}
