using UnityEngine;
using System.Collections;

public class GeonwooMovement : MonoBehaviour
{
    public float gridSize = 1f;
    public float moveDuration = 0.1f;

    private bool isMoving = false;

    private int moveCount = 0; // ������ Ƚ��
    private int inputCount = 0; // �Է��� Ƚ�� (��ȣ�ۿ뵵 ����)


    void Update()
    {
        if (isMoving) return;

        Vector2 input = Vector2.zero;
        if (Input.GetKeyDown(KeyCode.W)) input = Vector2.up;
        else if (Input.GetKeyDown(KeyCode.S)) input = Vector2.down;
        else if (Input.GetKeyDown(KeyCode.A)) input = Vector2.left;
        else if (Input.GetKeyDown(KeyCode.D)) input = Vector2.right;

        if (input != Vector2.zero)
        {

            inputCount++;
            Debug.Log("����Ű �Է� Ƚ��: " + inputCount);

            Vector2 playerPos = transform.position;
            Vector2 targetPos = playerPos + input;
            Collider2D hit = Physics2D.OverlapCircle(targetPos, 0.1f);

            // 0. �տ� ������ �ְ�, ���� ���� ĭ�� ��������� �� ���� �̵�
            if (hit != null && hit.CompareTag("Obstacle_JumpHole"))
            {
                Vector2 gapNextPos = targetPos + input;
                Collider2D nextHit = Physics2D.OverlapCircle(gapNextPos, 0.1f);

                if (nextHit == null)
                {
                    // ���� + ���� ĭ ��� ������ �� ���� �̵�
                    StartCoroutine(Move(transform, gapNextPos));
                    return;
                }
                else
                {
                    // ���� ���� ĭ�� ���� ���� �� ���� ���� �� �̵� ����
                    return;
                }
            }

            // 1. �տ� Breakable �±� ������Ʈ�� ������ �ı��ϰ� ��
            if (hit != null && hit.CompareTag("Obstacle_Breakable"))
            {
                Destroy(hit.gameObject);
                Debug.Log("�ı� ������ ���� �ı���!");
                return;
            }

            // 2. Box�� �ִٸ� �� �ڰ� ������� �ڽ� �б�
            if (hit != null && hit.CompareTag("Obstacle_Box"))
            {
                Vector2 boxTargetPos = targetPos + input;
                Collider2D behindHit = Physics2D.OverlapCircle(boxTargetPos, 0.1f);

                // �ڽ� �ڿ� �ƹ��͵� ���ų�, ��/�ڽ�/������ �ƴϸ� �б� ����
                if (behindHit == null ||
                    (!behindHit.CompareTag("Obstacle_Box") &&
                     !behindHit.CompareTag("Obstacle_Wall") &&
                     !behindHit.CompareTag("Obstacle_Breakable") &&
                     !behindHit.CompareTag("Obstacle_JumpHole"))) // �� ���� �߰�
                {
                    StartCoroutine(Move(hit.transform, boxTargetPos));
                }

                return;
            }

            // 3. �̵� ������ ��� (�� ����)
            if (hit == null || (!hit.CompareTag("Obstacle_Wall") && !hit.CompareTag("Obstacle_Box") && !hit.CompareTag("Obstacle_Breakable")))
            {
                StartCoroutine(Move(transform, targetPos));
            }
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

        //  �÷��̾ �̵��� ��쿡�� �̵� Ƚ�� ����
        if (obj == transform)
        {
            moveCount++;
            Debug.Log("�̵� Ƚ��: " + moveCount);
        }
    }
}
