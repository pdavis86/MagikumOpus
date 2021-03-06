﻿using Assets.ApiScripts.Registry;
using Assets.Core.Localization;
using Assets.Core.Registry;
using Assets.Core.Registry.Base;
using Assets.Core.Registry.Types;
using Assets.Core.Spells.Shapes;
using Assets.Core.Spells.Targeting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable InconsistentNaming

namespace Assets.Core.Crafting
{
    public class ResultFactory
    {
        private static readonly Random _random = new Random();

        private readonly TypeRegistry _typeRegistry;
        private readonly Localizer _localizer;
        private readonly List<ILoot> _lootTypes;
        private readonly List<IEffect> _effectsForLoot;
        private readonly List<ISpellTargeting> _spellTargetingOptions;
        private readonly List<ISpellShape> _spellShapeOptions;

        public ResultFactory(TypeRegistry typeRegistry, Localizer localizer)
        {
            _typeRegistry = typeRegistry;
            _localizer = localizer;

            _lootTypes = _typeRegistry.GetRegisteredTypes<ILoot>().ToList();
            _effectsForLoot = _typeRegistry.GetLootPossibilities();
            _spellTargetingOptions = _typeRegistry.GetRegisteredTypes<ISpellTargeting>().ToList();
            _spellShapeOptions = _typeRegistry.GetRegisteredTypes<ISpellShape>().ToList();
        }

        private int ComputeAttribute(IEnumerable<ItemBase> components, Func<ItemBase, int> getProp, bool allowMax = true)
        {
            var withValue = components.Where(x => getProp(x) > 0);

            if (!withValue.Any())
            {
                return 1;
            }

            var min = withValue.Min(getProp);
            var max = withValue.Max(getProp);
            var topEndSkew = max - ((max - min) / 10);

            int result = allowMax
                ? topEndSkew
                : (int)Math.Round(topEndSkew - (0.009 * (topEndSkew - 50)), MidpointRounding.AwayFromZero);

            //Debug.Log($"{getProp.Method.Name} = Min:{min}, Max:{max}, Skew:{topEndSkew}, Result:{result}");

            return result == 0 ? 1 : result;
        }

        public ISpellTargeting GetSpellTargeting(string typeName)
        {
            return _spellTargetingOptions.First(x => x.TypeName == typeName);
        }

        private ISpellTargeting GetSpellTargeting(IEnumerable<IMagical> spellComponents)
        {
            //Exactly one targeting option
            var targetingComponent = spellComponents.FirstOrDefault(x => x.Targeting != null);

            if (targetingComponent == null)
            {
                return _spellTargetingOptions.First(x => x is Projectile);
            }

            return targetingComponent.Targeting;
        }

        public ISpellShape GetSpellShape(string typeName)
        {
            return _spellShapeOptions.First(x => x.TypeName == typeName);
        }

        private ISpellShape GetSpellShape(ISpellTargeting targeting, IEnumerable<IMagical> spellComponents)
        {
            //Only one shape, if any
            if (!targeting.HasShape)
            {
                return null;
            }

            var shapeComponent = spellComponents.FirstOrDefault(x => x.Shape != null);

            if (shapeComponent == null)
            {
                return null;
            }

            return _spellShapeOptions.FirstOrDefault(x => x.TypeId == shapeComponent.Shape.TypeId);
        }

        private List<IEffect> GetEffects(string craftingType, IEnumerable<ItemBase> components)
        {
            var effects = components.Where(x => x.Effects != null).SelectMany(x => x.Effects);

            var elementalEffects = effects.Where(x => x is IElement);
            var elementalEffect = elementalEffects.FirstOrDefault();
            if (elementalEffect != null)
            {
                effects = effects
                    .Except(elementalEffects.Where(x => x != elementalEffect));
            }

            if (craftingType == nameof(Armor) || craftingType == nameof(Accessory))
            {
                return effects
                    .Where(x =>
                         x is IEffectBuff
                        || x is IEffectSupport
                        || x is IElement)
                    .ToList();
            }

            if (craftingType == nameof(Weapon))
            {
                return effects
                    .Where(x =>
                        x is IEffectDebuff
                        || x is IElement)
                    .ToList();
            }

            if (craftingType != nameof(Spell))
            {
                throw new Exception($"Unexpected craftingType '{craftingType}'");
            }

            if (effects.Any(x => x is IEffectBuff || x is IEffectSupport))
            {
                effects = effects
                    .Where(x =>
                        !(x is IEffectDebuff)
                        && !(x is IElement));
            }

            return effects.ToList();
        }

