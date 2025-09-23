using UnityEngine;
using UnityEngine.EventSystems;

public class UIBlocker : MonoBehaviour {
    void Start() {
        // �� ���� �� UI ���� ����
        EventSystem.current.SetSelectedGameObject(null);
    }

    void Update() {
        // �÷��� �� ��� UI�� ��Ŀ���� ���� Ǯ����
        if (EventSystem.current.currentSelectedGameObject != null)
            EventSystem.current.SetSelectedGameObject(null);
    }
}
