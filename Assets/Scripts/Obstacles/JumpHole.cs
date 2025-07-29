using UnityEngine;
using System.Collections;

public class JumpHole : MonoBehaviour, IInteractable
{
    public float moveDuration = 0.1f;

    public void Interact(GameObject interactor, Vector2 direction)
    {
        Vector2 currentPos = transform.position;
        Vector2 targetPos = currentPos + direction;

        // ���� ĭ Ȯ��
        Collider2D hit = Physics2D.OverlapCircle(targetPos, 0.1f);

        if (hit == null || !IsBlocked(hit))
        {
            PlayerMovement player = interactor.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.MoveTo(targetPos);
                Debug.Log("JumpHole: �̵� ����");

                // �̵� �Ϸ� �� ���ȣ�ۿ� �õ�
                player.StartCoroutine(DelayedRecheck(player, direction));
            }
        }
        else
        {
            Debug.Log("JumpHole: �̵� ���� - ����");
        }
    }

    bool IsBlocked(Collider2D col)
    {
        if (col == null) return false;

        return col.CompareTag("Obstacle_Wall") ||
               col.CompareTag("Obstacle_Box") ||
               col.CompareTag("Obstacle_Breakable");
    }

    IEnumerator DelayedRecheck(PlayerMovement player, Vector2 direction)
    {
        // isMoving�� false�� �� ������ ��ٸ�
        while (player.IsMoving)
            yield return null;

        // ���� ��ġ���� �ٽ� ��ȣ�ۿ� �õ�
        player.TryInteractOnly(direction);
    }
}
