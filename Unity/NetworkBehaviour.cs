using BardNetworking.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkBehaviour : MonoBehaviour
{
    public BardIdentity identity;

    private void Awake()
    {
        identity = GetComponent<BardIdentity>();
    }

}