        private bool IsSuccess(int percentageChance)
        {
            return _random.Next(1, 101) <= percentageChance;
        }

        private int GetAttributeValue(int percentageChance)
        {
            return IsSuccess(percentageChance) ? _random.Next(1, 100) : 0;
        }

        private IEffect GetRandomEffect()
        {
            return _effectsForLoot.ElementAt(_random.Next(0, _effectsForLoot.Count));
        }

        private ISpellTargeting GetRandomSpellTargeting()
        {
            if (IsSuccess(50))
            {
                return _spellTargetingOptions.ElementAt(_random.Next(0, _spellTargetingOptions.Count));
            }
            return null;
        }

        private ISpellShape GetRandomSpellShape()
        {
            if (IsSuccess(10))
            {
                return _spellShapeOptions.ElementAt(_random.Next(0, _spellShapeOptions.Count));
            }
            return null;
        }

        private int GetMinBiasedNumber(int min, int max)
        {
            return min + (int)Math.Round((max - min) * Math.Pow(_random.NextDouble(), 3), 0);
        }

        internal ItemBase GetLootDrop()
        {
            var lootDrop = new Loot
            {
                Id = Guid.NewGuid().ToString(),
                Attributes = new Attributes
                {
                    IsAutomatic = IsSuccess(50),
                    IsSoulbound = IsSuccess(10),
                    ExtraAmmoPerShot = IsSuccess(20) ? _random.Next(1, 4) : 0,
                    Strength = GetAttributeValue(75),
                    Efficiency = GetAttributeValue(75),
                    Range = GetAttributeValue(75),
                    Accuracy = GetAttributeValue(75),
                    Speed = GetAttributeValue(75),
                    Recovery = GetAttributeValue(75),
                    Duration = GetAttributeValue(75),
                    Luck = GetAttributeValue(75)
                }
            };

            var magicalLootTypes = _lootTypes.Where(x => x.Category == ILoot.LootCategory.Magic);
            var techLootTypes = _lootTypes.Where(x => x.Category == ILoot.LootCategory.Technology);

            var isMagical = magicalLootTypes.Any() && IsSuccess(50);
            if (isMagical)
            {
                lootDrop.RegistryType = magicalLootTypes
                    .OrderBy(x => _random.Next())
                    .First();

                var effects = new List<IEffect>();
                var numberOfEffects = GetMinBiasedNumber(1, Math.Min(4, _effectsForLoot.Count));
                for (var i = 1; i <= numberOfEffects; i++)
                {
                    IEffect effect;
                    var debugCounter = 0;
                    do
                    {
                        effect = GetRandomEffect();
                        debugCounter++;
                    }
                    while (debugCounter < 10 && (effect == null || effects.Contains(effect)));
                    effects.Add(effect);

                    if (debugCounter >= 10)
                    {
                        UnityEngine.Debug.LogError("Infinite loop situation here. Go fix it!");
                    }
                }

                lootDrop.Effects = effects.ToList();

                lootDrop.Targeting = GetRandomSpellTargeting();
                lootDrop.Shape = GetRandomSpellShape();

                //Debug.Log($"Added {numberOfEffects} effects: {string.Join(", ", lootDrop.Effects)}");
            }
            else
            {
                lootDrop.RegistryType = techLootTypes
                    .OrderBy(x => _random.Next())
                    .First();
            }

            var typeTranslation = _localizer.Translate("crafting.loot.type");
            var suffix = int.Parse(lootDrop.GetHashCode().ToString().TrimStart('-').Substring(5));
            lootDrop.Name = $"{_localizer.GetTranslatedTypeName(lootDrop.RegistryType)} ({typeTranslation} #{suffix.ToString("D5", CultureInfo.InvariantCulture)})";

            return lootDrop;
        }

