using UnityEngine;

public class Wall : MonoBehaviour, IInteractable
{
    public void Interact(GameObject interactor, Vector2 direction)
    {
        // ���� �⺻������ �ƹ� �ϵ� �Ͼ�� ����
        Debug.Log("Wall: ��ȣ�ۿ� ���� (����)");
    }
}
