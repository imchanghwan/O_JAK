using UnityEngine;

public interface IAllySkill
{
    void UseOnSelf(GameObject caster);               // �ڱ⿡�� ���
    void UseOnAlly(GameObject caster, GameObject ally); // ��Ʈ�ʿ��� ���
}
