/**
 * Ardity (Serial Communication for Arduino + Unity)
 * Author: Daniel Wilches <dwilches@gmail.com>
 *
 * This work is released under the Creative Commons Attributions license.
 * https://creativecommons.org/licenses/by/2.0/
 */

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.IO;
using System.Management;




/**
 * This class allows a Unity program to continually check for messages from a
 * serial device.
 *
 * It creates a Thread that communicates with the serial port and continually
 * polls the messages on the wire.
 * That Thread puts all the messages inside a Queue, and this SerialController
 * class polls that queue by means of invoking SerialThread.GetSerialMessage().
 *
 * The serial device must send its messages separated by a newline character.
 * Neither the SerialController nor the SerialThread perform any validation
 * on the integrity of the message. It's up to the one that makes sense of the
 * data.
 */
public class SerialController : MonoBehaviour
{

    [SerializeField]float movimento = 1000f;                //velocidade da seringa 
    [SerializeField]float deslocamentoMinimo = 1f;          //determina deslocamento minimo para ativacao do vibracall quando a seringa esta inserida
    bool seringaDentro = false;
    bool isConnect = false;
    bool portaAberta = false;
    Vector3 posicao;
    float diferenca;
    //bool novaPosicao = true;                                //evita que a co-rotina seja chamada indiscriminadamente
    [SerializeField]
    Transform embolo;
    [SerializeField]
    Transform fimEmbolo;
    [SerializeField]
    Transform inicioEmbolo;
    Vector3 positionInput;
    //public Transform camPivot;
    //float heading = 0;
    //public Transform cam;
    float velocidadeMouse = 180f;
    

    //float ultimaPosicao;
    Vector3 ultimaPosicao;
    Vector3 distEmbolo;
    
    private Vector3 distEmboloInit;
    [Tooltip("Port name with which the SerialPort object will be created.")]
    public string portName = "COM3";
    string[] portNames = SerialPort.GetPortNames(); 
    //string[] portNamess = {"COM1","COM2","COM3","COM4","COM5"};

    [Tooltip("Baud rate that the serial device is using to transmit data.")]
    public int baudRate = 9600;

    [Tooltip("Reference to an scene object that will receive the events of connection, " +
             "disconnection and the messages from the serial device.")]
    public GameObject messageListener;

    [Tooltip("After an error in the serial communication, or an unsuccessful " +
             "connect, how many milliseconds we should wait.")]
    public int reconnectionDelay = 1000;

    [Tooltip("Maximum number of unread data messages in the queue. " +
             "New messages will be discarded.")]
    public int maxUnreadMessages = 1;

    // Constants used to mark the start and end of a connection. There is no
    // way you can generate clashing messages from your serial device, as I
    // compare the references of these strings, no their contents. So if you
    // send these same strings from the serial device, upon reconstruction they
    // will have different reference ids.
    public const string SERIAL_DEVICE_CONNECTED = "__Connected__";
    public const string SERIAL_DEVICE_DISCONNECTED = "__Disconnected__";

    // Internal reference to the Thread and the object that runs in it.
    protected Thread thread;
    protected SerialThreadLines serialThread;


    // ------------------------------------------------------------------------
    // Invoked whenever the SerialController gameobject is activated.
    // It creates a new thread that tries to connect to the serial device
    // and start reading from it.
    // ------------------------------------------------------------------------
    void Start()
    {
        distEmbolo = (fimEmbolo.position - embolo.position).normalized;
        distEmboloInit = (fimEmbolo.position - embolo.position);
        
    }
    void SlideEmbolo(int leitura)
    {
        if (leitura > 100)
        {
            print("favor, recalibrar a seringa");
            leitura = 100;
        }
        //0 está para o fim assim como o máximo está para o inicio 
        embolo.position= (-fimEmbolo.position + inicioEmbolo.position)*leitura / 100 + fimEmbolo.position;

    }
    void OnEnable()
    {
       string  port = "COM6";
        var serialPort = new SerialPort(port, 9600, 0, 8, StopBits.One);
        serialPort.ReadTimeout = 100;
        serialPort.Open();
   
        
        portaAberta = true;
        portName = port;
       

    }

