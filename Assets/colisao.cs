/*Autor:Guilherme Cecato
guilhermececato@usp.br
brief: modela a comunicacao para ativacao do motor na seringa,
 atraves de colisoes e movimentos no unity */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class colisao : MonoBehaviour
{

    [SerializeField]float movimento = 1000f;
    Rigidbody rigidBody;
    

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        RespondtoCommands();  
    }

    private void RespondtoCommands(){                                        //modela a movimentacao
        float movimentoNesseFrame = movimento * Time.deltaTime;
        if(Input.GetKey(KeyCode.A))
            rigidBody.AddRelativeForce(Vector3.back * movimentoNesseFrame);
        
        if(Input.GetKey(KeyCode.D))
            rigidBody.AddRelativeForce(Vector3.forward * movimentoNesseFrame);
        
        rigidBody.velocity = Vector3.zero;
    }

    void OnTriggerEnter(Collider collider){                                  //detecta a insercao da seringa
            //faz a comunicacao
    }       

    void OnTriggerExit(Collider collider){                                  //detecta a retirada da seringa
            //faz a comunicacao
    }     

    void OnTriggerStay(Collider collider){                                 //detecta o movimento da seringa no interior da cabeca
        if(rigidBody.velocity != Vector3.zero){
            //faz a comunicacao
        }
    }
}
