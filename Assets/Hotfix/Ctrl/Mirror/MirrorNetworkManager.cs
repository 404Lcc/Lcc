using System.Reflection;
using LccModel;
using Mirror;

public class MirrorNetworkManager : NetworkManager
{
    public override void OnStartServer()
    {
        base.OnStartServer();

        var obj = HotfixFunc.CallProperty("LccHotfix", "Main", "MirrorService", BindingFlags.Static | BindingFlags.Public);
        HotfixFunc.CallMethod("LccHotfix", "MirrorManager", "OnServerStart", BindingFlags.NonPublic | BindingFlags.Instance, new object[] { }, obj);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        var obj = HotfixFunc.CallProperty("LccHotfix", "Main", "MirrorService", BindingFlags.Static | BindingFlags.Public);
        HotfixFunc.CallMethod("LccHotfix", "MirrorManager", "OnServerStop", BindingFlags.NonPublic | BindingFlags.Instance, new object[] { }, obj);
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();

        var obj = HotfixFunc.CallProperty("LccHotfix", "Main", "MirrorService", BindingFlags.Static | BindingFlags.Public);
        HotfixFunc.CallMethod("LccHotfix", "MirrorManager", "OnClientConnected", BindingFlags.NonPublic | BindingFlags.Instance, new object[] { }, obj);
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();

        var obj = HotfixFunc.CallProperty("LccHotfix", "Main", "MirrorService", BindingFlags.Static | BindingFlags.Public);
        HotfixFunc.CallMethod("LccHotfix", "MirrorManager", "OnClientDisconnected", BindingFlags.NonPublic | BindingFlags.Instance, new object[] { }, obj);
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);

        var obj = HotfixFunc.CallProperty("LccHotfix", "Main", "MirrorService", BindingFlags.Static | BindingFlags.Public);
        HotfixFunc.CallMethod("LccHotfix", "MirrorManager", "OnServerRemoteClientDisconnected", BindingFlags.NonPublic | BindingFlags.Instance, new object[] { conn.connectionId }, obj);
    }
}