    // ------------------------------------------------------------------------
    // Invoked whenever the SerialController gameobject is deactivated.
    // It stops and destroys the thread that was reading from the serial device.
    // ------------------------------------------------------------------------
    void OnDisable()
    {
        // If there is a user-defined tear-down function, execute it before
        // closing the underlying COM port.
        if (userDefinedTearDownFunction != null)
            userDefinedTearDownFunction();

        // The serialThread reference should never be null at this point,
        // unless an Exception happened in the OnEnable(), in which case I've
        // no idea what face Unity will make.
        if (serialThread != null)
        {
            serialThread.RequestStop();
            serialThread = null;
        }

        // This reference shouldn't be null at this point anyway.
        if (thread != null)
        {
            thread.Join();
            thread = null;
        }
    }

    // ------------------------------------------------------------------------
    // Polls messages from the queue that the SerialThread object keeps. Once a
    // message has been polled it is removed from the queue. There are some
    // special messages that mark the start/end of the communication with the
    // device.
    // ------------------------------------------------------------------------
    void Update()
    {
       
        
       // RespondtoMovementCommands();
        MovementInputs();


    
        if(portaAberta){
            serialThread = new SerialThreadLines(portName, 
                                                 baudRate, 
                                                 reconnectionDelay,
                                                 maxUnreadMessages);
            thread = new Thread(new ThreadStart(serialThread.RunForever));
            thread.Start();
            portaAberta = false;
            isConnect = true;
        }

        if(portaAberta || isConnect){
            RespondtoCommands();
        
        
            if(seringaDentro){
                //if((Mathf.Abs(ultimaPosicao - transform.position.magnitude)*100) > deslocamentoMinimo){             //com a seringa dentro, checa se foi deslocada num valor 
            
                if((Mathf.Abs((ultimaPosicao - transform.position).magnitude)*100) > deslocamentoMinimo){             //com a seringa dentro, checa se foi deslocada num valor 
                    if(seringaDentro){                                                                              //razoavel e manda a mensagem
                        SendSerialMessage("P");
                        //ultimaPosicao = transform.position.magnitude;
                        ultimaPosicao = transform.position;
                    }
                }
            }
        
            // If the user prefers to poll the messages instead of receiving them
            // via SendMessage, then the message listener should be null.
            if (messageListener == null)
                return;

            // Read the next message from the queue
            string message = (string)serialThread.ReadMessage();
           
            if (message == null)
                return;
            UnityEngine.Debug.Log("msg: " + message);
            bool sucess = false;
            int leituraInt=0;
            try
            {
                leituraInt = (int)Convert.ToDouble(message);
                UnityEngine.Debug.Log("msg: "+leituraInt);
                sucess = true;
            }catch(Exception e)
            {
                sucess = false;
            }
            if(sucess)
                SlideEmbolo(leituraInt);

            // Check if the message is plain data or a connect/disconnect event.
            if (ReferenceEquals(message, SERIAL_DEVICE_CONNECTED))
                messageListener.SendMessage("OnConnectionEvent", true);
            else if (ReferenceEquals(message, SERIAL_DEVICE_DISCONNECTED))
                messageListener.SendMessage("OnConnectionEvent", false);
            else
                messageListener.SendMessage("OnMessageArrived", message);
        }
        /*
        if(!isConnect){
           
                foreach(string port in portNames){
                    portName = port;
                    //Debug.Log(portName);
                    SendSerialMessage("a");
                    //Debug.Log(isConnect);
                    //Debug.Log(message);
                    if(message == "A"){
                       isConnect = true;
                        break;
                    }
                }
           
        } */
        
    }

