using UnityEngine;
using System.Collections;

public static class MPGFactory
{
    private static ObjectFactory m_ObjFactory =null;
    private static SkillFactory m_SkillFactory = null;
    private static AttrFactory m_AttrFactory = null;
    private static AnimFactory m_AnimFactory = null;

    public static ObjectFactory GetObjFactory()
    {
        if (m_ObjFactory == null)
            m_ObjFactory = new ObjectFactory();
        return m_ObjFactory;
    }

    public static SkillFactory GetSkillFactory()
    {
        if (m_SkillFactory == null)
            m_SkillFactory = new SkillFactory();
        return m_SkillFactory;
    }

    public static AttrFactory GetAttrFactory()
    {
        if (m_AttrFactory == null)
            m_AttrFactory = new AttrFactory();
        return m_AttrFactory;
    }

    public static AnimFactory GetAnimFactory()
    {
        if (m_AnimFactory == null)
            m_AnimFactory = new AnimFactory();
        return m_AnimFactory;
    }
}
