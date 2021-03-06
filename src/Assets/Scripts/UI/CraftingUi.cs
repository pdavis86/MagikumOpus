﻿using Assets.ApiScripts.Registry;
using Assets.Core.Extensions;
using Assets.Core.Localization;
using Assets.Core.Registry.Base;
using Assets.Core.Registry.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Compiler
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable InconsistentNaming

public class CraftingUi : MonoBehaviour
{
    [SerializeField] private GameObject _componentsContainer;
    [SerializeField] private GameObject _rowPrefab;
    [SerializeField] private Text _outputText;
    [SerializeField] private Dropdown _typeDropdown;
    [SerializeField] private Dropdown _subTypeDropdown;
    [SerializeField] private Dropdown _handednessDropdown;
    [SerializeField] private InputField _craftName;
    [SerializeField] private Button _craftButton;
    [SerializeField] private Text _craftErrors;

    private PlayerInventory _inventory;
    private List<ItemBase> _components;
    private Dictionary<Type, string> _craftingCategories;
    private Dictionary<IGearArmor, string> _armorTypes;
    private Dictionary<IGearAccessory, string> _accessoryTypes;
    private Dictionary<IGearWeapon, string> _weaponTypes;
    private List<string> _handednessOptions;
    private List<int?> _optionalTwoHandedWeaponIndexes;

    private void Awake()
    {
        _components = new List<ItemBase>();

        _inventory = GameManager.Instance.LocalPlayer.GetComponent<PlayerInventory>();

        _typeDropdown.onValueChanged.AddListener(TypeOnValueChanged);

        _subTypeDropdown.onValueChanged.AddListener(SubTypeOnValueChanged);

        _handednessDropdown.onValueChanged.AddListener(HandednessOnValueChanged);

        _craftButton.onClick.AddListener(CraftButtonOnClick);

        _craftingCategories = new Dictionary<Type, string>
        {
            { typeof(Weapon), GameManager.Instance.Localizer.Translate(Localizer.TranslationType.CraftingCategory, nameof(Weapon)) },
            { typeof(Armor), GameManager.Instance.Localizer.Translate(Localizer.TranslationType.CraftingCategory, nameof(Armor)) },
            { typeof(Accessory), GameManager.Instance.Localizer.Translate(Localizer.TranslationType.CraftingCategory, nameof(Accessory)) },
            { typeof(Spell), GameManager.Instance.Localizer.Translate(Localizer.TranslationType.CraftingCategory, nameof(Spell)) }
        };

        _handednessOptions = new List<string> {
            { GameManager.Instance.Localizer.Translate(Localizer.TranslationType.WeaponHandedness, "one") },
            { GameManager.Instance.Localizer.Translate(Localizer.TranslationType.WeaponHandedness, "two") }
        };

        _armorTypes = GameManager.Instance.TypeRegistry.GetRegisteredTypes<IGearArmor>()
            .ToDictionary(x => x, x => GameManager.Instance.Localizer.GetTranslatedTypeName(x))
            .OrderBy(x => x.Value)
            .ToDictionary(x => x.Key, x => x.Value);

        _accessoryTypes = GameManager.Instance.TypeRegistry.GetRegisteredTypes<IGearAccessory>()
            .ToDictionary(x => x, x => GameManager.Instance.Localizer.GetTranslatedTypeName(x))
            .OrderBy(x => x.Value)
            .ToDictionary(x => x.Key, x => x.Value);

        _weaponTypes = GameManager.Instance.TypeRegistry.GetRegisteredTypes<IGearWeapon>()
            .ToDictionary(x => x, x => GameManager.Instance.Localizer.GetTranslatedTypeName(x))
            .OrderBy(x => x.Value)
            .ToDictionary(x => x.Key, x => x.Value);

        _optionalTwoHandedWeaponIndexes = _weaponTypes
            .Select((x, i) => !x.Key.EnforceTwoHanded && x.Key.AllowTwoHanded ? (int?)i : null)
            .Where(x => x != null)
            .ToList();
    }

    private void CraftButtonOnClick()
    {
        _craftButton.interactable = false;

        var componentIds = _components.Select(x => x.Id).ToArray();
        var selectedType = GetCraftingCategory();
        var selectedSubType = GetCraftableTypeName(selectedType);
        var isTwoHanded = IsTwoHandedSelected();

        _inventory.CmdCraftItem(componentIds, selectedType, selectedSubType, isTwoHanded, _craftName.text);
    }

