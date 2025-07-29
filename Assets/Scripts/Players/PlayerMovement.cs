using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("�̵� ����")]
    public float gridSize = 1f;
    public float moveDuration = 0.1f;

    [Header("UI ����")]
    public TMPro.TextMeshProUGUI moveCountText;
    public TMPro.TextMeshProUGUI inputCountText;

    private bool isMoving = false;
    private int moveCount = 0;
    private int inputCount = 0;
    public bool hasKey = false; // ����

    public bool IsMoving => isMoving; // �̰� ���Ҹ��� �𸣰��������� �ƹ�ư �ʿ��ѵ�


    void Update()
    {
        if (isMoving) return;

        Vector2 input = GetInputDirection();
        if (input != Vector2.zero)
        {
            inputCount++;
            UpdateInputUI();

            TryMoveOrInteract(input);
        }
    }

    Vector2 GetInputDirection()
    {
        if (Input.GetKeyDown(KeyCode.W)) return Vector2.up;
        if (Input.GetKeyDown(KeyCode.S)) return Vector2.down;
        if (Input.GetKeyDown(KeyCode.A)) return Vector2.left;
        if (Input.GetKeyDown(KeyCode.D)) return Vector2.right;
        return Vector2.zero;
    }

    void TryMoveOrInteract(Vector2 dir)
    {
        Vector2 playerPos = transform.position;
        Vector2 targetPos = playerPos + dir;

        Collider2D hit = Physics2D.OverlapCircle(targetPos, 0.1f);
        if (hit != null)
        {
            IInteractable interactable = hit.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact(gameObject, dir);
                return;
            }
        }

        // �̵� ������ ��� (hit == null)
        if (hit == null)
        {
            StartCoroutine(Move(transform, targetPos));
        }
    }

    IEnumerator Move(Transform obj, Vector2 target)
    {
        isMoving = true;

        Vector2 start = obj.position;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            obj.position = Vector2.Lerp(start, target, t);
            yield return null;
        }

        obj.position = target;
        isMoving = false;

        if (obj == transform)
        {
            moveCount++;
            UpdateMoveUI();
        }
    }

    public void MoveTo(Vector2 targetPos)
    {
        if (!isMoving)
        {
            StartCoroutine(Move(transform, targetPos));
        }
    }

    // JumpHole���� ���
    public void TryInteractOnly(Vector2 dir)
    {

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.1f);

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            IInteractable interactable = hit.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact(gameObject, dir);
                break; // ù ��°�� ó��
            }
        }
    }


    void UpdateInputUI()
    {
        if (inputCountText != null)
            inputCountText.text = $"�Է� Ƚ��: {inputCount}";
    }

    void UpdateMoveUI()
    {
        if (moveCountText != null)
            moveCountText.text = $"�̵� Ƚ��: {moveCount}";
    }

    public void AcquireKey()
    {
        hasKey = true;
        Debug.Log(" ���� ȹ��!");
    }

    public void UseKey()
    {
        hasKey = false;
        Debug.Log(" ���� ���!");
    }

    public bool HasKey()
    {
        return hasKey;
    }
}

