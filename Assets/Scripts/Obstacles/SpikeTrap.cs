using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    private bool isActive = true;

    public void Deactivate()
    {
        if (!isActive) return;

        isActive = false;
        Debug.Log("������ ���ŵǾ����ϴ�!");
        Destroy(gameObject); // ���� ����
    }
}
