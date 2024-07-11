using UnityEngine;
using Photon.Pun;

public class PlayerManager : MonoBehaviourPun
{
    private static GameObject localPlayerInstance;

    public static GameObject LocalPlayerInstance
    {
        get { return localPlayerInstance; }
    }

    void Awake()
    {
        if (photonView.IsMine)
        {
            localPlayerInstance = this.gameObject;
        }
    }
}
