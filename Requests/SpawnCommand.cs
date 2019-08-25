using System;
using System.Collections.Generic;
using System.Text;
using PRNet.Utils;
using UnityEngine;
using PRNet.NetworkEntities;
using OPS.Serialization.Attributes;
using static PRNet.Core.NetworkSyncItem;
using PRNet.Core;

namespace PRNet.Requests{
    [Serializable]
    [SerializeAbleClass]
    public class SpawnCommand : IEquatable<SpawnCommand> {

        public SpawnCommand() { }

        public SpawnCommand(int ownerId, NetworkInstanceId id, string name, Vector3 position, Quaternion rotation, NetworkSyncVarValue syncVarValues, NetworkSpawnArgs args) {

            this.id = id;
            this.ownerId = ownerId;
            this.name = name;

            this.position = new NetworkSyncVector3(position);
            this.rotation = new NetworkSyncQuaternion(rotation);

            this.syncVarValues = syncVarValues;
            this.arguments = args;
        }

        [SerializeAbleField(0)]
        public NetworkInstanceId id;
        [SerializeAbleField(1)]
        public int ownerId;
        [SerializeAbleField(2)]
        public string name;
        [SerializeAbleField(3)]
        public NetworkSyncVector3 position;
        [SerializeAbleField(4)]
        public NetworkSyncQuaternion rotation;
        [SerializeAbleField(5)]
        public NetworkSyncVarValue syncVarValues;
        [SerializeAbleField(6)]
        public NetworkSpawnArgs arguments;


        public bool Equals(SpawnCommand other) {

            return id == other.id && name == other.name;
        }
    }
}
