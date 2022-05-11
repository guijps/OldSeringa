using UnityEngine;
using System.Collections;

using System.Collections.Generic;       //Allows us to use Lists. 
using System.Xml;
using System.IO;
using System;
using System.Xml.Linq;

public class GameManager : SingletonGameObject<GameManager>
{
    //####Configuracoes que sao salvas pelo PlayerPrefs
    //MENU
    //  config_toggle_dispositivo_haptico
    //  config_toggle_suporte_fisico
    //  config_toggle_arduino
    //  config_toggle_htcVive
    //  config_toggle_oculusRift

    //SISTEMA
    //  arduinoPortName

    //CALIBRACAO
    //  calibra_cabeca_posicao_rotacao
    //  position_haptic_calibration_x
    //  position_haptic_calibration_y
    //  position_haptic_calibration_z
    //  position_camera_x
    //  position_camera_y
    //  position_camera_z
    //  rotation_camera_x
    //  rotation_camera_y
    //  rotation_camera_z
    //  rotation_camera_w 

    //#######

    public static float[] calibration_position;
    //public float[] head_position;
   // public float[] body_position;
    public static float[] offset_calibration_position;
	public float offsetVertical;
	public float offsetProfundidade;
	public float offsetHorizontal;
    public enum Type {inteiro,flutuante,texto};
    public static bool ligaHaptico = false;
    public static bool ligaARtracking = false;
    public static string identificacaoAluno = "identificacaoTeste";
    public static string arduinoTimeOut = "";
    public static string arduinoPortName = "";
    public static string distancia_embolo_seringa = "100";
    public static string batimentos = "100";
    public static bool usingOculus = true;
    public static bool usingVive = false;
    //Posicao e direcao do olhar
    public static Vector3 eyeTrackingOriginCombinedLocal = new Vector3();
    public static Vector3 eyeTrackingDirectionCombinedLocal = new Vector3();
    public static Vector3 eyeTrackingDirectionLeftLocal = new Vector3();
    public static Vector3 eyeTrackingOriginLeftLocal = new Vector3();
    public static Vector3 eyeTrackingDirectionRightLocal = new Vector3();
    public static Vector3 eyeTrackingOriginRightLocal = new Vector3();

    GameObject player;
    public static XmlDocument xmlConfigDoc;
    public static GameManager manager_instance = null;             
	public bool usingArduino;

    public ConnectToServerSocket conexaoServerSocketAR;

    public static string pathConfig = "Config.xml";

    public string pathStudentsVideos = "C:\\Users\\Pichau\\Desktop\\Videos VIDA Odonto";
	void Awake()
	{
        DontDestroyOnLoad(this.gameObject);
        if (manager_instance == null)
            manager_instance = this;
        offset_calibration_position = new float[] { offsetVertical, offsetProfundidade, offsetHorizontal };
        player = GameObject.FindWithTag("Player");
        arduinoPortName = getConfig("arduinoPortName", Type.texto).ToString();
        //verifica se é para ligar o AR tracking
        ligaARtracking = Convert.ToBoolean(getConfig("config_toggle_ARTracking", GameManager.Type.texto));
        // loadXMLConfig(pathConfig);
    }	

    public static void setIdentificacaoAluno(string identificacao) {
        identificacaoAluno = identificacao;
    }


	public void saveData(){		
		//salva a posicao de calibracao do haptico
        if(calibration_position != null)
        {
            saveConfig("position_haptic_calibration_x", calibration_position[0], Type.flutuante);
            saveConfig("position_haptic_calibration_y", calibration_position[1], Type.flutuante);
            saveConfig("position_haptic_calibration_z", calibration_position[2], Type.flutuante);
           
        }

        if (player != null) {

            saveConfig("position_camera_x", player.transform.position.x, Type.flutuante);
            saveConfig("position_camera_y", player.transform.position.y, Type.flutuante);
            saveConfig("position_camera_z", player.transform.position.z, Type.flutuante);
            saveConfig("rotation_camera_x", player.transform.rotation.x, Type.flutuante);
            saveConfig("rotation_camera_y", player.transform.rotation.y, Type.flutuante);
            saveConfig("rotation_camera_z", player.transform.rotation.z, Type.flutuante);
            saveConfig("rotation_camera_w", player.transform.rotation.w, Type.flutuante);
            print("Salvando Posicao = " + player.transform.position.x + ";" + player.transform.position.y + ";" + player.transform.position.z);
            print("Salvando Rotacao = " + player.transform.rotation.x + ";" + player.transform.rotation.y + ";" + player.transform.rotation.z + ";" + player.transform.rotation.w);
        }
		
		

	} 


    public static void saveConfig(string name, object value, Type type)
    {
        switch (type){
            case Type.flutuante:
                PlayerPrefs.SetFloat(name, (float)value);
                break;
            case Type.inteiro:
                PlayerPrefs.SetInt(name, (int)value);
                break;
            case Type.texto:
                PlayerPrefs.SetString(name, value.ToString());
                break;
        }            
        PlayerPrefs.Save();

        //XML config
        if (xmlConfigDoc == null)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(pathConfig);
            xmlConfigDoc = doc;
        }
        XmlNodeList itemList = xmlConfigDoc.GetElementsByTagName("configs");
        foreach (XmlNode item in itemList)
        {
            print("salvando no config em XML: " + name + " = " + value.ToString());
            item[name].InnerText = value.ToString().Trim();
            xmlConfigDoc.Save(pathConfig);
        }
    }   
    

    public static object getConfig(string name, Type type)
    {


        //XML config
        if (xmlConfigDoc == null)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(pathConfig);
            xmlConfigDoc = doc;
        }
        XmlNodeList itemList = xmlConfigDoc.GetElementsByTagName("configs");
        foreach (XmlNode item in itemList)
        {
            object obj = (object)item[name].InnerText.Trim();
            if (obj != null)
            {
                print("Valor retornado do config em XML: " + name + " = " + obj.ToString());
                return obj;
            }
                         
        }

        //Se nao encontrar a configuracao no XML, pegue-o do PlayerPrefs do Unity.
        switch (type)
        {
            case Type.flutuante:
                return PlayerPrefs.GetFloat(name);
               
            case Type.inteiro:
                return PlayerPrefs.GetInt(name);
               
            case Type.texto:
                return PlayerPrefs.GetString(name);
                
        }

        return null;
    }

    void Update()
	{

	}
}