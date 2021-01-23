﻿using Assets.Scripts.Crafting;
using Assets.Scripts.Crafting.Results;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ChooseCraftingSubType : MonoBehaviour
{
    public Dropdown TypeDropdown;
    public Dropdown HandednessDropdown;

    private Dropdown _subTypeDropdown;

    void Start()
    {
        _subTypeDropdown = transform.GetComponent<Dropdown>();
        _subTypeDropdown.onValueChanged.AddListener(OnValueChanged);

        HandednessDropdown.ClearOptions();
        HandednessDropdown.AddOptions(HandednessOptions);
        HandednessDropdown.gameObject.SetActive(false);
    }

    void OnValueChanged(int index)
    {
        SetHandednessDropDownVisibility(HandednessDropdown, TypeDropdown.options[TypeDropdown.value].text, _subTypeDropdown.options[index].text);
        UiHelper.UpdateResults(transform.parent.parent, new ResultFactory());
    }



    private static readonly string[] _handednessSubTypes = new[] { Weapon.Axe, Weapon.Sword, Weapon.Hammer, Weapon.Gun };

    public static List<string> HandednessOptions = new List<string> {
        Weapon.OneHanded,
        Weapon.TwoHanded
    };

    public static void SetHandednessDropDownVisibility(Dropdown handednessDropdown, string type, string subType)
    {
        handednessDropdown.gameObject.SetActive(type == ChooseCraftingType.CraftingTypeWeapon && _handednessSubTypes.Contains(subType));
    }

}