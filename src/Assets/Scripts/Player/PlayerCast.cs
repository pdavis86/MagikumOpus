﻿using UnityEngine;

public class PlayerCast : MonoBehaviour
{
    public GameObject SpellPrefab;
    public GameObject HitTextUiPrefab;
    public GameObject UiCanvas;

    private Camera _camera;

    private void Start()
    {
        _camera = transform.Find("PlayerCamera").GetComponent<Camera>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CastSpell();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            CastSpell(true);
        }
    }

    void CastSpell(bool leftHand = false)
    {
        var startPos = transform.position + _camera.transform.forward + new Vector3(leftHand ? -0.15f : 0.15f, -0.1f, 0);
        var spell = Instantiate(SpellPrefab, startPos, transform.rotation);
        spell.SetActive(true);

        var spellRb = spell.GetComponent<Rigidbody>();
        spellRb.AddForce(_camera.transform.forward * 20f, ForceMode.VelocityChange);

        var spellScript = spell.GetComponent<SpellBehaviour>();
        spellScript.HitTextUiPrefab = HitTextUiPrefab;
        spellScript.Player = gameObject;
        spellScript.PlayerCamera = _camera;
        spellScript.UiCanvas = UiCanvas;
    }

}
