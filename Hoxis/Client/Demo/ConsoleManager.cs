using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DacLib.Generic;
using DacLib.Hoxis;
using DacLib.Hoxis.Client;

using FF = DacLib.Generic.FormatFunc;
using SF = DacLib.Generic.SystemFunc;

namespace DacLib.Hoxis.Client.Demo
{
    public class ConsoleManager : MonoBehaviour
    {
        public int logCapacity = 10;
        public GameObject logItemPrefab;
        public int maxLineCharacter = 100;

        [Header("ProtocolType")]
        public ProtocolType protocolType;
        [Header("Handle")]
        public string request;
        [Header("Error")]
        public string error;
        [Header("HoxisProtocolReceiver")]
        public ReceiverType receiverType;
        public long receiverUID;
        [Header("HoxisProtocolSender")]
        public long senderUID;
        public HoxisAgentID senderAgentID;
        public bool loopback;
        [Header("HoxisProtocolAction")]
        public string method;
        public KVString[] arguments;
        [Header("Description")]
        public string description;

        private GameObject _logPanel;
        private bool _logPanelOn;
        private Vector3 _logPanelOffPosition = new Vector3(0, 731f, 0);
        private Vector3 _logPanelOnPosition = new Vector3(0, 531f, 0);
        private Queue<GameObject> _logQueue;
        private Vector3 _logItemOriginalPosition = new Vector3(0, -400f, 0);

        void Awake()
        {
            _logPanel = transform.Find("log panel").gameObject;
            _logQueue = new Queue<GameObject>(logCapacity);
        }

        // Use this for initialization
        void Start()
        {
            HoxisClient.onInitError += (ret) => { Debug.LogError(ret.desc); Log(ret.desc, LogLevel.Error); };
            HoxisClient.onConnectError += (ret) => { Log(ret.desc, LogLevel.Error); };
            HoxisClient.onConnected += () => { Log(FF.StringFormat("connect to {0}", HoxisClient.serverIP)); };
            HoxisClient.onCloseError += (ret) => { Log(ret.desc, LogLevel.Error); };
            HoxisClient.onNetworkAnomaly += (code, message) => { Log(FF.StringFormat("network anomaly: {0}, {1}", code, message), LogLevel.Error); };
            HoxisDirector.Ins.onResponseError += (err, desc) => { Log(FF.StringFormat("response err: {0}, {1}", err, desc), LogLevel.Error); };
            HoxisDirector.Ins.onProtocolEntry += (proto) => { Log(FF.StringFormat("protocol entry: {0}", FF.ObjectToJson(proto))); };
            HoxisDirector.Ins.onProtocolPost += (proto) => { Log(FF.StringFormat("protocol post: {0}", FF.ObjectToJson(proto))); };
            HoxisDirector.Ins.onAffairConnected += () => { Log(FF.StringFormat("connect to {0}", HoxisClient.serverIP)); };
            HoxisDirector.Ins.onAffairConnectError += (ret) => { Log(ret.desc, LogLevel.Error); };

            _logPanel.GetComponent<RectTransform>().localPosition = _logPanelOffPosition;
            _logPanelOn = false;

            HoxisClient.InitializeConfig();
            HoxisDirector.Ins.AwakeIns();
        }

        void Update()
        {
            _logPanel.GetComponent<RectTransform>().localPosition = Vector3.Lerp(_logPanel.GetComponent<RectTransform>().localPosition, _logPanelOn ? _logPanelOnPosition : _logPanelOffPosition, 20f * Time.deltaTime);
            if (Input.GetKeyDown(KeyCode.BackQuote)) { _logPanelOn = !_logPanelOn; }
        }

        public void Connect() { HoxisClient.Connect(); }
        public void BeginConnect() { HoxisClient.BeginConnnect(); }
        public void Close() { HoxisClient.Close(); }
        public void SignIn() { HoxisDirector.Ins.Request("SignIn", new KVString("uid", "123456789")); }
        public void SignOut() { HoxisDirector.Ins.Request("SignOut"); }
        public void QueryState() { HoxisDirector.Ins.Request("QueryConnectionState", new KVString("uid", "123456789")); }
        public void Reconnect() { HoxisDirector.Ins.Request("Reconnect", new KVString("uid", "123456789")); }
        public void ActivateState() { HoxisDirector.Ins.Request("ActivateConnectionState"); }
        public void DefaultState() { HoxisDirector.Ins.Request("SetDefaultConnectionState"); }
        public void ClearLog() { while (_logQueue.Count > 0) { GameObject item = _logQueue.Dequeue(); Destroy(item); } }
        public void SendProtocol()
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            foreach (KVString kv in arguments) { args.Add(kv.key, kv.val); }
            HoxisProtocol proto = new HoxisProtocol
            {
                type = protocolType,
                handle = FF.ObjectToJson(new ReqHandle { req = method, ts = SF.GetTimeStamp(TimeUnit.Millisecond) }),
                err = error,
                receiver = new HoxisProtocolReceiver { type = receiverType, uid = receiverUID },
                sender = new HoxisProtocolSender { uid = senderUID, aid = senderAgentID, loopback = loopback },
                action = new HoxisProtocolAction(method, new HoxisProtocolArgs(args)),
                desc = description
            };
            HoxisDirector.Ins.ProtocolPost(proto);
        }

        private void Log(string log, LogLevel level = LogLevel.Info)
        {
            GameObject item = Instantiate(logItemPrefab, Vector3.zero, Quaternion.identity, _logPanel.transform);
            item.GetComponent<RectTransform>().localPosition = _logItemOriginalPosition;
            Text t = item.GetComponent<Text>();
            t.text = log;

            foreach (GameObject i in _logQueue)
            {
                if (i == null) continue;
                i.GetComponent<RectTransform>().localPosition += new Vector3(0, 15f, 0);
                i.GetComponent<Text>().color -= new Color(0, 0, 0, 0.065f);
            }


            switch (level)
            {
                case LogLevel.Info:
                    t.color = new Color(1, 1, 1, t.color.a);
                    break;
                case LogLevel.Warning:
                    t.color = new Color(1, 1, 0, t.color.a);
                    break;
                case LogLevel.Error:
                    t.color = new Color(1, 0, 0, t.color.a);
                    break;
            }

            if (_logQueue.Count >= logCapacity)
            {
                GameObject last = _logQueue.Dequeue();
                Destroy(last);
            }
            _logQueue.Enqueue(item);
        }
    }
}