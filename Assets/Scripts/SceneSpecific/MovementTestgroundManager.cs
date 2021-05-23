using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovementTestgroundManager : MonoBehaviour
{
    public TermiteTS tileSystem;
    public GameObject robot2test;
    public DroneAnimationComponent robotAnimation;

    public InputField commandField;

    // Start is called before the first frame update
    void Start()
    {

        robotAnimation = robot2test.GetComponent<DroneAnimationComponent>();

        tileSystem.Initialize(5, 3, "TermiteTile");
        robotAnimation.Initialize(gameObject);

        //robotAnimation.TestAnimation();





    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayAnimation() {

        string command = commandField.textComponent.text;
        robotAnimation.CommandAnimation(command);

    }



}
