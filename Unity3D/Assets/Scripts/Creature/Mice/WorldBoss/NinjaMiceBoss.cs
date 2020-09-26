using UnityEngine;
using System.Collections;
using MPProtocol;

public class NinjaMiceBoss : MiceBossBase
{
    public override void Initialize(float lerpSpeed, float upSpeed, float upDistance, float lifeTime)
    {
        myHits = otherHits = 0;
        transform.localPosition = new Vector3(0, 0);
        GetComponent<BoxCollider2D>().enabled = true;

        m_Skill.Display(gameObject, m_Arribute, m_AIState);
    }

    protected override void OnHit()
    {
        if (Global.isGameStart /*&& ((cam.eventReceiverMask & gameObject.layer) == cam.eventReceiverMask) */&& enabled && m_Arribute.GetHP() > 0)
        {
            m_AnimState.Play(IAnimatorState.ENUM_AnimatorState.OnHit);
            ShadowAvatarSkill skill = m_Skill as ShadowAvatarSkill;

            if (m_Arribute.GetHP() - 1 == 0) GetComponent<BoxCollider2D>().enabled = false;

            if (m_Arribute.GetShield() == 0 && skill.GetShadowCount() == 0)
                Global.photonService.BossDamage(1);  // 傷害1是錯誤的 需要由Server判定、技能等級
            else
                m_Arribute.SetShield(m_Arribute.GetShield() - 1);

            if (m_Arribute.GetHP() <= 0)
                Destroy(gameObject);
        }
    }
}
