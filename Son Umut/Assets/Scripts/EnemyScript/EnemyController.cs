using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public Transform Target;
    private NavMeshAgent _navMeshAgent;
    private Animator anim;
    void Start()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        
    }

    private void Update()
    {
        _navMeshAgent.SetDestination(Target.position);
        if (_navMeshAgent.velocity == Vector3.zero)
        {
            anim.SetBool("isMoving",false);
        }
        else
        {
            anim.SetBool("isMoving",true);
        }
    }
}
