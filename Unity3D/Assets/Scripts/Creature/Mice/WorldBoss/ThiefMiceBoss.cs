using UnityEngine;
using System.Collections;
using MPProtocol;

public class ThiefMiceBoss : MiceBossBase
{

    public override void Initialize(float lerpSpeed, float upSpeed, float upDistance, float lifeTime)
    {
        myHits = otherHits = 0;
        transform.localPosition = new Vector3(0, 0);
        GetComponent<BoxCollider2D>().enabled = true;

        m_Skill.Display(gameObject,m_Arribute/*, m_AIState*/);
    }

    public override void SetAI(ICreatureAI ai)
    {
        //if (m_AI != null)
        //    m_AI = null;
        m_AI = ai;
    }

    public override void SetGameObject(GameObject go)
    {
        throw new System.NotImplementedException();
    }

    public override void Update()
    {
        throw new System.NotImplementedException();
    }
}
