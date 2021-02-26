﻿using Assets.Scripts.Crafting.Results;
using Assets.Scripts.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global

public class Inventory : NetworkBehaviour
{
    public int MaxItems;
    public List<ItemBase> Items;

    private void Awake()
    {
        Items = new List<ItemBase>();
    }

    private void Start()
    {
        if (isLocalPlayer)
        {
            var netId = GetComponent<NetworkIdentity>();
            if (netId == null)
            {
                return;
            }
            netId.connectionToServer.RegisterHandler(Assets.Scripts.Networking.MessageIds.InventoryLoad, OnLoadInventory);
            netId.connectionToServer.RegisterHandler(Assets.Scripts.Networking.MessageIds.InventoryAddItem, OnAddItemToInventory);
            netId.connectionToServer.RegisterHandler(Assets.Scripts.Networking.MessageIds.InventoryAddAccessory, OnAddAccessoryToInventory);
            netId.connectionToServer.RegisterHandler(Assets.Scripts.Networking.MessageIds.InventoryAddArmor, OnAddArmorToInventory);
            netId.connectionToServer.RegisterHandler(Assets.Scripts.Networking.MessageIds.InventoryAddSpell, OnAddSpellToInventory);
            netId.connectionToServer.RegisterHandler(Assets.Scripts.Networking.MessageIds.InventoryAddWeapon, OnAddWeaponToInventory);
        }
    }

    private void OnLoadInventory(NetworkMessage netMsg)
    {
        //todo: make a generic load method instead of here
        var loadData = JsonUtility.FromJson<PlayerSave>(netMsg.ReadMessage<StringMessage>().value);

        LoadData(loadData.Inventory);
    }

    private void OnAddItemToInventory(NetworkMessage netMsg)
    {
        AddOfType<ItemBase>(netMsg.ReadMessage<StringMessage>().value);
    }

    private void OnAddAccessoryToInventory(NetworkMessage netMsg)
    {
        AddOfType<Accessory>(netMsg.ReadMessage<StringMessage>().value);
    }

    private void OnAddArmorToInventory(NetworkMessage netMsg)
    {
        AddOfType<Armor>(netMsg.ReadMessage<StringMessage>().value);
    }

    private void OnAddSpellToInventory(NetworkMessage netMsg)
    {
        AddOfType<Spell>(netMsg.ReadMessage<StringMessage>().value);
    }

    private void OnAddWeaponToInventory(NetworkMessage netMsg)
    {
        AddOfType<Weapon>(netMsg.ReadMessage<StringMessage>().value);
    }







    public void LoadData(Assets.Scripts.Data.Inventory loadData)
    {
        MaxItems = loadData.MaxItems == 0 ? 30 : loadData.MaxItems;

        if (loadData.Loot != null) { Items.AddRange(loadData.Loot); }
        if (loadData.Accessories != null) { Items.AddRange(loadData.Accessories); }
        if (loadData.Armor != null) { Items.AddRange(loadData.Armor); }
        if (loadData.Spells != null) { Items.AddRange(loadData.Spells); }
        if (loadData.Weapons != null) { Items.AddRange(loadData.Weapons); }

        //Debug.Log($"There are {Items.Count} items in the inventory after loading on " + (isServer ? "server" : "client") + " for " + gameObject.name);
    }

    public Assets.Scripts.Data.Inventory GetSaveData()
    {
        var groupedItems = Items.GroupBy(x => x.GetType());

        return new Assets.Scripts.Data.Inventory
        {
            MaxItems = MaxItems,
            Loot = groupedItems.FirstOrDefault(x => x.Key == typeof(ItemBase))?.ToArray(),
            Accessories = groupedItems.FirstOrDefault(x => x.Key == typeof(Accessory))?.Select(x => x as Accessory).ToArray() as Accessory[],
            Armor = groupedItems.FirstOrDefault(x => x.Key == typeof(Armor))?.Select(x => x as Armor).ToArray() as Armor[],
            Spells = groupedItems.FirstOrDefault(x => x.Key == typeof(Spell))?.Select(x => x as Spell).ToArray() as Spell[],
            Weapons = groupedItems.FirstOrDefault(x => x.Key == typeof(Weapon))?.Select(x => x as Weapon).ToArray() as Weapon[]
        };
    }

    private void AddOfType<T>(string stringValue) where T : ItemBase
    {
        Add(JsonUtility.FromJson<T>(stringValue));
        //Debug.Log($"Inventory now has {Items.Count} items in it");
    }

    public void Add(ItemBase item)
    {
        if (Items.Count == MaxItems)
        {
            //todo: send to storage instead
            //Debug.Log("Your inventory is at max");
            return;
        }

        Items.Add(item);
    }

    public void RemoveIds(IEnumerable<string> idEnumerable)
    {
        foreach (var id in idEnumerable)
        {
            var matchingItem = Items.FirstOrDefault(x => x.Id.ToString().Equals(id, StringComparison.OrdinalIgnoreCase));
            if (matchingItem == null)
            {
                Debug.LogError("No item found with ID: " + id);
                continue;
            }
            Items.Remove(matchingItem);
        }
    }

}