/*Este sketch controla os níveis de porcentagem da seringa de 3 mL para uma
contra-seringa de 10 mL.
Funciona tanto para potenciômetros lineares quanto não lineares.
O estado da última calibragem é gravado na EEPROM do Arduino e portanto
não há necessidade de calibrar toda vez que o liga.
Há também um botão de reset que zera o estado e pede uma nova calibragem.
*/
#include <stdio.h>
#include <stdlib.h>
#include <EEPROM.h>

/*Estrutura que abstrai a seringa*/
struct seri {
  int pot[7];  /*divide a seringa em partes*/
  float nivel[7];
};

/*Definição do tipo de dado abstrato Seringa*/
typedef struct seri Seringa;

/*Instanciação da variável ponteiro seringa (minúsculo) do tipo Seringa (maiúsculo)*/
Seringa *seringa;


/*Declaração de variáveis de escopo global*/ 
double cheio, pot, range, minimo, maximo, top;
unsigned long ultimo;
int nivel, x, reset, comunicacao, potenciaVibra = 252, safeData;
float y;
bool vibrando = false;

/*Grava um inteiro na EEPROM quando passados o endereço de 2 bytes (tamanho do int)*/
void escreveInt(int endereco1, int endereco2, int valor){ // Escreve um inteiro de 2 bytes na EEPROM
  int valorAtual = lerInt(endereco1,endereco2); // Lemos o valor inteiro da memória
  if (valorAtual == valor){ // Se o valor lido for igual ao que queremos escrever não é necessário escrever novamente
    return;
  }
  else{ // Caso contrário "quebramos nosso inteiro em 2 bytes e escrevemos cada byte em uma posição da memória
      byte primeiroByte = valor&0xff; //Executamos a operação AND de 255 com todo o valor, o que mantém apenas o primeiro byte
      byte segundoByte = (valor >> 8) &0xff; // Realizamos um deslocamento de 8 bits para a direita e novamente executamos um AND com o valor 255, o que retorna apenas o byte desejado
      EEPROM.write(endereco1,primeiroByte); // Copiamos o primeiro byte para o endereço 1
      EEPROM.write(endereco2,segundoByte); // Copiamos o segundo byte para o endereço 2
  }
}

/*Resgata o inteiro passando o endereço dos 2 bytes em que está gravado na EEPROM*/
int lerInt(int endereco1, int endereco2){ // Le o int armazenado em dois endereços de memória
  int valor = 0; // Inicializamos nosso retorno
  byte primeiroByte = EEPROM.read(endereco1); // Leitura do primeiro byte armazenado no endereço 1
  byte segundoByte = EEPROM.read(endereco2); // Leitura do segundo byte armazenado no endereço 2
  valor = (segundoByte << 8) + primeiroByte; // Deslocamos o segundo byte 8 vezes para a esquerda ( formando o byte mais significativo ) e realizamos a soma com o primeiro byte ( menos significativo )
  
  return valor; // Retornamos o valor da leitura
}

/*Zera a memória EEPROM colocando o valor 0 em todos os seus campos*/
void zerarEEPROM() {
  int i;
  for (i = 0; i < EEPROM.length(); i++)
    EEPROM.write(i, 0);
}

/*Função responsável por calibrar o nível da seringa*/
void calibrar(Seringa *seringa) {
  int button, x, aux;
  
  for (x = 0, y = 0; x < 7; x++, y += 0.5) {
    seringa->nivel[x] = y;  
    seringa->pot[x] = 0;
  }
  button = x = 0;
  
  while (x < 7) {
    button = digitalRead(13);
    if (button != HIGH && seringa->pot[x] == 0) {
        seringa->pot[x] = analogRead(A0);
        Serial.print("Nivel ");
        Serial.print(seringa->nivel[x]);
        Serial.println(" OK");
        x++;
        if (x < 7) {
          Serial.print("Posicione o embolo na marca ");
          Serial.print(seringa->nivel[x]);
          Serial.println(" mL e aperte o botao amarelo.");
        }
        button = LOW;
    }
    delay(200);
    reset = digitalRead(12);
    if (reset == LOW) {
      for (x = 0; x < 7; x++)
        seringa->pot[x] = 0;
      x = 0;
      button = LOW;
      Serial.println("Calibragem resetada!");
    }
  }
  Serial.println("Seringa calibrada!");  
  for (x = 0; x < 7; x++) {
    Serial.print(seringa->pot[x]);
    if(x < 6)
      Serial.print(", ");
  }
  Serial.print("\n\n");
  for (x = 0; x < 7; x++) {
    escreveInt(2*x+1, 2*x+2, seringa->pot[x]);
  }
  EEPROM.write(0, 1);
}

int nivelar(Seringa *seringa, double pot) {
  int faixa[] = {0, 17, 34, 50, 67, 84, 100};
  int x, newPot;

  for (x = 0; x < 6; x++) {
    if (pot <= seringa->pot[x] && pot >= seringa->pot[x+1]) {
      newPot = map(pot, seringa->pot[x], seringa->pot[x+1], faixa[x], faixa[x+1]);
      //Serial.print(newPot, "cheguei1");
      //Serial.println();
      return newPot;
    }
  }
  return int(newPot);
}


