﻿using Assets.Core.Registry.Types;

namespace Assets.Core.Data
{
    [System.Serializable]
    public class Inventory
    {
        public int MaxItems;

        public Loot[] Loot;
        public Accessory[] Accessories;
        public Armor[] Armor;
        public Spell[] Spells;
        public Weapon[] Weapons;

        public string[] EquipSlots;
    }
}
