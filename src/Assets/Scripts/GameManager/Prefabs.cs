﻿using UnityEngine;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

public class Prefabs : MonoBehaviour
{
    public CombatClass Combat;

    [System.Serializable]
    public class CombatClass
    {
        public GameObject Spell;
        public GameObject HitText;
        public GameObject ElementalText;
    }

}
