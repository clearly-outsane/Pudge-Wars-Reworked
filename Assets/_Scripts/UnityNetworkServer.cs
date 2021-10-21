using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.Examples.NetworkRoom;
using UnityEngine;

public class UnityNetworkServer : NetworkRoomManagerExt
{

    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);
    }

 
}
