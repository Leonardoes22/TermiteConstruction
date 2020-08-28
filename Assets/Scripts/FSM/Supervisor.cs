using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEditorInternal;
using UnityEngine;


public class Supervisor
{
    public Dictionary<int, FSM.Event> eventsContainer = new Dictionary<int, FSM.Event>(); // Events Dictionary
    public Dictionary<int, FSM.State> statesContainer = new Dictionary<int, FSM.State>(); // States Dictionary
    public List<FSM.Transition> transitionsList = new List<FSM.Transition>(); // Transition List

    public FSM.State initialState;

    public Supervisor(XElement automaton, StructurePlant parentPlant) {

        // Get Nodes
        IEnumerable events = automaton.Descendants("Event"); // Enumerável de eventos
        IEnumerable states = automaton.Descendants("State"); // Enumerável de estados
        IEnumerable transitions = automaton.Descendants("Transition"); // Enumerável de transições


        // Get lists
        eventsContainer = LoadEvents(events);
        statesContainer = LoadStates(states, parentPlant.shape);
        transitionsList = LoadTransitions(transitions);

    }


    Dictionary<int, FSM.Event> LoadEvents(IEnumerable events) {

        Dictionary<int, FSM.Event> eventsContainer = new Dictionary<int, FSM.Event>();

        //Carrega o dicionário de eventos
        foreach (XElement eventData in events) {

            int eventId = (int)eventData.Attribute("id");
            string eventLabel = (string)eventData.Attribute("label");

            FSM.Event eventCaster = new FSM.Event(eventId, eventLabel);
            eventsContainer.Add(eventId, eventCaster);
        }

        return eventsContainer;

    }

    Dictionary<int, FSM.State> LoadStates(IEnumerable states, Coord size) {

        Dictionary<int, FSM.State> statesContainer = new Dictionary<int, FSM.State>();

        //Carrega o dicionário de estados
        foreach (XElement stateData in states) {

            int stateId = (int)stateData.Attribute("id");
            string stateName = (string)stateData.Attribute("name");

            FSM.State stateCaster = new FSM.State(stateId, stateName, size);
            if ((string)stateData.Attribute("accepting") == "true") {
                stateCaster.marked = true;
            }

            
            if ((string)stateData.Attribute("initial") == "true") {
                initialState = stateCaster;
            }
            

            statesContainer.Add(stateId, stateCaster);
        }

        return statesContainer;
        
    }

    List<FSM.Transition> LoadTransitions(IEnumerable transitions) {

        List<FSM.Transition> transitionsList = new List<FSM.Transition>();

        //Carrega a lista de transições
        foreach (XElement transitionData in transitions) {

            int dest = (int)transitionData.Attribute("dest");
            int evento = (int)transitionData.Attribute("event");
            int source = (int)transitionData.Attribute("source");

            FSM.Transition transitionCaster = new FSM.Transition(dest, evento, source);
            transitionsList.Add(transitionCaster);
        }

        return transitionsList;
    }
}



        

        

        