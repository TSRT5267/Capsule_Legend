using UnityEngine;
using Photon.Pun;

public class Player : MonoBehaviourPunCallbacks,IPunObservable
{
    public Rigidbody Rb;
    public PhotonView Pv;
    public MovePlayer Pm;

    private bool isGround;
    private Vector3 curPos;

    

    private void Update()
    {
        isGround = Pm.GetComponent<MovePlayer>().IsGround;
    }




    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
    }

   
}
