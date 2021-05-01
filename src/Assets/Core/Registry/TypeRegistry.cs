﻿using Assets.ApiScripts.Registry;
using Assets.Core.Registry.Base;
using Assets.Core.Registry.Types;
using Assets.Core.Spells.Shapes;
using Assets.Core.Spells.Targeting;
using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable ConvertToNullCoalescingCompoundAssignment
// ReSharper disable PossibleMultipleEnumeration

namespace Assets.Core.Registry
{
    public class TypeRegistry
    {
        private readonly List<IGearAccessory> _accessories = new List<IGearAccessory>();
        private readonly List<IGearArmor> _armor = new List<IGearArmor>();
        private readonly List<IGearWeapon> _weapons = new List<IGearWeapon>();
        private readonly List<ILoot> _loot = new List<ILoot>();
        private readonly List<IEffect> _effects = new List<IEffect>();
        private readonly List<ISpellShape> _shapes = new List<ISpellShape>();
        private readonly List<ISpellTargeting> _targeting = new List<ISpellTargeting>();

        public void FindAndRegisterAll()
        {
            RegisterCoreTypes();

            //todo: how do we scan for registrable types?

            foreach (var t in new Standard.Registration().GetRegisterables())
            {
                ValidateAndRegister(t);
            }
        }

        private void RegisterCoreTypes()
        {
            ValidateAndRegister(typeof(Wall));
            ValidateAndRegister(typeof(Zone));

            ValidateAndRegister(typeof(Beam));
            ValidateAndRegister(typeof(Cone));
            ValidateAndRegister(typeof(Projectile));
            ValidateAndRegister(typeof(Self));
            ValidateAndRegister(typeof(Touch));
        }

        private void ValidateAndRegister(Type type)
        {
            if (!typeof(IRegisterable).IsAssignableFrom(type))
            {
                UnityEngine.Debug.LogError($"{type.Name} does not implement {nameof(IRegisterable)}");
                return;
            }

            var toRegister = Activator.CreateInstance(type);

            if (toRegister is IGearAccessory accessory)
            {
                Register(_accessories, accessory);
                return;
            }
            else if (toRegister is IGearArmor armor)
            {
                Register(_armor, armor);
                return;
            }
            else if (toRegister is IGearWeapon craftableWeapon)
            {
                Register(_weapons, craftableWeapon);
                return;
            }
            else if (toRegister is ILoot loot)
            {
                Register(_loot, loot);
                return;
            }
            else if (toRegister is IEffect effect)
            {
                Register(_effects, effect);
                return;
            }
            else if (toRegister is ISpellShape shape)
            {
                Register(_shapes, shape);
                return;
            }
            else if (toRegister is ISpellTargeting targeting)
            {
                Register(_targeting, targeting);
                return;
            }

            UnityEngine.Debug.LogError($"{type.Name} does not implement any of the valid interfaces");
        }

        private void Register<T>(List<T> list, T item) where T : IRegisterable
        {
            var match = list.FirstOrDefault(x => x.TypeId == item.TypeId);
            if (match != null)
            {
                UnityEngine.Debug.LogError($"A type with name '{item.TypeId}' has already been registered");
                return;
            }

            list.Add(item);
        }

        public IEnumerable<T> GetRegisteredTypes<T>() where T : IRegisterable
        {
            var interfaceName = typeof(T).Name;
            switch (interfaceName)
            {
                case nameof(IGearAccessory): return (IEnumerable<T>)_accessories;
                case nameof(IGearArmor): return (IEnumerable<T>)_armor;
                case nameof(IGearWeapon): return (IEnumerable<T>)_weapons;
                case nameof(ILoot): return (IEnumerable<T>)_loot;
                case nameof(IEffect): return (IEnumerable<T>)_effects;
                case nameof(ISpellShape): return (IEnumerable<T>)_shapes;
                case nameof(ISpellTargeting): return (IEnumerable<T>)_targeting;
                default: throw new Exception($"Unexpected type {interfaceName}");
            }
        }

        public T GetRegisteredByTypeName<T>(string typeName) where T : IRegisterable
        {
            return GetRegisteredTypes<T>().FirstOrDefault(x => x.TypeName.Equals(typeName, StringComparison.OrdinalIgnoreCase));
        }

        private T GetRegisteredById<T>(string typeId) where T : IRegisterable
        {
            var craftablesOfType = GetRegisteredTypes<T>();

            if (string.IsNullOrWhiteSpace(typeId))
            {
                return (T)(object)null;
            }

            var matches = craftablesOfType.Where(x => x.TypeId == new Guid(typeId));

            if (!matches.Any())
            {
                throw new Exception($"Could not find a match for '{typeof(T).Name}' and '{typeId}'");
            }
            else if (matches.Count() > 1)
            {
                throw new Exception($"How is there more than one match for '{typeof(T).Name}' and '{typeId}'");
            }

            return matches.First();
        }

        public IRegisterable GetRegisteredForItem(ItemBase item)
        {
            if (item is Accessory)
            {
                return GetRegisteredById<IGearAccessory>(item.RegistryTypeId);
            }
            else if (item is Armor)
            {
                return GetRegisteredById<IGearArmor>(item.RegistryTypeId);
            }
            else if (item is Weapon)
            {
                return GetRegisteredById<IGearWeapon>(item.RegistryTypeId);
            }
            else if (item is Loot)
            {
                return GetRegisteredById<ILoot>(item.RegistryTypeId);
            }

            return null;
        }

        public List<IEffect> GetLootPossibilities()
        {
            return _effects
                .Where(x => !x.IsSideEffect)
                .ToList();
        }

        public IEffect GetEffect(Guid typeId)
        {
            return _effects.FirstOrDefault(x => x.TypeId == typeId);
        }








        //todo: un-hardcode this
        public static UnityEngine.GameObject GetPrefabForWeaponType(string typeName, bool twoHanded)
        {
            switch (typeName)
            {
                case "Axe": return twoHanded ? GameManager.Instance.Prefabs.Weapons.Axe2 : GameManager.Instance.Prefabs.Weapons.Axe1;
                case "Bow": return GameManager.Instance.Prefabs.Weapons.Bow;
                case "Crossbow": return GameManager.Instance.Prefabs.Weapons.Crossbow;
                case "Dagger": return GameManager.Instance.Prefabs.Weapons.Dagger;
                case "Gun": return twoHanded ? GameManager.Instance.Prefabs.Weapons.Gun2 : GameManager.Instance.Prefabs.Weapons.Gun1;
                case "Hammer": return twoHanded ? GameManager.Instance.Prefabs.Weapons.Hammer2 : GameManager.Instance.Prefabs.Weapons.Hammer1;
                case "Shield": return GameManager.Instance.Prefabs.Weapons.Shield;
                case "Staff": return GameManager.Instance.Prefabs.Weapons.Staff;
                case "Sword": return twoHanded ? GameManager.Instance.Prefabs.Weapons.Sword2 : GameManager.Instance.Prefabs.Weapons.Sword1;
                default: throw new Exception($"Unexpected weapon type {typeName}");
            }
        }

    }
}
