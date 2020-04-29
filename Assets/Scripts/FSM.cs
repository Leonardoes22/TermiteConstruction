using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml.Linq;
using System;
using System.Runtime.InteropServices;

public class FSM
{
    public List<Transition> transitionList = new List<Transition>(); // Lista de transições
    public Dictionary<int, State> statesConteiner = new Dictionary<int, State>(); // Dicionário de estados
    public Dictionary<int, Event> eventsConteiner = new Dictionary<int, Event>(); // Dicionário de eventos

    public State currentState; // Estado atual
    public Coord size;

    // Start is called before the first frame update
    public FSM(string automaton)
    {
        LoadSupervisor(automaton);
    }


    // Carrega arquivo xml com supervisor e gera Estados, Eventos e Transições 
    void LoadSupervisor(string file) {

        // Pega o caminho do arquivo
        var supervisorsfolder = Directory.GetCurrentDirectory() + "\\Assets\\Resources\\Supervisors\\";
        var supPath = supervisorsfolder + file;

        

        //Nó (automata)
        XElement supervisor = XElement.Load(supPath);

        //Obter forma do grid
        string name = (string)supervisor.Element("Automaton").Attribute("name"); // Nome do Automaton
        string[] parse = name.Split(new string[] { "||" }, StringSplitOptions.None); // Parser

        size.x = int.Parse(parse[parse.Length - 2]);
        size.y = int.Parse(parse[parse.Length - 1]);


        //Carregar informações
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

            State stateCaster = new State(stateId, stateName, size);
            if ((string)stateData.Attribute("accepting") == "true") {
                stateCaster.marked = true;
            }

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
    public void CallEvent(int id) {
        TriggerEvent(eventsConteiner[id]);
    }


    //Verifica (e dispara) se há alguma transição disponível para o estado atual dado o evento ocorrido
    void TriggerEvent(Event e) {

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
        currentState = statesConteiner[t.dest];
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
        public bool hasTile; //Robô carregado (alternative)
        int x, y; //Posição no Grid
        public HeightMap heightMap; //Mapa de alturas 

        public bool initial; //Estado inicial?
        public bool marked; //Estado Marcado?

        // Construtor Manual 
        public State(int id, bool carregado, int x, int y, HeightMap heights) {

            this.id = id;
            this.sStr = carregado ? "S1" : "S0";
            this.hasTile = carregado;
            this.x = x;
            this.y = y;
            this.heightMap = heights;

        }

        // Construtor XML
        public State(int id, string name, Coord size) {

            string[] stateInfo = name.Split('.');

            this.id = id;
            this.qStr = stateInfo[0];
            this.sStr = stateInfo[1];
            this.hasTile = sStr == "S1";
            this.x = int.Parse(stateInfo[2]);
            this.y = int.Parse(stateInfo[3]);
            this.heightMap = new HeightMap(size.x, size.y);

            for (int i = 1; i < size.x + 1; i++) {
                for (int j = 1; j < size.y + 1; j++) {
                    int index = 3 + (size.x * (i-1) + j);
                    heightMap[i, j] = int.Parse(stateInfo[index]);
                }
            }
        }

        // Sobrescreve método ToString(), não está final
        public override string ToString() {

            string name = sStr + "." + x + "." + y;

            for (int i = 1; i < heightMap.Shape.x + 1; i++) {
                for (int j = 1; j < heightMap.Shape.y + 1; j++) {
                    name += "." + heightMap[i, j] ;
                }
            }


            return name;
        }

        // Retorna a posição do robô
        public Coord GetPosition() {
            return new Coord(x,y);
        }



    }
}

