# TermiteConstruction

## Interface
![](Manual1.png)
1. *Start*: Initializes the simulation of the selected scenario.
1. *Scenarios*: Selection of the desired scenario. (Between the available XML files)
1. *Fullscreen Button*: Switches _fullscreen_ on or off.
1. *Structure Preview*: Vizualisation of the scenario's final structure.
1. *Sair*: Return to menu.
1. *Add Bot*: Add a robot. (If possible in the selected scenario)
1. *Fast Animation*: Activates instant animations.
1. *Auto*: Alternates between manual control and automatic mode.
1. *Disband*: Removes robot from the scenario.
1. *States*: Shows the state of the selected robot.
1. *Events*: Allows the selection of events during the manual control mode.
1. *Selected Robot*: A blue border indicates which is the current selected robot.

## Camera and Control

**Structure Preview**: Use the left mouse button to turn the camere angle and the _scroll_ wheel for zooming in and out.

**Simulator**: **Structure Preview** controls + *WASD* to move the camera and *shift* the move faster. To select a robot it suffices to click on it with the left mouse button.

## Adding New Scenarios
To add new scenarios to the simulator it suffices to move the desired XML file to the folder ".../TermiteSim/Assets/Resources/Supervisors". 
The file must contain an automaton with its name as in the example of the following image:

![](SupervisorModel.png)