        private Spell GetSpell(IEnumerable<ItemBase> components)
        {
            var spellComponents = components.OfType<IMagical>();

            var targeting = GetSpellTargeting(spellComponents);

            var spell = new Spell
            {
                Id = Guid.NewGuid().ToString(),
                Targeting = targeting,
                Shape = GetSpellShape(targeting, spellComponents),
                Attributes = new Attributes
                {
                    IsSoulbound = components.Any(x => x.Attributes.IsSoulbound),
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                    Efficiency = ComputeAttribute(components, x => x.Attributes.Efficiency),
                    Range = ComputeAttribute(components, x => x.Attributes.Range),
                    Accuracy = ComputeAttribute(components, x => x.Attributes.Accuracy),
                    Speed = ComputeAttribute(components, x => x.Attributes.Speed),
                    Recovery = ComputeAttribute(components, x => x.Attributes.Recovery),
                    Duration = ComputeAttribute(components, x => x.Attributes.Duration)
                },
                Effects = GetEffects(nameof(Spell), components)
            };

            var suffix = _localizer.Translate(Localizer.TranslationType.CraftingCategory, nameof(Spell));

            if (spell.Effects.Count > 0)
            {
                spell.Name = _localizer.GetTranslatedTypeName(spell.Effects.First()) + " " + suffix;
            }
            else
            {
                var customSuffix = _localizer.GetTranslatedTypeName(spell.Targeting) + " " + suffix;
                spell.Name = GetItemName(true, spell, customSuffix);
            }

            return spell;
        }

        private string GetItemNamePrefix(bool isAttack)
        {
            return _localizer.Translate(Localizer.TranslationType.CraftingNamePrefix, isAttack ? "attack" : "defence");
        }

        private string GetItemName(bool isAttack, ItemBase item, string customSuffix = null)
        {
            var suffix = string.IsNullOrWhiteSpace(customSuffix)
                ? _localizer.GetTranslatedTypeName(item.RegistryType)
                : customSuffix;
            return $"{GetItemNamePrefix(isAttack)} {item.Attributes.Strength} {suffix}";
        }

        private Weapon GetMeleeWeapon(IGearWeapon craftableType, IEnumerable<ItemBase> components, bool isTwoHanded)
        {
            var weapon = new Weapon()
            {
                RegistryType = craftableType,
                Id = Guid.NewGuid().ToString(),
                IsTwoHanded = craftableType.EnforceTwoHanded || (craftableType.AllowTwoHanded && isTwoHanded),
                Attributes = new Attributes
                {
                    IsSoulbound = components.Any(x => x.Attributes.IsSoulbound),
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                    Accuracy = ComputeAttribute(components, x => x.Attributes.Accuracy),
                    Speed = ComputeAttribute(components, x => x.Attributes.Speed)
                },
                Effects = GetEffects(nameof(Weapon), components)
            };
            weapon.Name = GetItemName(true, weapon);
            return weapon;
        }

        private Weapon GetRangedWeapon(IGearWeapon craftableType, IEnumerable<ItemBase> components, bool isTwoHanded)
        {
            var weapon = new Weapon()
            {
                RegistryType = craftableType,
                Id = Guid.NewGuid().ToString(),
                IsTwoHanded = craftableType.EnforceTwoHanded || (craftableType.AllowTwoHanded && isTwoHanded),
                Attributes = new Attributes
                {
                    IsSoulbound = components.Any(x => x.Attributes.IsSoulbound),
                    IsAutomatic = craftableType.AllowAutomatic && components.Any(x => x.Attributes.IsAutomatic),
                    ExtraAmmoPerShot = components.FirstOrDefault(x => x.Attributes.ExtraAmmoPerShot > 0)?.Attributes.ExtraAmmoPerShot ?? 0,
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                    Efficiency = ComputeAttribute(components, x => x.Attributes.Efficiency),
                    Range = ComputeAttribute(components, x => x.Attributes.Range),
                    Accuracy = ComputeAttribute(components, x => x.Attributes.Accuracy),
                    Speed = ComputeAttribute(components, x => x.Attributes.Speed),
                    Recovery = ComputeAttribute(components, x => x.Attributes.Recovery)
                },
                Effects = GetEffects(nameof(Weapon), components)
            };
            weapon.Name = GetItemName(true, weapon);
            return weapon;
        }

