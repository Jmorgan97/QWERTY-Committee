using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrafeLocomotion : MonoBehaviour
{
    Animator animator;
    StarterAssetsInputs _input;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        _input = GetComponent<StarterAssetsInputs>();
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetFloat("InputX", _input.move.x);
        animator.SetFloat("InputY", _input.move.y);
    }
}