    void SubTypeOnValueChanged(int index)
    {
        try
        {
            SetHandednessDropDownVisibility();
            UpdateResults();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    void HandednessOnValueChanged(int index)
    {
        UpdateResults();
    }

    private void SetHandednessDropDownVisibility()
    {
        _handednessDropdown.gameObject.SetActive(GetCraftingCategory() == nameof(Weapon) && _optionalTwoHandedWeaponIndexes.Contains(_subTypeDropdown.value));
    }

    void TypeOnValueChanged(int index)
    {
        try
        {
            _subTypeDropdown.ClearOptions();

            var isSpell = false;

            switch (GetCraftingCategory())
            {
                case nameof(Weapon): _subTypeDropdown.AddOptions(_weaponTypes.Select(x => x.Value).ToList()); break;
                case nameof(Armor): _subTypeDropdown.AddOptions(_armorTypes.Select(x => x.Value).ToList()); break;
                case nameof(Accessory): _subTypeDropdown.AddOptions(_accessoryTypes.Select(x => x.Value).ToList()); break;
                case nameof(Spell): isSpell = true; break;

                default:
                    throw new InvalidOperationException("Unknown crafting type");
            }

            if (isSpell)
            {
                _subTypeDropdown.gameObject.SetActive(false);
            }
            else
            {
                _subTypeDropdown.RefreshShownValue();
                _subTypeDropdown.gameObject.SetActive(true);
            }

            SetHandednessDropDownVisibility();

            UpdateResults();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    private void OnEnable()
    {
        ResetUi();
        LoadInventory();
    }

    private void OnDisable()
    {
        _componentsContainer.transform.Clear();
    }

    public void AddComponent(string itemId)
    {
        var item = _inventory.Items.FirstOrDefault(x => x.Id == itemId);

        if (item == null)
        {
            Debug.LogWarning("No item found with id " + itemId);
            return;
        }

        _components.Add(item);
    }

    public void RemoveComponent(string itemId)
    {
        var item = _components.FirstOrDefault(x => x.Id == itemId);
        if (item != null)
        {
            _components.Remove(item);
        }
    }

    public void ResetUi()
    {
        _typeDropdown.ClearOptions();
        _typeDropdown.AddOptions(_craftingCategories.Select(x => x.Value).ToList());

        _handednessDropdown.ClearOptions();
        _handednessDropdown.AddOptions(_handednessOptions);

        TypeOnValueChanged(0);

        ResetUiText();

        _craftButton.interactable = false;
    }

    private void ResetUiText()
    {
        _outputText.text = null;
        _craftName.text = null;
        _craftErrors.text = null;
    }

    public void LoadInventory()
    {
        _components.Clear();

        InventoryItemsList.LoadInventoryItems(
            null,
            _componentsContainer,
            _rowPrefab,
            _inventory,
            HandleRowToggle,
            null,
            false
        );
    }

    private void HandleRowToggle(GameObject row, GameObject slot, ItemBase item)
    {
        var rowImage = row.GetComponent<Image>();
        var toggle = row.GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(isOn =>
        {
            if (isOn)
            {
                rowImage.color = Color.green;
                AddComponent(item.Id);
            }
            else
            {
                rowImage.color = Color.white;
                RemoveComponent(item.Id);
            }

            UpdateResults();
        });
    }

    private string GetCraftingCategory()
    {
        return _craftingCategories.ElementAt(_typeDropdown.value).Key.Name;
    }

    private string GetCraftableTypeName(string craftingCategory)
    {
        switch (craftingCategory)
        {
            case nameof(Weapon): return (_weaponTypes.ElementAt(_subTypeDropdown.value).Key).TypeName;
            case nameof(Armor): return (_armorTypes.ElementAt(_subTypeDropdown.value).Key).TypeName;
            case nameof(Accessory): return (_accessoryTypes.ElementAt(_subTypeDropdown.value).Key).TypeName;
            case nameof(Spell): return null;
            default: throw new InvalidOperationException("Unknown crafting type");
        }
    }

    private bool IsTwoHandedSelected()
    {
        return _handednessDropdown.options.Count > 0 && _handednessDropdown.value == 1;
    }

    private void UpdateResults()
    {
        ResetUiText();

        if (_components.Count == 0)
        {
            return;
        }

        var craftingCategory = GetCraftingCategory();

        var craftedItem = GameManager.Instance.ResultFactory.GetCraftedItem(
            craftingCategory,
            GetCraftableTypeName(craftingCategory),
            IsTwoHandedSelected(),
            _components
        );

        var errors = _inventory.ValidateIsCraftable(_components.Select(x => x.Id).ToArray(), craftedItem);
        if (errors.Any())
        {
            _craftErrors.text = string.Join(Environment.NewLine, errors);
            _craftButton.interactable = false;
            return;
        }

        _craftButton.interactable = true;
        _outputText.text = GameManager.Instance.ResultFactory.GetItemDescription(craftedItem);
    }

}
