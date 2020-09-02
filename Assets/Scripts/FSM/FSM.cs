using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml.Linq;
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;

public class FSM
{
    public List<Transition> transitionList = new List<Transition>(); // Lista de transições
    public Dictionary<int, State> statesConteiner = new Dictionary<int, State>(); // Dicionário de estados
    public Dictionary<int, Event> eventsConteiner = new Dictionary<int, Event>(); // Dicionário de eventos

    public State currentState; // Estado atual
    public Coord size;

    public FSM(Supervisor sup, string customInit=null) {

        eventsConteiner = sup.eventsContainer;
        statesConteiner = sup.statesContainer;
        transitionList = sup.transitionsList;


        
        if(customInit != null) {

            foreach (var state in statesConteiner.Values) {

                if (state.name == customInit) {
                    currentState = state;
                    break;
                }
                
            }

        }

        if(currentState == null) {
            currentState = sup.initialState;
        }


        

    }


    // Internal Utilities Methods -------------------

    //Função temporária de apoio
    public void CallEvent(int id) {
        TriggerEvent(eventsConteiner[id]);
    }

    //Returns the R2_event of input (if non existent returns self)
    public Event AltEvent(Event _event) {

        string label = _event.label + "_r2";

        foreach (var e in eventsConteiner.Values) {
            if(e.label == label) {
                return e;
            }
        }
        return null;
    }

    //Verifica (e dispara) se há alguma transição disponível para o estado atual dado o evento ocorrido
    public void TriggerEvent(Event e, bool alt=false) {

        e = alt ? AltEvent(e) : e;
        
        if(e != null) {
            currentState = ImagineEvent(e);
        } 
        

    }

    public State ImagineEvent(Event e, State source=null) {

        source = (source == null) ? currentState : source;

        foreach (Transition trans in transitionList) {
            if (e.id == trans.evento) {
                if (trans.source == source.id) {
                    return statesConteiner[trans.dest];
                }
            }
        }

        return null;

    }
    public State ImagineEvent(int eID) {
        return ImagineEvent(eventsConteiner[eID]);
    }

    // Returns all feasible events
    public List<Event> FeasibleEvents() {
        return FeasibleEvents(currentState);
    }
    public List<Event> FeasibleEvents(State s, bool local=false) {

        List<Event> feasible = new List<Event>();


        foreach (Transition trans in transitionList) {
            if(s.id == trans.source) {

                if (!eventsConteiner[trans.evento].label.EndsWith("_r2") || !local) {
                    feasible.Add(eventsConteiner[trans.evento]);
                }

                
            }
        }

        return feasible;
    }

    public void RunEvents(List<Event> eventList) {
        for (int i = 0; i < eventList.Count; i++) {
            TriggerEvent(eventList[i], true);
        }
    }

   
    //  -------------------

    //FSM Classes

    //Event 
    public class Event 
    {
        static string[] typeMovement = {"u", "d", "l", "r"};
        static string[] typeMovementIO = { "out", "in" };
        static string typeGet = "getBrick";


        public int id; 
        public string label; 

        public string type {

            get {
                foreach (var mov in typeMovement) {
                    if(label == mov) {
                        return "typeMovement";
                    }
                }
                foreach (var movIO in typeMovementIO) {
                    if (label == movIO) {
                        return "typeMovementIO";
                    }
                }

                if(label.Length >= 2 && label.Substring(0,2) == "in") {
                    return "typeMovementIO";
                }

                if (label == typeGet) {
                    return "typeGet";
                }

                if ( label[0] == 'a') {
                    if (label.EndsWith("_r2")) {

                        return "typePlaceR2";

                    } else {
                        
                        return "typePlace";

                    }

                }

                return "typeOther";
            }
        }

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

        public bool InGrid {
            get {
                if(x > 0 && y > 0) {
                    return true;
                } else { return false; }
            }
        }

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

            this.name = name;

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
                    int index = 3 + (size.y * (i-1) + j);
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

