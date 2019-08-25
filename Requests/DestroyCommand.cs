using System;
using PRNet.NetworkEntities;
using OPS.Serialization.Attributes;

[Serializable]
[SerializeAbleClass]
public class DestroyCommand {

    [SerializeAbleField(0)]
    public NetworkInstanceId id;
    [SerializeAbleField(1)]
    public NetworkDestroyArgs args;

    public DestroyCommand() { }

    public DestroyCommand(NetworkInstanceId id, NetworkDestroyArgs args) {

        this.id = id;
        this.args = args;
    }
}
