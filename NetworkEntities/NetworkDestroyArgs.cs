using System;
using OPS.Serialization.Attributes;

namespace PRNet.NetworkEntities {

    [SerializeAbleClass]
    [ClassInheritance(typeof(PlayerDestroyArgs), 0)]
    public class NetworkDestroyArgs {
    }

    [SerializeAbleClass]
    public class PlayerDestroyArgs : NetworkDestroyArgs {

        [SerializeAbleField(0)]
        public NetworkInstanceId slayerId;
        [SerializeAbleField(1)]
        public string killedNotification;
    }
}
