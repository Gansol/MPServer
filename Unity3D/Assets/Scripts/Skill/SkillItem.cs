﻿using UnityEngine;
using System.Collections;
using MPProtocol;
using System.Collections.Generic;
public abstract class SkillItem : SkillBase
{
    protected List<GameObject> effects;

    public SkillItem(SkillAttr attr)
        : base(attr)
    {
        effects = new List<GameObject>();
    }
}
