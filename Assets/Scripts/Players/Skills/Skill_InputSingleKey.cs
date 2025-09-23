using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SkillInputSingleKey : MonoBehaviour
{
    [Header("�Է�")]
    public KeyCode skillKey = KeyCode.Q;
    [Tooltip("�� �ð�(��) �̻� ������ '�غ� �Ϸ�' ����")]
    public float holdThreshold = 1f;

    [Header("����")]
    public MonoBehaviour skillBehaviour;   // IAllySkill ���� ������Ʈ
    public GameObject partner;             // ��� �÷��̾�
    public Image holdGauge;                // ������ Image (Type=Filled)
    public CanvasGroup gaugeRoot;          // ������ ��Ʈ(CanvasGroup, ���̵��)

    [Header("�÷��̾� �̵�Ű ����(ĵ���� ���)")]
    public PlayerMovement ownerMovement;
    private bool canCancelUp, canCancelDown, canCancelLeft, canCancelRight; // Ȧ�� ���� ���� ������

    [Header("��ٿ�")]
    public float cooldown = 0.3f;

    [Header("UI")]
    [Tooltip("Ȧ�� ���� �� �� �ð��� ������ �������� ������(�� �� �ø�Ŀ ����)")]
    public float gaugeShowDelay = 0.3f;
    public float fadeDuration = 0.2f;
    public Color fillColor = Color.white;
    public Color readyColor = Color.green;

    private IAllySkill _skill;
    private float _pressedAt = -1f;
    private float _lastUseTime = -999f;
    private bool _ready = false;       // �Ӱ�ġ ���� ����(�� �� ����)
    private bool _canceled = false;    // Ȧ�� �� ����Ű�� ĵ�� ����
    private bool _gaugeShown = false;  // ���� �������� ���̴� ������
    private Coroutine _fadeCo;

    void Awake()
    {
        _skill = skillBehaviour as IAllySkill;
        if (_skill == null)
            Debug.LogError("[SkillInputSingleKey] skillBehaviour�� IAllySkill ������ �ʿ��մϴ�.");

        // ������ �⺻ ����: ��->��� ��������
        if (holdGauge != null)
        {
            holdGauge.type = Image.Type.Filled;
            holdGauge.fillMethod = Image.FillMethod.Horizontal;
            holdGauge.fillOrigin = (int)Image.OriginHorizontal.Left;
            holdGauge.fillAmount = 0f;
            holdGauge.color = fillColor;
        }

        // ������ ����
        SetGaugeVisible(false, instant: true);
    }

    void Update()
    {
        // ��ٿ� ���̸� ����
        if (Time.time - _lastUseTime < cooldown) { HideAndResetGauge(); return; }

        //  ĵ�� ���¿��� ��ų Ű ��� ������ ������ �ƹ� �͵� �� ��
        if (_canceled && Input.GetKey(skillKey))
            return;

        //  ��ų Ű�� ���� ĵ�� ��� ����
        if (_canceled && Input.GetKeyUp(skillKey))
        {
            _canceled = false;
            return;
        }

        // ĵ�� ���: Ű�� �� ������ �ƹ� ���� �� ��
        if (_canceled && Input.GetKey(skillKey)) return;
        if (_canceled && Input.GetKeyUp(skillKey)) { _canceled = false; return; }


        // ��ٿ� ���� �Է� ���� + UI ����
        if (Time.time - _lastUseTime < cooldown)
        {
            HideAndResetGauge();
            return;
        }

        // KeyDown: Ÿ�ӽ������� ���(��/Ȧ�� ���� ��)
        if (Input.GetKeyDown(skillKey))
        {
            _pressedAt = Time.time;
            _ready = false;
            _canceled = false;
            _gaugeShown = false;

            if (holdGauge != null)
            {
                holdGauge.fillAmount = 0f;
                holdGauge.color = fillColor;
            }
            SetGaugeVisible(false, instant: true); // ���̸� �� ������ ��

            // "�� ���� �̹� ���� �ִ�" ����Ű�� ĵ�� �ĺ����� ����
            // ������ �ε巯������!!!!
            if (ownerMovement != null)
            {
                canCancelUp = !Input.GetKey(ownerMovement.upKey);
                canCancelDown = !Input.GetKey(ownerMovement.downKey);
                canCancelLeft = !Input.GetKey(ownerMovement.leftKey);
                canCancelRight = !Input.GetKey(ownerMovement.rightKey);
            }
            else
            {
                // fallback (���ϸ� �����ص� ��)
                canCancelUp = !Input.GetKey(KeyCode.W);
                canCancelDown = !Input.GetKey(KeyCode.S);
                canCancelLeft = !Input.GetKey(KeyCode.A);
                canCancelRight = !Input.GetKey(KeyCode.D);
            }
        }

        // Key ���� ��: Ȧ�� ó��
        if (_pressedAt > 0f && Input.GetKey(skillKey))
        {
            // ����Ű �Է����� ĵ��
            if (IsMoveCancelTriggered())
            {
                _canceled = true;          // ��� ���
                _pressedAt = -1f;          // �̹� Ȧ�� ����
                HideAndResetGauge(keepCanceled: true);
                return;
            }

            float held = Time.time - _pressedAt;

            // ���� �� ó�� ���� �� 0���� ����
            if (held >= gaugeShowDelay && !_gaugeShown)
            {
                SetGaugeVisible(true);
                _gaugeShown = true;
                if (holdGauge != null) holdGauge.fillAmount = 0f; // �� 0���� ����
            }

            // ������ ä��� (ǥ�ÿ� �������)
            if (_gaugeShown && holdGauge != null)
            {
                float fill01 = GetDisplayFill(held);
                holdGauge.fillAmount = fill01;

                // �غ� �Ϸ� ������ '���� held'�� ���� ��� (�����ִ� �Ͱ� �и�)
                bool readyNow = held >= holdThreshold;
                holdGauge.color = readyNow ? readyColor : fillColor;
                _ready = readyNow; // Ű�� �� ��(_ready)�� �б�
            }
        }

        // KeyUp: �б�(ĵ��/Ȧ��Ϸ�/��)
        if (Input.GetKeyUp(skillKey) && _pressedAt > 0f)
        {
            float held = Time.time - _pressedAt;
            _pressedAt = -1f;

            if (_canceled)
            {
                // �ƹ� �͵� ���� ����
                HideAndResetGauge();
                return;
            }

            if (_ready || held >= holdThreshold)
            {
                // Ȧ�� �Ϸ�: Ű�� �� ������ ��뿡�� ����
                if (partner != null) _skill.UseOnAlly(gameObject, partner);
                else _skill.UseOnSelf(gameObject); // ��Ʈ�� ������ �ڱ� ����
            }
            else
            {
                // ��: �ڱ� ����
                _skill.UseOnSelf(gameObject);
            }

            _lastUseTime = Time.time;
            HideAndResetGauge();
        }
    }

    // ����Ű �Է� ����(Ű���� ��/ȭ��ǥ)
    private bool IsMoveCancelTriggered()
    {
        if (ownerMovement != null)
        {
            if (canCancelUp && Input.GetKeyDown(ownerMovement.upKey)) return true;
            if (canCancelDown && Input.GetKeyDown(ownerMovement.downKey)) return true;
            if (canCancelLeft && Input.GetKeyDown(ownerMovement.leftKey)) return true;
            if (canCancelRight && Input.GetKeyDown(ownerMovement.rightKey)) return true;
            return false;
        }

        // fallback (���ϸ� ����)
        if (canCancelUp && Input.GetKeyDown(KeyCode.W)) return true;
        if (canCancelDown && Input.GetKeyDown(KeyCode.S)) return true;
        if (canCancelLeft && Input.GetKeyDown(KeyCode.A)) return true;
        if (canCancelRight && Input.GetKeyDown(KeyCode.D)) return true;
        return false;
    }

    // === UI ��ƿ ===
    private void SetGaugeVisible(bool visible, bool instant = false)
    {
        //  �׻� Image.enabled�� �Բ� ����
        if (holdGauge != null) holdGauge.enabled = visible;

        if (gaugeRoot == null)
            return;

        if (_fadeCo != null) StopCoroutine(_fadeCo);

        if (instant || fadeDuration <= 0f)
        {
            gaugeRoot.alpha = visible ? 1f : 0f;
            gaugeRoot.interactable = visible;
            gaugeRoot.blocksRaycasts = visible;
        }
        else
        {
            _fadeCo = StartCoroutine(FadeGauge(visible ? 1f : 0f));
        }
    }

    private IEnumerator FadeGauge(float target)
    {
        float start = gaugeRoot.alpha;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            gaugeRoot.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }
        gaugeRoot.alpha = target;
        bool on = target > 0.99f;
        gaugeRoot.interactable = on;
        gaugeRoot.blocksRaycasts = on;
        _fadeCo = null;
    }

    private void HideAndResetGauge(bool keepCanceled = false)
    {
        if (holdGauge != null)
        {
            holdGauge.fillAmount = 0f;
            holdGauge.color = fillColor;
            //  gaugeRoot�� ���� ���� ���� ��
            if (gaugeRoot == null)
                holdGauge.enabled = false;
        }
        SetGaugeVisible(false);
        _gaugeShown = false;
        _ready = false;
        if (!keepCanceled)
            _canceled = false;   // �� �⺻�� �ʱ�ȭ, 'ĵ�� ��'���� ����
    }

    // ����: ǥ�ÿ� �����(0~1)
    // gaugeShowDelay ���� 0���� ������ holdThreshold���� 1�� �ǰ� ����
    float GetDisplayFill(float held)
    {
        // ������ġ: delay�� �Ӱ躸�� ũ�ų� ������ ���� �������
        if (holdThreshold <= gaugeShowDelay)
            return Mathf.Clamp01(held / Mathf.Max(0.0001f, holdThreshold));

        float visibleHeld = Mathf.Max(0f, held - gaugeShowDelay);
        float denom = holdThreshold - gaugeShowDelay;
        return Mathf.Clamp01(visibleHeld / Mathf.Max(0.0001f, denom));
    }

}
