using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Xml.Linq;


public class FSM : MonoBehaviour
{
    List<Transition> transitionList = new List<Transition>();
    Dictionary<int, State> statesConteiner = new Dictionary<int, State>();
    Dictionary<int, Event> eventsConteiner = new Dictionary<int, Event>();

    State currentState;
    

    // Start is called before the first frame update
    void Start()
    {

        StartFSM();
        LoadSupervisor();

    }

    // Update is called once per frame
    void Update()
    {
        


    }


    void LoadSupervisor() {


        foreach (XElement item in XElement.Load("Assets\\Resources\\supCSync.xml").Elements("Automata").DescendantNodes<XElement>()) {
            print("HAHHAHA");
        }


    }


    public void StartFSM() {
        LoadEvents();
        LoadStates();
        LoadTransitions();
    }

    void LoadStates() {
        statesConteiner.Add(0, new State(0, false, 0, 0, new int[0, 0]));
        statesConteiner.Add(1, new State(1, true, 0, 0, new int[0, 0]));
        currentState = statesConteiner[1];
        MonoBehaviour.print(currentState);
    }

    void LoadTransitions() {
        transitionList.Add(new Transition(0, 0, 1));
        transitionList.Add(new Transition(1, 0, 0));
        transitionList.Add(new Transition(0, 1, 1));
        transitionList.Add(new Transition(0, 1, 0));
    }

    void LoadEvents() {
        eventsConteiner.Add(0, new Event(0, "segue"));
        eventsConteiner.Add(1, new Event(1, "gira"));

    }



    //Função temporária de apoio
    public void SelectEvent(int id) {
        TriggerEvent(eventsConteiner[id]);
    }


    //Verifica (e dispara) se há alguma transição disponível para o estado atual dado o evento ocorrido
    void TriggerEvent(Event e) {
        MonoBehaviour.print("Triggered:" + e);

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
        MonoBehaviour.print("Fired:" + t);
        currentState = statesConteiner[t.dest];
        MonoBehaviour.print("State:" + currentState);
    }

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

    public override string ToString() {
        return "Event [id= " + this.id + ", label= " + this.label +"]";
    }

}

//Transition
 public class Transition {

    public int dest;
    public int evento;
    public int source;

    public Transition(int d, int e, int s) {

        this.dest = d;
        this.evento = e;
        this.source = s;

    }

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

    public State(int id, bool carregado, int x, int y, int[,] heights) {

        this.id = id;
        sStr = carregado ? "S1" : "S0";
        this.x = x;
        this.y = y;
        this.heightMap = heights;

    }

    public override string ToString() {
        return sStr + "." + x + "." + y;
    }


}

