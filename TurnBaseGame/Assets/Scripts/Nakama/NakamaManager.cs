using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Nakama.Helpers
{
    public class NakamaManager : MonoBehaviour
    {
        #region FIELDS

        private const string UdidKey = "udid";

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

        // Returns stable device ID. Falls back to hardware ID so cache-clear doesn't create new accounts.
        private string GetOrCreateUdid()
        {
            var stored = PlayerPrefs.GetString(UdidKey, "");
            if (!string.IsNullOrEmpty(stored)) return stored;

            // Hardware device ID — stable on Android even after cache clear
            var hwId = SystemInfo.deviceUniqueIdentifier;
            if (!string.IsNullOrEmpty(hwId) && hwId != "n/a" && hwId != SystemInfo.unsupportedIdentifier)
            {
                PlayerPrefs.SetString(UdidKey, hwId);
                PlayerPrefs.Save();
                return hwId;
            }

            var newId = Guid.NewGuid().ToString();
            PlayerPrefs.SetString(UdidKey, newId);
            PlayerPrefs.Save();
            return newId;
        }

        public void LoginWithUdid()
        {
            var udid = GetOrCreateUdid();
            client = new Client(connectionData.Scheme, connectionData.Host, connectionData.Port, connectionData.ServerKey, UnityWebRequestAdapter.Instance);
            Debug.Log("[NakamaManager] LoginWithUdid: " + udid.Substring(0, Mathf.Min(8, udid.Length)) + "...");
            LoginAsync(client.AuthenticateDeviceAsync(udid));
        }

        // Login with email+password. After success, links current device to this account.
        public void LoginWithEmail(string email, string password)
        {
            client = new Client(connectionData.Scheme, connectionData.Host, connectionData.Port, connectionData.ServerKey, UnityWebRequestAdapter.Instance);
            LoginAsync(client.AuthenticateEmailAsync(email, password, null, false), afterLogin: LinkCurrentDeviceAsync);
        }

        public void LoginWithDevice()
        {
            client = new Client(connectionData.Scheme, connectionData.Host, connectionData.Port, connectionData.ServerKey, UnityWebRequestAdapter.Instance);
            LoginAsync(client.AuthenticateDeviceAsync(SystemInfo.deviceUniqueIdentifier));
        }

        public void LoginWithCustomId(string customId)
        {
            client = new Client(connectionData.Scheme, connectionData.Host, connectionData.Port, connectionData.ServerKey, UnityWebRequestAdapter.Instance);
            LoginAsync(client.AuthenticateCustomAsync(customId));
        }

        // Links email+password to the current session so the user can login cross-device.
        public async Task LinkEmailAsync(string email, string password)
        {
            await client.LinkEmailAsync(session, email, password);
        }

        // Links the current device ID to this session (called after email login on a new device).
        public async Task LinkCurrentDeviceAsync()
        {
            var udid = GetOrCreateUdid();
            try
            {
                await client.LinkDeviceAsync(session, udid);
                Debug.Log("[NakamaManager] Device linked: " + udid.Substring(0, Mathf.Min(8, udid.Length)) + "...");
            }
            catch (Exception e)
            {
                // May already be linked — not a fatal error
                Debug.LogWarning("[NakamaManager] LinkDevice: " + e.Message);
            }
        }

        private async void LoginAsync(Task<ISession> sessionTask, Func<Task> afterLogin = null)
        {
            onConnecting?.Invoke();
            try
            {
                session = await sessionTask;
                socket = client.NewSocket(true);
                socket.Connected += Connected;
                socket.Closed += Disconnected;
                await socket.ConnectAsync(session);

                if (afterLogin != null)
                    await afterLogin();

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

        public async Task<IApiRpc> SendRPC(string rpc, string payload = "{}")
        {
            if (client == null || session == null)
                return null;

            return await client.RpcAsync(session, rpc, payload);
        }

        #endregion
    }
}
