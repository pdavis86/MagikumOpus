﻿using Assets.Core.Helpers;
using UnityEngine;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global

public class SceneObjectsOffline : MonoBehaviour
{
    void Start()
    {
        UnityHelper.GetObjectAtRoot(GameManager.NameCanvasScene).SetActive(true);
    }
    
}
