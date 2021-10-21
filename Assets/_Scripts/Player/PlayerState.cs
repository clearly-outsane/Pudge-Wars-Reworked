using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Mirror.Examples.NetworkRoom
{
    public class PlayerState : NetworkBehaviour
    {
        [SyncVar] public int teamNumber;


    }
}