﻿using System;
using UnityEngine;
using UnityEngine.EventSystems;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Compiler

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public delegate void PointerEnterEventDelegate(PointerEventData eventData);
    public event PointerEnterEventDelegate OnPointerEnterForTooltip;

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnPointerEnterForTooltip?.Invoke(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Tooltips.HideTooltip();
    }

    public void ClearHandlers()
    {
        if (OnPointerEnterForTooltip == null)
        {
            return;
        }
        foreach (Delegate d in OnPointerEnterForTooltip.GetInvocationList())
        {
            OnPointerEnterForTooltip -= (PointerEnterEventDelegate)d;
        }
    }

}
