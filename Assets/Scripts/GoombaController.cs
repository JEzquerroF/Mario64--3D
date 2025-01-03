using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoombaController : Enemy
{
    /* VARIABLES ENEMY: 
   [SerializeField] protected List<Transform> m_PatrolPoints;
   [SerializeField] protected float m_SpeedRotation;
   [SerializeField] protected float m_MaxDistanceToPatrol;
   [SerializeField] protected float m_MinDistanceToAlert;
   [SerializeField] protected float m_MaxDistanceToAttack;
   [SerializeField] protected float m_ConeAngle;
   [SerializeField] protected float m_MaxTimeChasing;
   [SerializeField] protected GameObject m_CenterEnemy;
   [SerializeField] protected int m_MaxHealth; 

   protected Vector3 m_PlayerPosition;
   protected Vector3 m_StartPosition;
   protected Animator m_EnemyAnimator;
   protected NavMeshAgent m_Agent;
   protected StatesEnemy m_CurrentState;
   protected bool m_Idle;
   protected int m_CurrentPatrol;
   protected bool m_FirstPatrol;
   protected bool m_setPatrol;
   protected bool m_Alert;
   protected bool m_Death = false; 
   protected bool m_IsDead;
   protected bool m_SeePlayer;
   protected bool m_SawPlayer;
   protected float m_TimeChasing;
   protected float m_MaxLerpSpeedRotation = 20;
   protected Vector3 m_LastValidPosition;
   protected float m_StartSpeed;
    */

    public override void Start()
    {
        base.Start();
    }

    public override void Update()
    {
        if (m_CurrentState == StatesEnemy.Chase)
            m_EnemyAnimator.SetBool("Chase", true);
        else
            m_EnemyAnimator.SetBool("Chase", false);

        if (m_CurrentState == StatesEnemy.Patrol)
            m_EnemyAnimator.SetBool("Patrol", true);
        else
            m_EnemyAnimator.SetBool("Patrol", false);

        if (m_CurrentState == StatesEnemy.Die)
            m_EnemyAnimator.SetBool("Death", true);

        if (m_CurrentState == StatesEnemy.Alert)
            m_EnemyAnimator.SetBool("Alert", true);
        else
            m_EnemyAnimator.SetBool("Alert", false);

        base.Update();
    }

    private IEnumerator RepulseOverTime(Vector3 Direction, float Distance, float Speed)
    {
        float l_TraveledDistance = 0f;
        Vector3 l_StartPosition = transform.position;

        while (l_TraveledDistance < Distance)
        {
            float l_Step = Speed * Time.deltaTime;
            transform.position += Direction * l_Step;
            l_TraveledDistance += l_Step;

            yield return null;
        }

        transform.position = l_StartPosition + Direction.normalized * Distance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.GetComponent<MarioController>().IsUpperHit(transform) == false)
        {
            MarioLifeController l_MarioLifeController = other.GetComponent<MarioLifeController>();

            if (l_MarioLifeController.m_IsMarioHit == false && m_Death == false)
            {
                other.GetComponent<Animator>().SetTrigger("Hit");
                StartCoroutine(l_MarioLifeController.PlayerHit(4.0f));

                Vector3 l_Direction = (other.transform.position - transform.position).normalized;
                l_Direction.Normalize();

                float l_RepulsionDistance = 4.0f;
                float l_RepulsionSpeed = 20.0f;
                StartCoroutine(other.GetComponent<MarioController>().RepulseOverTime(l_Direction, l_RepulsionDistance, l_RepulsionSpeed));
                StartCoroutine(RepulseOverTime(-l_Direction, l_RepulsionDistance, l_RepulsionSpeed));
            }
        }

        if (other.CompareTag("Punch"))
        {
            Debug.Log("Collision");
            m_MaxHealth -= 1;
            if (m_MaxHealth <= 0)
                Death();
        }

        if (other.CompareTag("Shell"))
        {
            KoopaController l_KoopaController = other.GetComponentInParent<KoopaController>();

            if (l_KoopaController.m_LaunchState)
            {
                Death();
            }
        }
    }
}
