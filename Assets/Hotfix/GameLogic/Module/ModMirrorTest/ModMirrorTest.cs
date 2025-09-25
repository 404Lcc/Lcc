using Mirror;
using UnityEngine;

namespace LccHotfix
{
    [Model]
    public class ModMirrorTest : ModelTemplate
    {
        public override void Init()
        {
            base.Init();

            Main.MirrorService.ClientRegisterMessage((int)MessageType.GCTest, OnClientGCTest);
            Main.MirrorService.ServerRegisterMessage((int)MessageType.CGTest, OnServerCGTest);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            Main.MirrorService.ClientUnregisterMessage((int)MessageType.GCTest, OnClientGCTest);
            Main.MirrorService.ServerUnregisterMessage((int)MessageType.CGTest, OnServerCGTest);
        }

        public void ClientSendCGTestInfo()
        {
            CGTestInfo cg = new CGTestInfo();
            cg.id = 1000;
            Main.MirrorService.ClientSendMessage(cg);
        }

        void OnClientGCTest(MessageObject info)
        {
            var gc = info as GCTestInfo;
            Debug.Log("客户端收到消息" + gc.id);
        }

        public void OnServerCGTest(NetworkConnectionToClient client, MessageObject info)
        {
            var cg = info as CGTestInfo;
            Debug.Log("服务器收到消息" + cg.id);

            GCTestInfo gc = new GCTestInfo();
            gc.id = 1001;

            Main.MirrorService.ServerSendMessage(client, gc);
        }
    }
}