using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TermiteAIComponent : MonoBehaviour
{
    public TermiteFSMBrain brain;

    public List<FSM.Event> myPlan = new List<FSM.Event>();

    private void Update() {
        if (brain.isAuto) {

            if (myPlan.Count == 0) {
                PlanAction(10, 5);
                foreach (var item in myPlan) {
                    //print(item);
                }
            }

            if (!brain.transitionHandler.IsTransitioning && !brain.animationHandler.IsAnimating) {

                if (brain.supervisorio.FeasibleEvents(brain.supervisorio.currentState, true).Contains(myPlan[0])) {
                    if (!brain.supervisorio.currentState.marked) {
                        brain.CallIntent(myPlan[0]);
                        myPlan.RemoveAt(0);
                    }



                } else {
                    PlanAction(10, 5);
                }

            }

        }
    }



    public void PlanAction(int steps = 5, int tries = 5) {

        List<FSM.Event> eventPlan = new List<FSM.Event>();
        int maxScore = -1;

        for (int i = 0; i < tries; i++) {

            FSM.State imaginaryState = brain.supervisorio.currentState;
            List<FSM.Event> tryPlan = new List<FSM.Event>();

            for (int j = 0; j < steps; j++) {

                //print(i + "" + j + " Started--- "+ imaginaryState);
                List<FSM.Event> feasible = brain.supervisorio.FeasibleEvents(imaginaryState, true);

                if (feasible.Count > 0) {
                    FSM.Event tryEvent = feasible[UnityEngine.Random.Range(0, feasible.Count)];
                    //print(i + "" + j + " Did--- " + tryEvent);

                    tryPlan.Add(tryEvent);

                    imaginaryState = brain.supervisorio.ImagineEvent(tryEvent, imaginaryState);
                    //print(i + "" + j + " Ended--- " + imaginaryState);
                }

            }

            if (EvaluatePlan(tryPlan, steps) > maxScore) {

                maxScore = EvaluatePlan(tryPlan, steps);
                eventPlan = tryPlan;
            }


        }
        //print("Score: " + maxScore);
        myPlan = eventPlan;


    }

    int EvaluatePlan(List<FSM.Event> tryPlan, int posImportance) {

        int count = 0;

        for (int i = 0; i < tryPlan.Count; i++) {

            switch (tryPlan[i].type) {

                case "typeGet":
                    count += 100 / ((i + 1) * posImportance);
                    break;

                case "typePlace":
                    if (i == 0) {
                        count += 10000;
                    } else {
                        count += 100 / ((i + 1) * posImportance);
                    }

                    break;

                case "typeMovementIO":
                    if (tryPlan[i].label == "out") {
                        if (i == 0) {
                            count += 1000;
                        } else {
                            count += 100 / ((i + 1) * posImportance);
                        }

                    }
                    break;

                default:
                    break;
            }

        }

        return count;
    }
}