        private Weapon GetDefensiveWeapon(IGearWeapon craftableType, IEnumerable<ItemBase> components, bool isTwoHanded)
        {
            var weapon = new Weapon()
            {
                RegistryType = craftableType,
                Id = Guid.NewGuid().ToString(),
                IsTwoHanded = craftableType.EnforceTwoHanded || (craftableType.AllowTwoHanded && isTwoHanded),
                Attributes = new Attributes
                {
                    IsSoulbound = components.Any(x => x.Attributes.IsSoulbound),
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                    Speed = ComputeAttribute(components, x => x.Attributes.Speed),
                    Recovery = ComputeAttribute(components, x => x.Attributes.Recovery)
                },
                Effects = GetEffects(nameof(Weapon), components)
            };
            weapon.Name = GetItemName(false, weapon);
            return weapon;
        }

        private Armor GetArmor(IGearArmor craftableType, IEnumerable<ItemBase> components)
        {
            var armor = new Armor()
            {
                RegistryType = craftableType,
                Id = Guid.NewGuid().ToString(),
                Attributes = new Attributes
                {
                    IsSoulbound = components.Any(x => x.Attributes.IsSoulbound),
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength)
                },
                Effects = GetEffects(nameof(Armor), components)
            };
            armor.Name = GetItemName(false, armor);
            return armor;
        }

