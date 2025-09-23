using UnityEngine;
using UnityEngine.UI;

public class JiknyeoSkill : MonoBehaviour, IAllySkill
{
    [Header("��ų ����")]
    public int maxUses = 3;   // �ִ� ��� ���� Ƚ��
    private int remainingUses; // ���� Ƚ��

    [Header("UI ����")]
    public Text usesText;
    public Animator anim;

    [Header("ĳ���� �ִϸ��̼�")]
    public Animator casterAnim; // ĳ���� �ִϸ�����
    public Animator allyAnim;

    void Awake()
    {
        remainingUses = maxUses; // ���� �� Ǯ�� ä��
    }

    // ���������� ui ����
    void OnEnable()
    {
        UpdateUI();
    }

    // ���� ����
    void Start()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        if (usesText != null)
            usesText.text = $"{remainingUses}";
    }

    public void UseOnSelf(GameObject caster)
    {
        if (remainingUses <= 0)
        {
            anim.SetTrigger("Pulse"); // �ִϸ��̼� Ʈ����
            Debug.Log("���� Ƚ�� ����!");
            return;
        }

        var pm = caster.GetComponent<PlayerMovement>();
        if (!pm) return;

        Vector2 playerPos = pm.transform.position;
        Vector2 targetPos = playerPos + pm.LastDir;

        Collider2D hit = Physics2D.OverlapCircle(targetPos, 0.1f);

        if (hit != null && hit.CompareTag("Obstacle_JumpHole"))
        {
            IInteractable interactable = hit.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact(caster, pm.LastDir);
                remainingUses--; // ���� �� Ƚ�� ����
                Debug.Log($"��ų ���! ���� Ƚ��: {remainingUses}/{maxUses}");
                UpdateUI(); // UI ����
                anim.SetTrigger("Pulse"); // �ִϸ��̼� Ʈ����
                casterAnim.SetTrigger("Jump");
                return;
            }
        }
    }

    public void UseOnAlly(GameObject caster, GameObject ally)
    {

        if (remainingUses <= 0)
        {
            anim.SetTrigger("Pulse"); // �ִϸ��̼� Ʈ����
            Debug.Log("���� Ƚ�� ����!");
            return;
        }

        var pm = ally.GetComponent<PlayerMovement>();
        if (!pm) return;

        Vector2 playerPos = pm.transform.position;
        Vector2 targetPos = playerPos + pm.LastDir;

        Collider2D hit = Physics2D.OverlapCircle(targetPos, 0.1f);

        if (hit != null && hit.CompareTag("Obstacle_JumpHole"))
        {
            IInteractable interactable = hit.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact(ally, pm.LastDir);
                remainingUses--; // ���� �� Ƚ�� ����
                Debug.Log($"��ų ���! ���� Ƚ��: {remainingUses}/{maxUses}");
                UpdateUI(); // UI ����
                anim.SetTrigger("Pulse"); // �ִϸ��̼� Ʈ����
                allyAnim.SetTrigger("Jump");
                return;
            }
        }
    }


}
