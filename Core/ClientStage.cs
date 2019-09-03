using System;
using System.Collections.Generic;
using System.Linq;
using PRNet.Exceptions;
using PRNet.NetworkEntities;
using PRNet.Packets;
using PRNet.Requests;
using PRNet.Utils;
using UnityEngine;

namespace PRNet.Core {
    static class ClientStage {

        public static bool active = false;
        public static int clientId;

        public static int EVENT_CLIENTCONNECTED = 0;
		public static int EVENT_CLIENTDISCONNECTED = 1;

        private static Dictionary<int, List<Action<NetworkEventPayload>>> networkEvents = new Dictionary<int, List<Action<NetworkEventPayload>>>() {

            { EVENT_CLIENTCONNECTED, new List<Action<NetworkEventPayload>>() },
            { EVENT_CLIENTDISCONNECTED, new List<Action<NetworkEventPayload>>() }
        };

        private static List<SpawnCommand> pendingSpawnRequests = new List<SpawnCommand>();
        private static List<DestroyCommand> pendingDestroyCommands = new List<DestroyCommand>();

        private static Queue<NetworkEventItem> networkEventItems = new Queue<NetworkEventItem>();
        private static NetworkObjectsManager objectsManager = new NetworkObjectsManager();

        private static PRClient clientObject;
        private static bool ready = false;

        public static void ConnectClient(string ip, int port) {

            Debug.Log("Connecting client");

			PacketRecorder pr = new PacketRecorder(100, 300000);
            clientObject = new PRClient(objectsManager, pr, pr, new NetworkMonitor());
            clientObject.RequestConnection(ip, port);

            RpcHandler.Initialize(RegisterMessageEvent);
            RpcHandler.ObjectRegistryCallback callback = FindNetworkEntityWithId;
            RpcHandler.SetObjectRegistryCallback(callback);

            active = true;

            Debug.Log("Client Connected");
        }

        public static void ResetClient() {

            Debug.Log("Resetting client");
            objectsManager.ResetClient();
            ready = false;
        }

        public static void DisconnectClient() {

            Debug.Log("Disconnecting client");
            ready = false;
            active = false;
            clientObject.DisconnectClient();
            objectsManager.ResetClient();
        }

        public static void AddEntityDefinitions(EntityDictionaryEntry[] definitions) {

            objectsManager.entityDefinitions = definitions;
        }

        public static void ParseSpawnCommand(Packet packet) {

            PacketSpawnCommand command = (PacketSpawnCommand)packet;

            if (command.spawnCommand != null)
                pendingSpawnRequests.Add(command.spawnCommand);
        }

        public static void ParseDestroyCommand(Packet packet) {

            PacketDestroyCommand command = (PacketDestroyCommand)packet;

            if (command.destroyCommand != null)
                pendingDestroyCommands.Add(command.destroyCommand);
        }

        public static void ParseNetworkMessages(Packet packet) {

            if (!ready) {

                return;
            }

            PacketNetworkMessage messagePacket = (PacketNetworkMessage)packet;

            foreach (var message in messagePacket.networkMessages) {

                objectsManager.pendingMessagesInbound.Push(message);
            }
        }

        public static void Tick() {

            if (clientObject.state != (int)PRClient.States.Connected)
                return;

            SpawnObjects();
            DestroyObjects();

            CallNetworkMessageEvents();
            CallNetworkEvents();

            PacketStateRequest request = BuildStateRequest();
            clientObject.RequestUpdate(request);
        }

        private static void SpawnObjects() {

            List<SpawnCommand> spawnCommands = new List<SpawnCommand>();
            spawnCommands.AddRange(pendingSpawnRequests);
            pendingSpawnRequests.RemoveRange(0, pendingSpawnRequests.Count);

            foreach (SpawnCommand command in spawnCommands) {

                if (command == null || objectsManager.spawnedEntities.ContainsKey(command.id)) {

                    continue;
                }

                NetworkEntity newEntity = objectsManager.entityDefinitions.Where(entity => entity.name == command.name).Single().entity;

                NetworkEntity spawnedEntity = GameObject.Instantiate(newEntity, command.position.Value, command.rotation.Value);
                spawnedEntity.Initialize(command.ownerId, command.id, command.name, command.arguments);
                spawnedEntity.ReceiveSyncVarUpdate(command.syncVarValues);

                objectsManager.spawnedEntities.Add(spawnedEntity.instanceId, spawnedEntity);
            }
        }

