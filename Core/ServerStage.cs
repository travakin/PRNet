using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using PRNet.Exceptions;
using PRNet.NetworkEntities;
using PRNet.Packets;
using PRNet.Requests;
using PRNet.Utils;
using UnityEngine;

namespace PRNet.Core {
    static class ServerStage {

        public static bool active = false;

        public static int EVENT_CLIENTCONNECTED = 0;
        public static int EVENT_CLIENTREADY = 1;
        public static int EVENT_CLIENTDISCONNECTED = 2;

        private static Dictionary<int, List<Action<NetworkConnection, NetworkEventPayload>>> networkEvents = new Dictionary<int, List<Action<NetworkConnection, NetworkEventPayload>>>() {

            { EVENT_CLIENTCONNECTED, new List<Action<NetworkConnection, NetworkEventPayload>>() },
            { EVENT_CLIENTREADY, new List<Action<NetworkConnection, NetworkEventPayload>>() },
            { EVENT_CLIENTDISCONNECTED, new List<Action<NetworkConnection, NetworkEventPayload>>() }
        };

        private static List<SpawnCommand> pendingSpawnCommands = new List<SpawnCommand>();
        private static List<DestroyCommand> pendingDestroyCommands = new List<DestroyCommand>();

        private static NetworkObjectsManager objectsManager = new NetworkObjectsManager(ServerDestroyNoTracking);
        private static Queue<NetworkEventItem> networkEventItems = new Queue<NetworkEventItem>();

        private static PRServer serverObject;

        public static void StartServer(int port) {

			PacketRecorder pr = new PacketRecorder(100, 300000);
            serverObject = new PRServer(port, objectsManager, pr, pr, new NetworkMonitor());

            RpcHandler.Initialize(RegisterMessageEvent);
            RpcHandler.ObjectRegistryCallback callback = FindNetworkEntityWithId;
            RpcHandler.SetObjectRegistryCallback(callback);

            active = true;
        }

        public static void ResetServer() {

            Debug.Log("Resetting server");
            objectsManager.ResetServer();
        }

        public static void Disconnect() {

            serverObject.Stop();
        }

        public static void AddEntityDefinitions(EntityDictionaryEntry[] definitions) {

            objectsManager.entityDefinitions = definitions;
        }

        public static void Ready() {

            NetworkEntity[] staticEntities = GameObject.FindObjectsOfType<NetworkEntity>().Where(entity => entity.staticEntity).ToArray();

            foreach (NetworkEntity staticEntity in staticEntities) {

                if (objectsManager.spawnedEntities.Values.Contains(staticEntity))
                    return;

                Debug.Log("Received valid network entity registration request");

                Transform et = staticEntity.transform;
                staticEntity.instanceId = new NetworkInstanceId((int)(et.position.x * 100) + (int)(et.position.y * 10) + (int)(et.position.z));
                staticEntity.Ready();

                objectsManager.spawnedEntities.Add(staticEntity.instanceId, staticEntity);
            }
        }

        public static void ServerSpawn(NetworkEntity toSpawn, NetworkSpawnArgs args) {

            Debug.Log("Spawning object with no ownership");

            List<int> currentIds = objectsManager.spawnedEntities.Keys.Select(key => key.id).ToList();
            NetworkInstanceId id = new NetworkInstanceId(Identification.GetUniqueIdentifierFromList(currentIds));
            toSpawn.Initialize(-1, id, toSpawn.definitionName, args);

            objectsManager.spawnedEntities.Add(id, toSpawn);

            SpawnCommand spawnCommand = toSpawn.GetSpawnRequest();

            pendingSpawnCommands.Add(spawnCommand);
        }

        public static void ServerSpawn(NetworkEntity toSpawn, NetworkConnection clientConnection, NetworkSpawnArgs args) {

            Debug.Log("Spawning object with ownership");

            List<int> currentIds = objectsManager.spawnedEntities.Keys.Select(key => key.id).ToList();
            NetworkInstanceId id = new NetworkInstanceId(Identification.GetUniqueIdentifierFromList(currentIds));
            toSpawn.Initialize(clientConnection.clientId, id, toSpawn.definitionName, clientConnection, args);

            objectsManager.spawnedEntities.Add(id, toSpawn);

            SpawnCommand spawnCommand = toSpawn.GetSpawnRequest();

            pendingSpawnCommands.Add(spawnCommand);
        }

        public static void ServerDestroy(NetworkEntity toDestroy, NetworkDestroyArgs args) {

            DestroyCommand destroy = new DestroyCommand(toDestroy.instanceId, args);

            objectsManager.spawnedEntities.Remove(toDestroy.instanceId);
            GameObject.Destroy(toDestroy.gameObject);

            pendingDestroyCommands.Add(destroy);
        }

        private static void ServerDestroyNoTracking(NetworkEntity toDestroy, NetworkDestroyArgs args) {

            DestroyCommand destroy = new DestroyCommand(toDestroy.instanceId, args);

            GameObject.Destroy(toDestroy.gameObject);

            pendingDestroyCommands.Add(destroy);
        }

