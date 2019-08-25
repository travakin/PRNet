using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OPS.Serialization.Attributes;

namespace PRNet.NetworkEntities {

    [Serializable]
    [SerializeAbleClass]
    public class NetworkInstanceId : IEquatable<NetworkInstanceId> {

        public static NetworkInstanceId Invalid { get { return new NetworkInstanceId(-1); } }

        [SerializeAbleField(0)]
        public int id;

        public NetworkInstanceId() { }

        public NetworkInstanceId(int id) {

            this.id = id;
        }

        public override int GetHashCode() {

            return id.GetHashCode();
        }

        public bool Equals(NetworkInstanceId other) {

            return this.id == other.id;
        }

        public static bool operator ==(NetworkInstanceId id1, NetworkInstanceId id2) {

            if (ReferenceEquals(id1, null)) {

                return ReferenceEquals(id2, null);
            }

            if (ReferenceEquals(id2, null)) {

                return ReferenceEquals(id1, null);
            }

            return id1.id == id2.id;
        }

        public static bool operator !=(NetworkInstanceId id1, NetworkInstanceId id2) {

            return !(id1 == id2);
        }
    }
}