using Nakama.TinyJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

namespace Nakama.Helpers
{
    public class NakamaManager : MonoBehaviour
    {
        #region FIELDS

        private const string UdidKey = "udid";

        /* A reference to the NakamaConnectionData scriptable object.  set local and host*/
        [SerializeField] private NakamaConnectionData connectionData = null;

        private IClient client = null;
        private ISession session = null;
        private ISocket socket = null;

        #endregion

        #region EVENTS

        public event Action onConnecting = null;
        public event Action onConnected = null;
        public event Action onDisconnected = null;
        public event Action onLoginSuccess = null;
        public event Action onLoginFail = null;

        #endregion

        #region PROPERTIES

        public static NakamaManager Instance { get; private set; } = null;
        public string Username { get => session == null ? string.Empty : session.Username; }
        public bool IsLoggedIn { get => socket != null && socket.IsConnected; }
        public ISocket Socket { get => socket; }
        public ISession Session { get => session; }
        public IClient Client { get => client; }
        private bool conncet;
        private bool loginActionSuccessful;

        public Sprite SpritePlayerIcon;
        #endregion

        #region BEHAVIORS

        private void Awake()
        {
            Instance = this;
        }

        private void OnApplicationQuit()
        {
            socket?.CloseAsync();

        }

        /// <summary>
        /// It gets the UDID from the PlayerPrefs, if it doesn't exist, it creates a new one and saves it
        /// to the PlayerPrefs
        /// </summary>
        public void LoginWithUdid()
        {
            var udid = PlayerPrefs.GetString(UdidKey, Guid.NewGuid().ToString());
            PlayerPrefs.SetString(UdidKey, udid);
            client = new Client(connectionData.Scheme, connectionData.Host, connectionData.Port, connectionData.ServerKey, UnityWebRequestAdapter.Instance);
            LoginAsync(connectionData, client.AuthenticateDeviceAsync(udid));
        }

        public void LoginWithDevice()
        {
            client = new Client(connectionData.Scheme, connectionData.Host, connectionData.Port, connectionData.ServerKey, UnityWebRequestAdapter.Instance);
            LoginAsync(connectionData, client.AuthenticateDeviceAsync(SystemInfo.deviceUniqueIdentifier));
            PlayerPrefs.SetString("Web3Token", "web");
        }

        public void LoginWithCustomId(string customId)
        {
            client = new Client(connectionData.Scheme, connectionData.Host, connectionData.Port, connectionData.ServerKey, UnityWebRequestAdapter.Instance);
            LoginAsync(connectionData, client.AuthenticateCustomAsync(customId));
            PlayerPrefs.SetString("Web3Token", "web");
        }
        public  IEnumerator LoginWithGoogle()
        {
            loginActionSuccessful = true;
            GetPublicKey();

            yield return null;
            Debug.Log(loginActionSuccessful);
          //  LoginCustome();
          //   NakamaUserManager.Instance.UpdateDisplayName(PlayerPrefs.GetString("USERNAME"));



        }
        public void LoginCustome()
        {

            client = new Client(connectionData.Scheme, connectionData.Host, connectionData.Port, connectionData.ServerKey, UnityWebRequestAdapter.Instance);
           
            LoginAsync(connectionData, client.AuthenticateCustomAsync(PlayerPrefs.GetString("EMAIL")));
        }

        /// <summary>
        /// get value HXD and set on storage
        /// </summary>
        public async void LoginWithSetHXDToStorage()
        {
            client = new Client(connectionData.Scheme, connectionData.Host, connectionData.Port, connectionData.ServerKey, UnityWebRequestAdapter.Instance);
            string name = PlayerPrefs.GetString("USERNAME").Replace(" ", "_");
            PlayerPrefs.SetString("USERNAME", name);

            LoginAsync(connectionData, client.AuthenticateCustomAsync(PlayerPrefs.GetString("USERNAME"), "", true));
            var wallet = new WalletData
            {
                hxdAmount = 500,
                _decimal = 1
            };
            await Task.Delay(1000);
            NakamaStorageManager.Instance.SendValueToServer(NakamaStorageManager.Instance.NakamaCollectionObjectWallet, wallet);
        }
        public void GetPublicKey()
        {
            Web3Manager.instance.Login(async (e, r) =>
           {
               loginActionSuccessful = false;
               Debug.Log($"idToken={e} publicKey={r}");
               LoginCustome();
               await Task.Delay(1000);
               NakamaUserManager.Instance.UpdateDisplayName(PlayerPrefs.GetString("USERNAME"));

           }, Onfail, OnUrl);
        }
        private void Onfail()
        {
            Debug.Log("Onfail");
        }
        private void OnUrl()
        {
            Debug.Log("ssss");
        }

        private async void LoginAsync(NakamaConnectionData connectionData, Task<ISession> sessionTask)
        {
            onConnecting?.Invoke();
            try
            {
                session = await sessionTask;
                socket = client.NewSocket(true);
                socket.Connected += Connected;

                socket.Closed += Disconnected;
                await socket.ConnectAsync(session);
                onLoginSuccess?.Invoke();
            }
            catch (Exception exception)
            {
                Debug.Log(exception);
                onLoginFail?.Invoke();
            }

        }

        public void LogOut()
        {
            socket.CloseAsync();
        }

        private void Connected()
        {
            onConnected?.Invoke();

        }

        private void Disconnected()
        {
            onDisconnected?.Invoke();

        }

        /// <summary>
        /// > Send an RPC to the server
        /// </summary>
        /// <param name="rpc">The name of the RPC you want to call.</param>
        /// <param name="payload">The JSON payload to send to the server.</param>
        /// <returns>
        /// The return value is an object of type IApiRpc.
        /// </returns>
        public async Task<IApiRpc> SendRPC(string rpc, string payload = "{}")
        {
            if (client == null || session == null)
                return null;

            return await client.RpcAsync(session, rpc, payload);
        }

        #endregion
    }
}
public class Claims
{
    public string id;
    public string username;
}
