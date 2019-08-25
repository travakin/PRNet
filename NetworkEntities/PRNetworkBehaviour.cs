using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PRNet.NetworkEntities {

    [RequireComponent(typeof(NetworkEntity))]
    public abstract class PRNetworkBehaviour : MonoBehaviour {

        public NetworkEntity myNetworkEntity;

        public NetworkInstanceId NetId { get { return myNetworkEntity.instanceId; } }

        // Start is called before the first frame update
        void Awake() {

            //Debug.Log("Assigning network entity");
            myNetworkEntity = GetComponent<NetworkEntity>();
        }
    }
}