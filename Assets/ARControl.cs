using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARControl : MonoBehaviour
{
    int coef = 10;
    GameManager manager_instance = GameManager.manager_instance;
    ConnectToServerSocket conectionServerSocket;
    // Start is called before the first frame update
    public void funcaoRemendo()
    {
        
        if (GameManager.manager_instance)
        {
           conectionServerSocket = GameManager.manager_instance.conexaoServerSocketAR;
        }
  
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (conectionServerSocket)
        {
            if (conectionServerSocket.JsonInfo.success)
            {
                conectionServerSocket.step = conectionServerSocket.speed * Time.deltaTime;
                //print("Msg recebida:" + " Tx: " + conectionServerSocket.JsonInfo.translation_x + " ;Ty: " + conectionServerSocket.JsonInfo.translation_y + " ;Tz: " + conectionServerSocket.JsonInfo.translation_z);
                
                transform.localPosition = new Vector3(-conectionServerSocket.JsonInfo.translation_x / coef, -conectionServerSocket.JsonInfo.translation_y / coef, -conectionServerSocket.JsonInfo.translation_z / coef);

                Vector3 up = new Vector3(conectionServerSocket.JsonInfo.rotation_up_x, conectionServerSocket.JsonInfo.rotation_up_y, conectionServerSocket.JsonInfo.rotation_up_z);
                Vector3 forward = new Vector3(conectionServerSocket.JsonInfo.rotation_forward_x, conectionServerSocket.JsonInfo.rotation_forward_y, conectionServerSocket.JsonInfo.rotation_forward_z);
                transform.localRotation = Quaternion.LookRotation(forward, up);

            }
        }
    }
}
