namespace PRNet.Utils {

    public interface INetworkMonitor {

        void AddBytesInbound(int bytes);
        void AddPacketsInbound(int packets);
        void AddBytesOutbound(int bytes);
        void AddPacketsOutbound(int packets);
        int GetBytesInbound();
        int GetPacketsInbound();
        int GetBytesOutbound();
        int GetPacketsOutbound();
        void ClearData();
    }
}