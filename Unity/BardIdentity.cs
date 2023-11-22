using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BardNetworking.Unity
{
    public class BardIdentity : MonoBehaviour
    {
        //Client ID of owner. (-1 if the server owns this object)
        public int ownerId = 0;
        
        public bool IsServer()
        { return (ownerId == -1); }
    }
}
