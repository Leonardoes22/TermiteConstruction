using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml.Linq;
using System;
using UnityEngine.PlayerLoop;


public class StructurePlant{


    public List<Supervisor> supList = new List<Supervisor>();
    
    // Properties
    public Coord shape;
    public HeightMap finalStructure;
    public bool isMultiBot;

    public StructurePlant(string filePath) {

        LoadStructure(filePath);

    }


    public void LoadStructure(string filePath) {

        var supPath = Directory.GetCurrentDirectory() + "\\Assets\\Resources\\Supervisors\\" + filePath;

        //Nó (automata)
        XElement structureSupervisors = XElement.Load(supPath);

        //Load Shape
        shape = LoadShape(structureSupervisors);

        //Load Supervisors List
        foreach (var automaton in structureSupervisors.Elements()) {
            Supervisor sup = new Supervisor(automaton, this);
            supList.Add(sup);
        }

        // Load Final Structure and check if MultiBot mode is possible
        finalStructure = LoadFinalStructure(supList[0]);
        isMultiBot = CheckIsMultiBot(supList[0]);

    }

    Coord LoadShape(XElement automata) {

        // Get Size
        string name = (string)automata.Element("Automaton").Attribute("name"); // Nome do Automaton
        string[] parse = name.Split(new string[] { "||" }, StringSplitOptions.None); // Parser
        
        Coord shape = new Coord();
        shape.x = int.Parse(parse[parse.Length - 2]);
        shape.y = int.Parse(parse[parse.Length - 1]);

        return shape;
    }

    HeightMap LoadFinalStructure(Supervisor sup) {
        
        foreach (var state in sup.statesContainer.Values) {
            if (state.marked == true) {
                return state.heightMap;
            }
        }
        return null;
    }

    bool CheckIsMultiBot(Supervisor sup) {
        int count = 0;
        int countAlt = 0;

        foreach (var e in sup.eventsContainer.Values) {

            if (e.label[0] == 'a') {
                if (e.label.EndsWith("_r2")) {
                    countAlt++;
                } else {
                    count++;
                }
            }

        }
        return count == countAlt;
    }

}