        public static void ClientInitializationUpdate(NetworkConnection conn) {

            List<SpawnCommand> spawnCommands = objectsManager.spawnedEntities.Values.Where(entity => !entity.staticEntity).Select(entity => entity.GetSpawnRequest()).ToList();

            foreach (SpawnCommand spawnCommand in spawnCommands) {

                PacketSpawnCommand command = new PacketSpawnCommand(spawnCommand);

                serverObject.SendClientUpdates(command, conn);
            }

            foreach (NetworkEntity entity in objectsManager.spawnedEntities.Values) {

                entity.Ready();
            }
        }

        public static void ParseStateRequestPacket(Packet request, NetworkConnection clientConnection) {

        }

        public static void Tick() {

            HandleNetworkMessages();
            HandleNetworkEvents();


            if (pendingSpawnCommands.Count > 0)
                SendSpawnCommands();

            if (pendingDestroyCommands.Count > 0)
                SendDestroyCommands();

            serverObject.SendNetworkMessages();
        }

        private static void HandleNetworkMessages() {

            if (objectsManager.pendingMessagesInbound.Count == 0)
                return;

            while (objectsManager.pendingMessagesInbound.Count > 0) {

                NetworkMessage currentMessage = objectsManager.pendingMessagesInbound.Pop();

                if (currentMessage == null)
                    continue;

                if (objectsManager.messageEvents.ContainsKey(currentMessage.type)) {

                    foreach (var msgEvent in objectsManager.messageEvents[currentMessage.type]) {

                        try {

                            msgEvent(currentMessage);
                        }
                        catch (Exception e) {
                        }
                    }   
                }
                else {

                    Debug.Log("Received network message with no available handler events.");
                }
            }
        }

        private static void HandleNetworkEvents() {

            while (networkEventItems.Count > 0) {

                NetworkEventItem item = networkEventItems.Dequeue();
                if (item == null)
                    continue;

                foreach (Action<NetworkConnection, NetworkEventPayload> evt in networkEvents[item.type]) {

                    try {

                        evt(item.conn, item.payload);
                    }
                    catch (Exception e) {

                    }
                }
            }
        }

        private static void SendSpawnCommands() {

            List<SpawnCommand> spawnCommands = new List<SpawnCommand>();
            spawnCommands.AddRange(pendingSpawnCommands);
            pendingSpawnCommands.RemoveRange(0, pendingSpawnCommands.Count);

            foreach (SpawnCommand spawnCommand in spawnCommands) {

                PacketSpawnCommand command = new PacketSpawnCommand();
                command.spawnCommand = spawnCommand;

                serverObject.SendClientUpdates(command);
            }
        }

        private static void SendDestroyCommands() {

            List<DestroyCommand> destroyCommands = new List<DestroyCommand>();
            destroyCommands.AddRange(pendingDestroyCommands);
            pendingDestroyCommands.RemoveRange(0, pendingDestroyCommands.Count);

            foreach (DestroyCommand destroyCommand in destroyCommands) {

                PacketDestroyCommand command = new PacketDestroyCommand();
                command.destroyCommand = destroyCommand;

                serverObject.SendClientUpdates(command);
            }
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

            if (first == second)
                Debug.LogError("Failed to unregister event.");
            else
                Debug.Log("Successfully unregistered event.");
        }

        public static void SubscribeNetworkEvent(int type, Action<NetworkConnection, NetworkEventPayload> evt) {

            networkEvents[type].Add(evt);
        }

        public static void EnqueueNetworkEvent(int type, NetworkConnection conn, NetworkEventPayload payload) {

            NetworkEventItem newItems = new NetworkEventItem();
            newItems.type = type;
            newItems.conn = conn;
            newItems.payload = payload;

            networkEventItems.Enqueue(newItems);
        }

        public static void ParseNetworkMessages(Packet packet, NetworkConnection clientConnection) {

            PacketNetworkMessage messagePacket = (PacketNetworkMessage)packet;

            foreach (var message in messagePacket.networkMessages) {

                message.senderConnection = clientConnection;
                objectsManager.pendingMessagesInbound.Push(message);
            }
        }

        public static void SendNetworkMessage(NetworkMessage msg) {

            objectsManager.pendingMessagesOutbound.Add(msg);
        }

        public static void SendNetworkMessage(NetworkMessage msg, NetworkConnection conn) {

            msg.senderConnection = conn;
            objectsManager.pendingMessagesOutboundTargeted[conn].Add(msg);
        }

        public static void SendHighPriorityNetworkMessage(NetworkMessage msg) {

            objectsManager.pendingMessagesOutboundHP.Add(msg);
        }

        public static void SendHighPriorityNetworkMessage(NetworkMessage msg, NetworkConnection conn) {

            msg.senderConnection = conn;
            objectsManager.pendingMessagesOutboundTargetedHP[conn].Add(msg);
        }

        public static NetworkEntity FindNetworkEntityWithId(NetworkInstanceId id) {

            if (!objectsManager.spawnedEntities.ContainsKey(id)) {

                throw new EntityNotFoundException();
            }

            return objectsManager.spawnedEntities[id];
        }

        public static List<NetworkEntity> FindEntitiesFromConnection(NetworkConnection conn) {

            List<NetworkEntity> entities = new List<NetworkEntity>();

            foreach (NetworkEntity entity in objectsManager.spawnedEntities.Values) {

                if (entity.connectionToClient == conn)
                    entities.Add(entity);
            }

            return entities;
        }

        private class NetworkEventItem {

            public int type;
            public NetworkConnection conn;
            public NetworkEventPayload payload;
        }
    }
}
