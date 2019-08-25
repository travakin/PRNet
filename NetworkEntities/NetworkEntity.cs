using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PRNet.Core;
using PRNet.Requests;
using UnityEngine;

namespace PRNet.NetworkEntities {
    public class NetworkEntity : MonoBehaviour, IEquatable<NetworkEntity> {

        public NetworkInstanceId instanceId;
        public NetworkConnection connectionToClient;
        public int ownerId = -1;
        public string definitionName;
        public bool staticEntity = false;

        private Vector3 _position;
        private Quaternion _rotation;
        private NetworkSpawnArgs _args;

        private Dictionary<string, RpcMeta> rpcDictionary = new Dictionary<string, RpcMeta>();
        private Dictionary<string, SyncVar> syncVarDictionary = new Dictionary<string, SyncVar>();

        private List<Action> readyEvents = new List<Action>();
        private List<Action<NetworkSpawnArgs>> initializationEvents = new List<Action<NetworkSpawnArgs>>();
        private List<Action<NetworkDestroyArgs>> destroyEvents = new List<Action<NetworkDestroyArgs>>();

        private void Update() {

            _position = transform.position;
            _rotation = transform.rotation;

            if (ServerStage.active) {

                foreach (SyncVar var in syncVarDictionary.Values) {

                    if (var.IsChanged()) {

                        SendVariableUpdate(var);
                        var.Equalize();
                    }
                }
            }
        }

        public bool Equals(NetworkEntity other) {

            return this.instanceId == other.instanceId;
        }

        public void Ready() {

            readyEvents.ForEach(evt => evt());
        }

        public void Initialize(int ownerId, NetworkInstanceId id, string definitionName, NetworkSpawnArgs args) {

            this.ownerId = ownerId;
            this.instanceId = id;
            this.definitionName = definitionName;
            this._args = args;

            Debug.Log(ownerId);

            StartCoroutine(CallSpawnEvents(args));
        }

        public void Initialize(int ownerId, NetworkInstanceId id, string definitionName, NetworkConnection conn, NetworkSpawnArgs args) {

            this.ownerId = ownerId;
            this.instanceId = id;
            this.definitionName = definitionName;
            this.connectionToClient = conn;
            this._args = args;

            Debug.Log(ownerId);

            StartCoroutine(CallSpawnEvents(args));
        }

        public NetworkSyncVector3 GetPositionSerializable() {

            return new NetworkSyncVector3(_position);
        }

        public NetworkSyncQuaternion GetRotationSerializable() {

            return new NetworkSyncQuaternion(_rotation);
        }

        public SpawnCommand GetSpawnRequest() {

            List<string> varNames = GetSyncVarNames();
            List<NetworkSyncItem> varValues = GetSyncVarValues(varNames);

            NetworkSyncVarValue values = new NetworkSyncVarValue(varNames, varValues);
            return new SpawnCommand(ownerId, instanceId, definitionName, _position, _rotation, values, _args);
        }

        private List<string> GetSyncVarNames() {

            return syncVarDictionary.Keys.ToList();
        }

        private List<NetworkSyncItem> GetSyncVarValues(List<string> names) {

            List<NetworkSyncItem> values = new List<NetworkSyncItem>();

            names.ForEach(name => values.Add(syncVarDictionary[name].currentItem));

            return values;
        }

        public bool IsLocalObject() {

            if (!ClientStage.active)
                return false;

            return this.ownerId == ClientStage.clientId;
        }

        public void RegisterRpc(Type type, PRNetworkBehaviour component, string methodName) {

            RpcMeta meta = new RpcMeta();
            meta.baseType = type;
            meta.behaviourScript = component;

            rpcDictionary.Add(methodName, meta);
        }

        public void InvokeRpc(string methodName, RpcArgs args, int priority) {

            NetworkMessage.RpcInvokeMessage rpcDef = new NetworkMessage.RpcInvokeMessage(instanceId, priority, methodName, args);

            if (ClientStage.active)
                ClientStage.SendNetworkMessage(rpcDef);

            if (ServerStage.active)
                ServerStage.SendNetworkMessage(rpcDef);
        }

        public void InvokeRpc(NetworkConnection conn, string methodName, RpcArgs args, int priority) {

            NetworkMessage.RpcInvokeMessage rpcDef = new NetworkMessage.RpcInvokeMessage(instanceId, priority, methodName, args);

            if (ServerStage.active)
                ServerStage.SendNetworkMessage(rpcDef, conn);
        }

        public void ReceiveRpc(string methodName, RpcArgs args) {

            if (!rpcDictionary.ContainsKey(methodName)) {

                Debug.LogWarning("Rpc function not found.");
                return;
            }

            RpcMeta functionMeta = rpcDictionary[methodName];

            Type[] types = args.types.Select(t => Type.GetType(t)).ToArray();

            MethodInfo method = functionMeta.baseType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic, null, types, null);

            method.Invoke(functionMeta.behaviourScript, args.values);
        }

        public void RegisterSyncVar(SyncVar var) {

            syncVarDictionary.Add(var.VariableName, var);
        }


        public void ReceiveSyncVarUpdate(NetworkSyncVarValue values) {

            StartCoroutine(InitializeSyncVars(values));
        }

        public void ReceiveSyncVarUpdate(string name, NetworkSyncItem value) {

            Debug.Log("Received syncvar update for variable " + name);
            syncVarDictionary[name].Assign(value);
        }

        public void RegisterNetworkReadyEvent(Action evt) {

            readyEvents.Add(evt);
        }

        public void RegisterInitializationEvent(Action<NetworkSpawnArgs> evt) {

            initializationEvents.Add(evt);
        }

        public void SendVariableUpdate(SyncVar var) {

            if (!ServerStage.active)
                return;

            NetworkMessage.SyncVarUpdateMessage msg = new NetworkMessage.SyncVarUpdateMessage(instanceId, var.VariableName, var.currentItem);
            ServerStage.SendHighPriorityNetworkMessage(msg);
        }

        private IEnumerator CallSpawnEvents(NetworkSpawnArgs args) {

            yield return null;

            foreach (var evt in initializationEvents) {

                evt(args);
            }
        }

        private IEnumerator InitializeSyncVars(NetworkSyncVarValue values) {

            yield return null;

            for (int i = 0; i < values.Names.Count; i++) {

                string name = values.Names[i];
                NetworkSyncItem value = values.Value[i];

                Debug.Log("Received syncvar update for variable " + name);

                syncVarDictionary[name].Assign(value);
            }
        }

        public void RegisterOnDestroyEvent(Action<NetworkDestroyArgs> evt) {

            destroyEvents.Add(evt);
        }

        public void OnNetworkDestroy(NetworkDestroyArgs args) {

            foreach (Action<NetworkDestroyArgs> evt in destroyEvents) {

                evt(args);
            }
        }

        private struct RpcMeta {

            public Type baseType;
            public PRNetworkBehaviour behaviourScript;
        }
    }
}
