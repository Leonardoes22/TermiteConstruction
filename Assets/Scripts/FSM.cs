using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Xml.Linq;
using System.Linq;


public class FSM : MonoBehaviour
{
    List<Transition> transitionList = new List<Transition>(); // Lista de transições
    Dictionary<int, State> statesConteiner = new Dictionary<int, State>(); // Dicionário de estados
    Dictionary<int, Event> eventsConteiner = new Dictionary<int, Event>(); // Dicionário de eventos

    State currentState; // Estado atual

    // Start is called before the first frame update
    void Start()
    {
        LoadSupervisor("supCSync.xml");
        print("Initial: "+ currentState);

        foreach (KeyValuePair<int, State> estado in statesConteiner) {
            print(estado);
        }
    }

    // Update is called once per frame
    void Update()
    {
       

    }

    // Carrega arquivo xml com supervisor e gera Estados, Eventos e Transições 
    void LoadSupervisor(string file) {

        // Pega o caminho do arquivo
        var supervisorsfolder = Directory.GetCurrentDirectory() + "\\Assets\\Resources\\Supervisors\\";
        var supPath = supervisorsfolder + file;

        //Nó (automata)
        XElement supervisor = XElement.Load(supPath);

        IEnumerable events = supervisor.Descendants("Event"); // Enumerável de eventos
        IEnumerable states = supervisor.Descendants("State"); // Enumerável de estados
        IEnumerable transitions = supervisor.Descendants("Transition"); // Enumerável de transições

        //Carrega o dicionário de eventos
        foreach (XElement eventData in events) {

            int eventId = (int) eventData.Attribute("id");
            string eventLabel = (string) eventData.Attribute("label");

            Event eventCaster = new Event(eventId, eventLabel);
            eventsConteiner.Add(eventId, eventCaster);
        }

        //Carrega o dicionário de estados
        foreach (XElement stateData in states) {

            int stateId = (int)stateData.Attribute("id");
            string stateName = (string)stateData.Attribute("name");

            State stateCaster = new State(stateId, stateName);

            if((string)stateData.Attribute("initial") == "true") {
                currentState = stateCaster;
            }

            statesConteiner.Add(stateId, stateCaster);
        }

        //Carrega a lista de transições
        foreach (XElement transitionData in transitions) {

            int dest = (int) transitionData.Attribute("dest");
            int evento = (int) transitionData.Attribute("event");
            int source = (int) transitionData.Attribute("source");

            Transition transitionCaster = new Transition(dest, evento, source);
            transitionList.Add(transitionCaster);
        }


    }



    //Função temporária de apoio
    public void SelectEvent(int id) {
        TriggerEvent(eventsConteiner[id]);
    }


    //Verifica (e dispara) se há alguma transição disponível para o estado atual dado o evento ocorrido
    void TriggerEvent(Event e) {
        print("Triggered:" + e);

        foreach (Transition trans in transitionList) {
            if (e.id == trans.evento) {
                if (trans.source == currentState.id) {
                    FireTransition(trans);
                    break;
                }
            }
        }

    }

    //Aplica a função de transição
    void FireTransition(Transition t) {
        print("Fired:" + t);
        currentState = statesConteiner[t.dest];
        print("State:" + currentState);
    }




    //FSM Classes

    //Event 
    public class Event 
    {

        public int id; 
        public string label; 

        public Event(int id, string label) {
            this.id = id;
            this.label = label;
        }

        // Sobrescreve método ToString(), não está final
        public override string ToString() {
            return "Event [id= " + this.id + ", label= " + this.label +"]";
        }

    }

    //Transition
     public class Transition {

        public int dest; // Estado de destino
        public int evento; // Evento gatilho
        public int source; // Estado habilitador

        public Transition(int d, int e, int s) {

            this.dest = d;
            this.evento = e;
            this.source = s;

        }

        // Sobrescreve método ToString(), não está final
        public override string ToString() {
            return "dest= " + this.dest + ", event= " + this.evento + ", source= " + this.source;
        }
    }

    //State
    public class State {

        public int id; //State ID
        public string name; //State name aka.:condesed info 

        public string qStr; //Why ???
        public string sStr; //Robô carregado?
        public int x, y; //Posição no Grid
        public int[,] heightMap; //Mapa de alturas 

        public bool initial; //Estado inicial?
        public bool marked; //Estado Marcado?

        // Construtor Manual 
        public State(int id, bool carregado, int x, int y, int[,] heights) {

            this.id = id;
            this.sStr = carregado ? "S1" : "S0";
            this.x = x;
            this.y = y;
            this.heightMap = heights;

        }

        // Construtor XML
        public State(int id, string name) {

            int[] size = { 1, 2 }; // Variável temporária, resolve formato do grid

            string[] stateInfo = name.Split('.');

            this.id = id;
            this.qStr = stateInfo[0];
            this.sStr = stateInfo[1];
            this.x = int.Parse(stateInfo[2]);
            this.y = int.Parse(stateInfo[3]);

            this.heightMap = new int[size[0], size[1]];

            for (int i = 0; i < size[0]; i++) {
                for (int j = 0; j < size[1]; j++) {
                    int index = 4 + (size[0] * i + j);
                        heightMap[i, j] = int.Parse(stateInfo[index]);
                }
            }

        }

        // Sobrescreve método ToString(), não está final
        public override string ToString() {
            return sStr + "." + x + "." + y;
        }


    }
}

