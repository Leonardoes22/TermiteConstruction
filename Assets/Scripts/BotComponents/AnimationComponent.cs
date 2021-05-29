using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AnimationComponent : MonoBehaviour
{

    public abstract void Initialize(GameObject manager);

    public abstract IEnumerator CallAnimation(string command, Action onComplete);

}
