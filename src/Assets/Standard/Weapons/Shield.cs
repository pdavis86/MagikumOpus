﻿using Assets.ApiScripts.Registry;
using System;

namespace Assets.Standard.Weapons
{
    public class Shield : IGearWeapon
    {
        public Guid TypeId => new Guid("2b0d5e47-77b0-4311-98ee-0e41827f5fc4");

        public string TypeName => nameof(Shield);

        public IGearWeapon.WeaponCategory Category => IGearWeapon.WeaponCategory.Defensive;

        public bool AllowAutomatic => false;

        public bool AllowTwoHanded => true;

        public bool EnforceTwoHanded => false;

        public string PrefabAddress => "Standard/Prefabs/Shield.prefab";

        //todo: different prefab for two-handed
        public string PrefabAddressTwoHanded => "Standard/Prefabs/Shield.prefab";
    }
}
