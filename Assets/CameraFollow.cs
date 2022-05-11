/*
Esse script linka a camera na seringa
Autor: Guilherme Cecato - guilhermececato@usp.br
Data: 01/03/2021
*/
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    public float smoothSpeed = 0.125f;
    public Vector3 offset;

    

    void FixedUpdate(){
        Vector3 desiredPosition = target.position + offset;
        

        //movimento liso opcao 1
        Vector3 smoothedPosition = Vector3.Lerp(transform.position,desiredPosition, smoothSpeed*Time.deltaTime);
        transform.position = smoothedPosition;
         // caso queira movimentacao direta opcao 2
        //transform.position = desiredPosition;


        //transform.LookAt(target);  
    } 


}
