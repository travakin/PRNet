using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using PRNet.Core;
using PRNet.NetworkEntities;
using PRNet.Requests;
using PRNet.Utils.Collections;

namespace PRNet.Utils {

    public class NetworkObjectsManager {

        public EntityDictionaryEntry[] entityDefinitions;
        public Dictionary<NetworkInstanceId, NetworkEntity> spawnedEntities = new Dictionary<NetworkInstanceId, NetworkEntity>();

        public Dictionary<int, List<Action<NetworkMessage>>> messageEvents = new Dictionary<int, List<Action<NetworkMessage>>>();
        public PRNetQueue<NetworkMessage> pendingMessagesInbound = new PRNetQueue<NetworkMessage>();
        public List<NetworkMessage> pendingMessagesOutbound = new List<NetworkMessage>();
        public Dictionary<NetworkConnection, List<NetworkMessage>> pendingMessagesOutboundTargeted = new Dictionary<NetworkConnection, List<NetworkMessage>>();
        public List<NetworkMessage> pendingMessagesOutboundHP = new List<NetworkMessage>();
        public Dictionary<NetworkConnection, List<NetworkMessage>> pendingMessagesOutboundTargetedHP = new Dictionary<NetworkConnection, List<NetworkMessage>>();

        private Action<NetworkEntity, NetworkDestroyArgs> serverDestroy;

        public NetworkObjectsManager() {

        }

        public NetworkObjectsManager(Action<NetworkEntity, NetworkDestroyArgs> destroyFunc) {

            serverDestroy = destroyFunc;
        }

        public void ResetClient() {

            foreach (NetworkEntity entity in spawnedEntities.Values) {

                GameObject.Destroy(entity.gameObject);
            }
            spawnedEntities.Clear();
        }

        public void ResetServer() {

            foreach (KeyValuePair<NetworkInstanceId, NetworkEntity> entry in spawnedEntities) {

                serverDestroy(entry.Value, new NetworkDestroyArgs());
            }

            spawnedEntities.Clear();
        }
    }
}