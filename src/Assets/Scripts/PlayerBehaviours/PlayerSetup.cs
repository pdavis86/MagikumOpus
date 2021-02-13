﻿using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

// ReSharper disable once CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Compiler

public class PlayerSetup : NetworkBehaviour
{
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private TextMeshProUGUI _nameTag;
    [SerializeField] private MeshRenderer _mainMesh;
    [SerializeField] private MeshRenderer _leftMesh;
    [SerializeField] private MeshRenderer _rightMesh;

    [SyncVar]
    public string PlayerName;

    [SyncVar]
    public string TextureUri;

    private Camera _sceneCamera;
    private SceneObjects001 _sceneObjects;

    private void Awake()
    {
        _sceneObjects = GameManager.GetSceneObjects().GetComponent<SceneObjects001>();
    }

    private void Start()
    {
        _sceneCamera = Camera.main;

        gameObject.name = "Player ID " + netId.Value;

        if (!isLocalPlayer)
        {
            gameObject.GetComponent<PlayerController>().enabled = false;

            _nameTag.text = string.IsNullOrWhiteSpace(PlayerName) ? "Player " + netId.Value : PlayerName;

            if (!string.IsNullOrWhiteSpace(TextureUri))
            {
                SetPlayerTexture(TextureUri);
            }

            return;
        }

        if (_sceneCamera != null)
        {
            _sceneCamera.gameObject.SetActive(false);
        }

        _playerCamera.gameObject.SetActive(true);

        _nameTag.text = null;

        _sceneObjects.UiHud.SetActive(true);

        var pm = gameObject.AddComponent<PlayerMovement>();
        pm.PlayerCamera = _playerCamera;

        ClientScene.RegisterPrefab(_sceneObjects.PrefabSpell);


        var joinParams = NetworkManager.singleton.GetComponent<JoinOrHostGame>();


        //todo: let players specify a URL to a texture PNG
        var filePath = joinParams.PlayerSkinUrl; // @"C:\Users\Paul\Desktop\Untitled.png";
        if (!string.IsNullOrWhiteSpace(filePath) && System.IO.File.Exists(filePath))
        {
            SetPlayerTexture(filePath);
            //todo: upload file
            TextureUri = filePath;
        }

        CmdHeresMyJoiningDetails(joinParams.PlayerName, joinParams.PlayerSkinUrl);
    }

    private void OnDisable()
    {
        if (_sceneObjects.UiHud != null)
        {
            _sceneObjects.UiHud.SetActive(false);
        }

        if (_sceneCamera != null)
        {
            _sceneCamera.gameObject.SetActive(true);
        }
    }

    void SetPlayerTexture(string playerSkinUri)
    {
        //todo: download file
        var filePath = playerSkinUri;

        var tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
        tex.LoadImage(System.IO.File.ReadAllBytes(filePath));
        var newMat = new Material(_mainMesh.material.shader);
        newMat.mainTexture = tex;

        _mainMesh.material = newMat;

        if (isLocalPlayer)
        {
            _leftMesh.material = newMat;
            _rightMesh.material = newMat;
        }
    }

    [Command]
    void CmdHeresMyJoiningDetails(string playerName, string playerSkinUri)
    {
        if (!string.IsNullOrWhiteSpace(playerName))
        {
            PlayerName = playerName;
        }

        if (!string.IsNullOrWhiteSpace(playerSkinUri))
        {
            TextureUri = playerSkinUri;
        }

        //var playerSetups = GameObject.FindGameObjectsWithTag("Player").Select(x => x.GetComponent<PlayerSetup>());
        //foreach (var playerSetup in playerSetups)
        //{
        //    playerSetup.RpcSetPlayerMaterial(playerSetup.TextureUri);
        //}

        RpcSetPlayerDetails(playerName, playerSkinUri);
    }

    [ClientRpc]
    void RpcSetPlayerDetails(string playerName, string playerSkinUri)
    {
        _nameTag.text = string.IsNullOrWhiteSpace(playerName) ? "Player " + netId.Value : playerName;

        if (!string.IsNullOrWhiteSpace(playerSkinUri))
        {
            //Debug.LogError($"Applying texture {uriToDownloadAndApply} to player {gameObject.name}");
            SetPlayerTexture(playerSkinUri);
        }
        else
        {
            Debug.LogError($"No texture for player {_nameTag.text}");
        }
    }

}
