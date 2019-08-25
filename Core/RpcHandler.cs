using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PRNet.Exceptions;
using PRNet.NetworkEntities;
using PRNet.Requests;

namespace PRNet.Core {

    public static class RpcHandler {

        public static int RPC_LOWPRIORITY = 0;
        public static int RPC_HIGHPRIORITY = 1;

        public delegate NetworkEntity ObjectRegistryCallback(NetworkInstanceId id);
        private static ObjectRegistryCallback objectRegistryCallback;

        public static void SetObjectRegistryCallback(ObjectRegistryCallback func) {

            objectRegistryCallback = func;
        }

        public static void Initialize(Action<int, Action<NetworkMessage>> registerEvent) {

            registerEvent(NetworkMessage.RpcInvoke, ParseRpc);
            registerEvent(NetworkMessage.UpdateTransform, ParseNetworkTransfrom);
            registerEvent(NetworkMessage.SyncVarUpdate, ParseSyncVarUpdate);
        }

        private static void ParseRpc(NetworkMessage rpcMsg) {

            NetworkMessage.RpcInvokeMessage invokeMessage = (NetworkMessage.RpcInvokeMessage)rpcMsg;
            NetworkEntity entity = null;

            try {

                entity = objectRegistryCallback(invokeMessage.objectId);
            }
            catch (EntityNotFoundException e) {

                Debug.LogWarning("Unable to invoke RPC on network entity with id " + invokeMessage.objectId.id);
                return;
            }
            finally {

                entity.ReceiveRpc(invokeMessage.functionName, invokeMessage.arguments);
            }
        }

        private static void ParseNetworkTransfrom(NetworkMessage msg) {

            NetworkMessage.UpdateTransformMessage transformMessage = (NetworkMessage.UpdateTransformMessage)msg;

            NetworkEntity entity = null;

            try {
                entity = objectRegistryCallback(transformMessage.objectId);
            }
            catch (EntityNotFoundException e) {

                Debug.LogWarning("Unable to update transform of network entity with id " + transformMessage.objectId.id);
                return;
            }
            finally {

                Debug.Log("In finally");
                NetworkTransform netTransform = entity.GetComponent<NetworkTransform>();

                if (netTransform != null) 
                    netTransform.ReceiveTransformUpdate(transformMessage.relayPosition.Value, transformMessage.relayRotation.Value, transformMessage.childRelayPosition.Value, transformMessage.childRelayRotation.Value);
            }
        }

        private static void ParseSyncVarUpdate(NetworkMessage msg) {

            NetworkMessage.SyncVarUpdateMessage updateMsg = (NetworkMessage.SyncVarUpdateMessage)msg;

            NetworkEntity entity = null;

            try {

                entity = objectRegistryCallback(updateMsg.objectId);
            }
            catch (EntityNotFoundException e) {

                Debug.LogWarning("Unable to update SyncVar of network entity with id " + updateMsg.objectId.id);
                return;
            }
            finally {

                entity.ReceiveSyncVarUpdate(updateMsg.name, updateMsg.value);
            }
        }
    }
}