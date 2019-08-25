using PRNet.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PRNet.Utils {

    public static class Extensions {

        public static NetworkSyncVector3 GetSerializableVector(this Vector3 value) {

            return new NetworkSyncVector3(value);
        }

        public static NetworkSyncQuaternion GetSerializableVector(this Quaternion value) {

            return new NetworkSyncQuaternion(value);
        }
    }
}
