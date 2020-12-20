using UnityEngine;
using System.Collections;
using MPProtocol;

public class BlackMiceBoss : MiceBossBase {

    public BlackMiceBoss()
    {

    }

    public override void Initialize(float lerpSpeed, float upSpeed, float upDistance, float lifeTime)
    {
        myHits = otherHits = 0;
        transform.localPosition = new Vector3(0, 0);
        GetComponent<BoxCollider2D>().enabled = true;

        m_Skill.Display(gameObject, m_Arribute/*, m_AIState*/);
    }

}