        private Armor GetBarrier(IGearArmor craftableType, IEnumerable<ItemBase> components)
        {
            var armor = new Armor()
            {
                RegistryType = craftableType,
                Id = Guid.NewGuid().ToString(),
                Attributes = new Attributes
                {
                    IsSoulbound = components.Any(x => x.Attributes.IsSoulbound),
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength),
                    Efficiency = ComputeAttribute(components, x => x.Attributes.Efficiency),
                    Speed = ComputeAttribute(components, x => x.Attributes.Speed),
                    Recovery = ComputeAttribute(components, x => x.Attributes.Recovery)
                },
                Effects = GetEffects(nameof(Armor), components)
            };
            armor.Name = GetItemName(false, armor);
            return armor;
        }

        private Accessory GetAccessory(IGearAccessory craftableType, IEnumerable<ItemBase> components)
        {
            var accessory = new Accessory()
            {
                RegistryType = craftableType,
                Id = Guid.NewGuid().ToString(),
                Attributes = new Attributes
                {
                    IsSoulbound = components.Any(x => x.Attributes.IsSoulbound),
                    Strength = ComputeAttribute(components, x => x.Attributes.Strength)
                },
                Effects = GetEffects(nameof(Accessory), components)
            };
            accessory.Name = GetItemName(true, accessory);
            return accessory;
        }

        public ItemBase GetCraftedItem(string categoryName, string typeName, bool isTwoHanded, IEnumerable<ItemBase> components)
        {
            if (categoryName == nameof(Spell))
            {
                return GetSpell(components);
            }

            switch (categoryName)
            {
                case nameof(Weapon):
                    var craftableWeapon = _typeRegistry.GetRegisteredByTypeName<IGearWeapon>(typeName);
                    switch (craftableWeapon.Category)
                    {
                        case IGearWeapon.WeaponCategory.Melee: return GetMeleeWeapon(craftableWeapon, components, isTwoHanded);
                        case IGearWeapon.WeaponCategory.Ranged: return GetRangedWeapon(craftableWeapon, components, isTwoHanded);
                        case IGearWeapon.WeaponCategory.Defensive: return GetDefensiveWeapon(craftableWeapon, components, isTwoHanded);
                        default: throw new Exception($"Unexpected weapon category '{craftableWeapon.Category}'");
                    }

                case nameof(Armor):
                    var craftableArmor = _typeRegistry.GetRegisteredByTypeName<IGearArmor>(typeName);
                    if (craftableArmor.InventorySlot == IGearArmor.ArmorSlot.Barrier)
                    {
                        return GetBarrier(craftableArmor, components);
                    }
                    return GetArmor(craftableArmor, components);

                case nameof(Accessory):
                    var craftableAccessory = _typeRegistry.GetRegisteredByTypeName<IGearAccessory>(typeName);
                    return GetAccessory(craftableAccessory, components);

                default:
                    throw new Exception($"Unexpected craftable category '{categoryName}'");
            }
        }

        private string GetAttributeTranslation(string suffix)
        {
            return _localizer.Translate(Localizer.TranslationType.Attribute, suffix);
        }

        public string GetItemDescription(ItemBase item, bool includeName = true)
        {
            if (item == null)
            {
                return null;
            }

            var sb = new StringBuilder();

            if (includeName) { sb.Append($"{GetAttributeTranslation(nameof(item.Name))}: {item.Name}\n"); }
            if (item.Attributes.IsAutomatic) { sb.Append(GetAttributeTranslation(nameof(item.Attributes.IsAutomatic)) + "\n"); }
            if (item.Attributes.IsSoulbound) { sb.Append(GetAttributeTranslation(nameof(item.Attributes.IsSoulbound)) + "\n"); }
            if (item.Attributes.ExtraAmmoPerShot > 0) { sb.Append($"{GetAttributeTranslation(nameof(item.Attributes.ExtraAmmoPerShot))}: {item.Attributes.ExtraAmmoPerShot}\n"); }
            if (item.Attributes.Strength > 0) { sb.Append($"{GetAttributeTranslation(nameof(item.Attributes.Strength))}: {item.Attributes.Strength}\n"); }
            if (item.Attributes.Efficiency > 0) { sb.Append($"{GetAttributeTranslation(nameof(item.Attributes.Efficiency))}: {item.Attributes.Efficiency}\n"); }
            if (item.Attributes.Range > 0) { sb.Append($"{GetAttributeTranslation(nameof(item.Attributes.Range))}: {item.Attributes.Range}\n"); }
            if (item.Attributes.Accuracy > 0) { sb.Append($"{GetAttributeTranslation(nameof(item.Attributes.Accuracy))}: {item.Attributes.Accuracy}\n"); }
            if (item.Attributes.Speed > 0) { sb.Append($"{GetAttributeTranslation(nameof(item.Attributes.Speed))}: {item.Attributes.Speed}\n"); }
            if (item.Attributes.Recovery > 0) { sb.Append($"{GetAttributeTranslation(nameof(item.Attributes.Recovery))}: {item.Attributes.Recovery}\n"); }
            if (item.Attributes.Duration > 0) { sb.Append($"{GetAttributeTranslation(nameof(item.Attributes.Duration))}: {item.Attributes.Duration}\n"); }

            if (item.Effects != null && item.Effects.Count > 0)
            {
                var localisedEffects = item.Effects.Select(x => _localizer.GetTranslatedTypeName(x));
                sb.Append($"{GetAttributeTranslation(nameof(item.Effects))}: {string.Join(", ", localisedEffects)}\n");
            }

            if (item is IMagical spell)
            {
                if (spell.Targeting != null) { sb.Append($"{GetAttributeTranslation(nameof(spell.Targeting))}: {_localizer.GetTranslatedTypeName(spell.Targeting)}\n"); }
                if (spell.Shape != null) { sb.Append($"{GetAttributeTranslation(nameof(spell.Shape))}: {_localizer.GetTranslatedTypeName(spell.Shape)}\n"); }
            }

            return sb.ToString();
        }

    }
}
