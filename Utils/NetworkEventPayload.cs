using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PRNet.Utils {

    [Serializable]
    public class NetworkEventPayload {

        [Serializable]
        public class ClientConnectPayload : NetworkEventPayload {

            public string level;
        }

        [Serializable]
        public class ClientDisonnectPayload : NetworkEventPayload {
        }

        [Serializable]
        public class ServerClientConnectPayload : NetworkEventPayload {

            public string username;
            public string playerName;
            public uint playerAccountBalance;
        }

        [Serializable]
        public class ServerClientReadyPayload : NetworkEventPayload {

        }

        [Serializable]
        public class ServerClientDisconnectPayload : NetworkEventPayload {
        }
    }
}