        private static void DestroyObjects() {

            List<DestroyCommand> destroyCommands = new List<DestroyCommand>();
            destroyCommands.AddRange(pendingDestroyCommands);
            pendingDestroyCommands.RemoveRange(0, pendingDestroyCommands.Count);

            foreach (DestroyCommand command in destroyCommands) {

                if (command == null || !objectsManager.spawnedEntities.ContainsKey(command.id)) {

                    continue;
                }

                NetworkEntity toDestroy = objectsManager.spawnedEntities[command.id];
                toDestroy.OnNetworkDestroy(command.args);

                objectsManager.spawnedEntities.Remove(command.id);

                GameObject.Destroy(toDestroy.gameObject);
            }
        }

        private static void CallNetworkMessageEvents() {

            while (objectsManager.pendingMessagesInbound.Count > 0) {

                NetworkMessage msg = objectsManager.pendingMessagesInbound.Pop();

                if (msg == null) {

                    continue;
                }

                if (objectsManager.messageEvents.ContainsKey(msg.type)) {

                    foreach (var msgEvent in objectsManager.messageEvents[msg.type]) {

                        try {

                            msgEvent(msg);
                        }
                        catch (Exception e) {
                        }
                    }
                }
            }
        }

        private static void CallNetworkEvents() {

            while (networkEventItems.Count > 0) {

                NetworkEventItem item = networkEventItems.Dequeue();
                if (item == null)
                    continue;

                foreach (Action<NetworkEventPayload> evt in networkEvents[item.type])
                    evt(item.payload);
            }
        }

        private static PacketStateRequest BuildStateRequest() {

            PacketStateRequest request = new PacketStateRequest();

            return request;
        }

        public static void RegisterMessageEvent(int eventKey, Action<NetworkMessage> messageEvent) {

            if (!objectsManager.messageEvents.ContainsKey(eventKey))
                objectsManager.messageEvents.Add(eventKey, new List<Action<NetworkMessage>>());

            objectsManager.messageEvents[eventKey].Add(messageEvent);
        }

        public static void UnRegisterMessageEvent(int eventKey, Action<NetworkMessage> messageEvent) {

            int first = objectsManager.messageEvents[eventKey].Count;
            objectsManager.messageEvents[eventKey].Remove(messageEvent);
            int second = objectsManager.messageEvents[eventKey].Count;
        }

        public static void SubscribeNetworkEvent(int type, Action<NetworkEventPayload> evt) {

            networkEvents[type].Add(evt);
        }

        public static void EnqueueNetworkEvent(int type, NetworkEventPayload payload) {

            NetworkEventItem newItem = new NetworkEventItem();
            newItem.type = type;
            newItem.payload = payload;

            networkEventItems.Enqueue(newItem);
        }

        public static void SendNetworkMessage(NetworkMessage msg) {

            objectsManager.pendingMessagesOutbound.Add(msg);
        }

        public static void SendHighPriorityNetworkMessage(NetworkMessage msg) {

            objectsManager.pendingMessagesOutboundHP.Add(msg);
        }

        public static NetworkEntity FindNetworkEntityWithId(NetworkInstanceId netId) {

            if (!objectsManager.spawnedEntities.ContainsKey(netId)) {

                throw new EntityNotFoundException();
            }

            return objectsManager.spawnedEntities[netId];
        }

        public static void Ready() {

            NetworkEntity[] staticEntities = GameObject.FindObjectsOfType<NetworkEntity>().Where(entity => entity.staticEntity).ToArray();

            foreach (NetworkEntity staticEntity in staticEntities) {

                if (objectsManager.spawnedEntities.Values.Contains(staticEntity)) {

                    continue;
                }

                Transform et = staticEntity.transform;
                staticEntity.instanceId = new NetworkInstanceId((int)(et.position.x * 100) + (int)(et.position.y * 10) + (int)(et.position.z));
                staticEntity.Ready();

                objectsManager.spawnedEntities.Add(staticEntity.instanceId, staticEntity);
            }

            ready = true;
            clientObject.GetStateFromServer();
        }

        private class NetworkEventItem {

            public int type;
            public NetworkEventPayload payload;
        }
    }
}