void vibracao(int comunicacao){   /*Trata toda a vibracao comandada no serial*/  
  if (comunicacao == 80){           /*Modo 1: vibra apenas por um pulso(80 = P em ascii (pulse))*/ 
    if(!vibrando){
      analogWrite(11,potenciaVibra);                     
      delay(110);
      analogWrite(11,0);
    }
    else{                                 /*Pulso com vibracao total caso a vibracao continua estiver ativa*/
      analogWrite(11,255);                     
      delay(140);
      analogWrite(11,potenciaVibra);
    }
  }
  else if(comunicacao == 76){       /*Modo 2: vibracao continua com toggle (76 = L em ascii (Liga))*/            
    if(!vibrando){
      analogWrite(11,potenciaVibra);
      vibrando = true;
    }                    
  }
  else if(comunicacao == 68){       /*Modo 2: vibracao continua com toggle (68 = D em ascii (Desliga))*/
    if(vibrando){
      analogWrite(11, 0);
      vibrando = false;
    }
  }
  else if(comunicacao == 48){       /*Desativa a vibracao do motor (recebe o nivel 0 em ascii)*/
    potenciaVibra = 0;
    if(vibrando)                      /*Atualiza a forca da vibracao caso o nivel seja mudado com a vibracao continua ativa*/
      analogWrite(11,potenciaVibra);
  }
  else if(comunicacao >= 49){       /*Regula a forca da vibracao do motor (recebe niveis de 1 a 9 em ascii)*/
    if(comunicacao <= 57){
      potenciaVibra = ((comunicacao - 48)*21 + 66);    /*49 == 1 em ascii,com um nivel minimo de 66+21 para funcionamento correto do vibra (opera bem entre 1 e 3.3V), no maior nivel tem a tensao max = 255*/
      if(vibrando)                    /*Atualiza a forca da vibracao caso o nivel seja mudado com a vibracao continua ativa*/
        analogWrite(11,potenciaVibra);
    }
  }
  else if(comunicacao == 70)      /*Possibilita a aplicacao forcar um serial flush(70 = F em ascii)*/
    Serial.flush();
  else if(comunicacao == 97)
    Serial.println(65);

      /*Nesse modo checa-se o comando de flush, liga e desliga da vibracao continua, pode descartar o outro*/
  while(Serial.available()){                //Limpar o buffer para evitar lagging entre a aplicacao e o vibra  
    safeData = Serial.read();               //Garante que o Toggle, o Serial.flush e a regulagem sempre serao executados
    
  if(safeData == 76){               //Modo 2: vibracao continua com toggle (76 = L em ascii (Liga))        
    if(!vibrando){
      analogWrite(11,potenciaVibra);
      vibrando = true;
    }                    
  }
  else if(safeData == 68){         //Modo 2: vibracao continua com toggle (68 = D em ascii (desliga))
    if(vibrando){
      analogWrite(11, 0);
      vibrando = false;
    }
  }
  else if(safeData == 70)              //Possibilita a aplicacao forcar um serial flush(70 = F em ascii)
    Serial.flush();
  else if(safeData == 97)
    Serial.println('A'); 
       
 }  
/* // Nessa versao checa-se apenas se o comando eh um serial.flush( comunicacao ==70),pode descartar os outros - pode ser usada para melhorar performance (foi imperceptivel nos testes)
  while(Serial.available()){ 
    
      if(Serial.read() == 70)
        Serial.flush();
  }
*/
}

void setup() {
  int x;
  pot = 0;
  minimo = 1023;
  maximo = 0;
  Serial.begin(9600);           //Baud Rate
  pinMode(A0, INPUT);
  pinMode(13, INPUT_PULLUP);
  pinMode(12, INPUT_PULLUP);
  pinMode(11, OUTPUT);          
  seringa = malloc(sizeof(Seringa)); //Alocação de memória para a Seringa
  if (EEPROM.read(0)) {
    for (x = 0; x < 7; x++)
      seringa->pot[x] = lerInt(2*x+1, 2*x+2);
  }
  else
    calibrar(seringa);
}

void loop() {
  reset = digitalRead(12);
  
  if (reset == LOW) {
    Serial.println("Calibragem resetada!");
    zerarEEPROM();
    calibrar(seringa);
  }
  if (Serial.available() > 0){          //leitura serial p/ ativacao do motor
    comunicacao = (Serial.read());        
    vibracao(comunicacao);
  } 
  pot = analogRead(A0);
  
  if (pot < minimo) {
    minimo = pot;  
  }
  if (pot > maximo) {
    maximo = pot;
  }  
 
  range = maximo - minimo;
  cheio = 100 * (pot - minimo) / range;
  nivel = nivelar(seringa, pot); 
  Serial.println(nivel);  
 
  
  delay(60);
}
