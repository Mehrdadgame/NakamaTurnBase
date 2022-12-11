using Merkator.BitCoin;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Web3Manager :MonoBehaviour
{

    [SerializeField] private Web3Auth auth;
    public static Web3Manager instance;
    private Action<string, string> _onSuccess;
    private Action _onFail;
    public Web3AuthResponse web3AuthResponse;

    private void Awake()
    {
        instance = this;
    }
    public void Login(Action<string, string> onSuccess, Action onFail, Action onUriReceived)
    {
        auth.setOptions(new Web3AuthOptions()
        {
            

        });
        auth.onLogin += OnLogin;
        auth.onLogout += OnLogout;
        auth.onUriReceived += onUriReceived;
        var options = new LoginParams()
        {
            loginProvider = Provider.GOOGLE,
            curve = Curve.ED25519,
        };
        _onSuccess = onSuccess;
        _onFail = onFail;
        auth.login(options);
    }

    private void OnLogin(Web3AuthResponse response)
    {
        //Debug.Log($"authenticated : {JsonConvert.SerializeObject(response)}");
        if (!string.IsNullOrEmpty(response.error))
        {
            _onFail?.Invoke();
            return;
        }
        var privateByteArray = new List<byte>();
        for (var i = 0; i < response.ed25519PrivKey.Length; i += 2)
        {
            var bt = Convert.ToByte(response.ed25519PrivKey.Substring(i, 2), 16);
            privateByteArray.Add(bt);
        }
        var privateKey = Base58Encoding.Encode(privateByteArray.ToArray());
        PlayerPrefs.SetString("Web3PrivateKey", privateKey);
        var publicKeyHex = response.ed25519PrivKey.Substring(response.privKey.Length);
        var publicByteArray = new List<byte>();
        for (var i = 0; i < publicKeyHex.Length; i += 2)
        {
            var bt = Convert.ToByte(publicKeyHex.Substring(i, 2), 16);
            publicByteArray.Add(bt);
        }
        var publicKey = Base58Encoding.Encode(publicByteArray.ToArray());
        Debug.Log(publicKey);
        PlayerPrefs.SetString("Web3PublicKey", publicKey);
        PlayerPrefs.SetString("Web3Token", response.userInfo.idToken);
        PlayerPrefs.SetString("USERNAME", response.userInfo.name);
        PlayerPrefs.SetString("EMAIL", response.userInfo.email);
        PlayerPrefs.SetString("Image", response.userInfo.profileImage);
        _onSuccess?.Invoke(response.userInfo.idToken, publicKey);
        web3AuthResponse= response;
    }

    private void OnLogout()
    {
        // TODO
    }


}