    private void MovementInputs(){
       // heading += Input.GetAxis("Mouse X")*Time.deltaTime*velocidadeMouse;
        //camPivot.rotation = Quaternion.Euler(0, heading, 0);

        
        positionInput = new Vector3(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical"),Input.GetAxis("Mouse ScrollWheel"));
        positionInput = Vector3.ClampMagnitude(positionInput, 1);

        //Vector3 camF = cam.forward;
        //Vector3 camR = cam.right;

        //camF = camF.normalized;
        //camR = camR.normalized;

        transform.position += new Vector3(positionInput.x, positionInput.y, positionInput.z*30)*Time.deltaTime*movimento;
        //transform.position += (camF*positionInput.y + camR*positionInput.x)*Time.deltaTime;
    }



/*
    private void RespondtoMovementCommands(){                                 //modela o movimento
        float movimentoNesseFrame = movimento * Time.deltaTime;
        if(Input.GetKey(KeyCode.A))
            this.transform.Translate(Vector3.back * movimentoNesseFrame);
        if(Input.GetKey(KeyCode.D))
            this.transform.Translate(Vector3.forward * movimentoNesseFrame);
    }
*/

    //Bloco feito para configurar a potencia do vibra do arduino (niveis de 0 a 9)
    private void RespondtoCommands(){
        if(isConnect){
            if(Input.GetKeyDown(KeyCode.Alpha0))
                SendSerialMessage("0");
            if(Input.GetKeyDown(KeyCode.Alpha1))
                SendSerialMessage("1");
            if(Input.GetKeyDown(KeyCode.Alpha2))
                SendSerialMessage("2");
            if(Input.GetKeyDown(KeyCode.Alpha3))
                SendSerialMessage("3");
            if(Input.GetKeyDown(KeyCode.Alpha4))
                SendSerialMessage("4");
            if(Input.GetKeyDown(KeyCode.Alpha5))
                SendSerialMessage("5");
            if(Input.GetKeyDown(KeyCode.Alpha6))
                SendSerialMessage("6");
            if(Input.GetKeyDown(KeyCode.Alpha7))
                SendSerialMessage("7");
            if(Input.GetKeyDown(KeyCode.Alpha8))
                SendSerialMessage("8");
            if(Input.GetKeyDown(KeyCode.Alpha9))
                SendSerialMessage("9");


            //Bloco feito para simular o uso da vibracao continua do vibra (a principio nao utilizado no vibra)
            if (Input.GetKeyDown(KeyCode.Z))
            {
                UnityEngine.Debug.Log("send L");
                SendSerialMessage("L");
            }
            if (Input.GetKeyDown(KeyCode.X))
            {
                UnityEngine.Debug.Log("send D");
                SendSerialMessage("D");
            }
                
        }

    }

    void OnTriggerEnter(Collider collider){                                  //detecta a insercao da seringa
        try{
            SendSerialMessage("P");
            print("entrei ack");
            seringaDentro = true;
            //ultimaPosicao = transform.position.magnitude;
            ultimaPosicao = transform.position;
        }
        catch(System.Exception){}

    }       

    void OnTriggerExit(Collider collider){                                  //detecta a retirada da seringa
        try{
            print("Sai ack");
            SendSerialMessage("P");
            seringaDentro = false;
        }
        catch(System.Exception){}
    }     


    






    // ------------------------------------------------------------------------
    // Returns a new unread message from the serial device. You only need to
    // call this if you don't provide a message listener.
    // ------------------------------------------------------------------------
    public string ReadSerialMessage()
    {
        // Read the next message from the queue
        return (string)serialThread.ReadMessage();
    }

    // ------------------------------------------------------------------------
    // Puts a message in the outgoing queue. The thread object will send the
    // message to the serial device when it considers it's appropriate.
    // ------------------------------------------------------------------------
    public void SendSerialMessage(string message)
    {
        serialThread.SendMessage(message);
    }

    // ------------------------------------------------------------------------
    // Executes a user-defined function before Unity closes the COM port, so
    // the user can send some tear-down message to the hardware reliably.
    // ------------------------------------------------------------------------
    public delegate void TearDownFunction();
    private TearDownFunction userDefinedTearDownFunction;
    public void SetTearDownFunction(TearDownFunction userFunction)
    {
        this.userDefinedTearDownFunction = userFunction;
    }

}




/*
        if(seringaDentro){
            if(novaPosicao)
                StartCoroutine(ColisaoCoroutine());
        }
        */

/*
    IEnumerator ColisaoCoroutine(){
        posicao = transform.position;                       //salva a posicao da seringa antes da pausa
        novaPosicao = false;

        yield return new WaitForSeconds(0.45f);              //pausa para captar diferenca do posicionamento da seringa e desacelerar as chamas da funcao
        
        diferenca = transform.position.x - posicao.x;
        novaPosicao = true;
        if(Mathf.Abs(diferenca*100) > deslocamentoMinimo)   //modela a diferenca minima de distancia, pode ser ajustada no fator ou no SerialField
            SendSerialMessage("2");
    }
*/