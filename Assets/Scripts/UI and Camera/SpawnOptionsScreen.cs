using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnOptionsScreen : MonoBehaviour
{

    public GameObject gameManager;

    // Supervisor and State Selector
    public GameObject spawnOptionsScreen;
    public GameObject supervisorDropdown;
    public GameObject stateInputField;
    public GameObject spawnBotButton;
    public GameObject startButton;
    public GameObject stateDropdown;


    public StructurePlant structurePlant;

    public CentralController centralController;
    public InterfaceMenu interfaceMenu;
    public InterfaceFSM interfaceFSM;


    // Start is called before the first frame update
    void Start()
    {


        //Spawn Bot Button
        spawnBotButton.GetComponentInChildren<Button>().onClick.AddListener(() => SpawnBotListener());

        //Start Button
        startButton.GetComponentInChildren<Button>().onClick.AddListener(() => StartSimulation());

        //Supervisor Selector Dropdown
        supervisorDropdown.GetComponent<Dropdown>().onValueChanged.AddListener(delegate { SupervisorDropdownChanged(); });
    }

    // Update is called once per frame
    void Update()
    {

        
    }

    //General
    public void Toggle() {

        if (spawnOptionsScreen.activeInHierarchy) {
            Close();
        } else {
            Open();
            UpdateSupervisorList();
        }

    }

    public void Open() {
        spawnOptionsScreen.SetActive(true);

        if(interfaceMenu != null) {
            spawnBotButton.SetActive(false);
            stateDropdown.SetActive(false);
            UpdateSupervisorList();
        }
        if(interfaceFSM != null) {
            startButton.SetActive(false);
            stateInputField.SetActive(false);
            UpdateSupervisorList();
            UpdateStatesList();
        }

        
        
        
    }
    public void Close() {
        spawnOptionsScreen.SetActive(false);
    }

    public void SetStructurePlant(StructurePlant plant) {

        structurePlant = plant;
        
    }


    
    
    void UpdateSupervisorList() {
        List<string> ops = new List<string>();
        var supList = structurePlant.supList;

        for (int i = 0; i < supList.Count; i++) {
            ops.Add("Supervisorio: " + (i + 1));
        }

        spawnOptionsScreen.GetComponentInChildren<Dropdown>().ClearOptions();
        spawnOptionsScreen.GetComponentInChildren<Dropdown>().AddOptions(ops);
    }

    void UpdateStatesList() {


        List<string> stateOptions = new List<string>();

        var statesList = gameManager.GetComponent<SimManager>().structurePlant.supList[GetSelectedSupervisor()].statesContainer.Values;

        foreach (var state in statesList) {

            if(state.heightMap == centralController.heightMap) {
                stateOptions.Add(state.name);
                
            }

        }

        
        stateDropdown.GetComponentInChildren<Dropdown>().ClearOptions();
        stateDropdown.GetComponentInChildren<Dropdown>().AddOptions(stateOptions);
    }


    int GetSelectedSupervisor() {

        string name = spawnOptionsScreen.GetComponentInChildren<Dropdown>().gameObject.GetComponentInChildren<Text>().text;

        int selected = int.Parse(name.Split(':')[1]) - 1;

        return selected;

    }

    // Simulation
    void SpawnBot() {

        int selected = GetSelectedSupervisor();

        string stateName = stateDropdown.GetComponentInChildren<Dropdown>().gameObject.GetComponentInChildren<Text>().text;
        print(stateName);
        centralController.SpawnBot(selected, stateName);

    }

    void StartSimulation() {

        int selected = GetSelectedSupervisor();
        string stateName = stateInputField.GetComponentsInChildren<Text>()[1].text;

        foreach (var state in structurePlant.supList[selected].statesContainer.Values)
        {
            if(state.name == stateName)
            {
                interfaceMenu.StartSimulation(selected, stateName);
                return ;
            }
        }


        interfaceMenu.StartSimulation(selected);

    }

    //Menu


    // Listeners
    void SpawnBotListener() {

        SpawnBot();
        Close();

    }

    
    
    void SupervisorDropdownChanged() {

        if (interfaceFSM != null)
        {
            UpdateStatesList();
        }

        

    }
    
    /*
    public void GenerateHint() {


        if (spawnOptionsScreen.activeInHierarchy) {

            int selected = GetSelectedSupervisor();


            stateInputField.GetComponent<InputField>().text = structurePlant.supList[selected].initialState.ToString();

        }

    }

        */

}
