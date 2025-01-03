using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IRestartGame
{
    [SerializeField] protected List<Transform> m_PatrolPoints;
    [SerializeField] protected float m_SpeedRotation;
    [SerializeField] protected float m_MaxDistanceToPatrol;
    [SerializeField] protected float m_MinDistanceToAlert;
    [SerializeField] protected float m_MaxDistanceToAttack;
    [SerializeField] protected float m_ConeAngle;
    [SerializeField] protected float m_MaxTimeChasing;
    [SerializeField] protected GameObject m_CenterEnemy;
    [SerializeField] protected int m_MaxHealth;
    [SerializeField] protected LayerMask m_HitLayerMask;

    [Header("Sounds")]
    [SerializeField] private AudioClip m_EnemyStompedSound;
    [SerializeField] private AudioClip m_EnemyDeathSound;

    [Header("Particles")]
    [SerializeField] protected GameObject m_DeathParticles;

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
    protected bool m_IsMarioDead;

    protected enum StatesEnemy
    {
        Idle,
        Patrol,
        Alert,
        Chase,
        Attack,
        Hit,
        Die
    }

    private void OnEnable()
    {
        MarioLifeController.OnPlayerDied += ExitChaseState;
    }

    private void OnDisable()
    {
        MarioLifeController.OnPlayerDied -= ExitChaseState;
    }

    public virtual void Start()
    {
        GameManager.instance.AddRestartGame(this);
        m_CurrentState = StatesEnemy.Idle;
        m_EnemyAnimator = GetComponentInChildren<Animator>();
        m_Agent = GetComponent<NavMeshAgent>();
        m_Agent.autoBraking = false;
        m_FirstPatrol = true;
        m_setPatrol = true;
        m_IsMarioDead = false;
        m_TimeChasing = 0;
        m_StartPosition = transform.position;
        m_StartSpeed = m_Agent.speed;
    }

    public virtual void Update()
    {
        m_PlayerPosition = GameManager.instance.GetPlayer().m_MarioTarget.transform.position;

        switch (m_CurrentState)
        {
            case StatesEnemy.Idle:
                HandleIdleState();
                break;
            case StatesEnemy.Patrol:
                HandlePatrolState();
                break;
            case StatesEnemy.Alert:
                HandleAlertState();
                break;
            case StatesEnemy.Chase:
                HandleChaseState();
                break;
            case StatesEnemy.Hit:
                HandleHitState();
                break;
            case StatesEnemy.Die:
                HandleDieState();
                break;
        }

        if ((DistanceToTarget(m_PlayerPosition) > m_MaxDistanceToPatrol || m_Idle) && !m_Death)
            m_CurrentState = StatesEnemy.Idle;
        else if ((DistanceToTarget(m_PlayerPosition) <= m_MaxDistanceToPatrol && m_Alert == false) && !m_Death)
            m_CurrentState = StatesEnemy.Patrol;
        else if ((m_Alert && SeePlayerConeVision() == false && SeePlayerHit()) && !m_Death)
            m_CurrentState = StatesEnemy.Alert;
        else if ((m_Alert && SeePlayerConeVision() && DistanceToTarget(m_PlayerPosition) <= m_MaxDistanceToAttack && SeePlayerHit()) && !m_IsMarioDead && !m_Death)
            m_CurrentState = StatesEnemy.Chase;
        else if (m_Death)
            m_CurrentState = StatesEnemy.Die;

        if (DistanceToTarget(m_PlayerPosition) <= m_MinDistanceToAlert)
            m_Alert = true;
        else
            m_Alert = false;

        if (m_Alert && SeePlayerHit())
            RotationToTarget(m_PlayerPosition);

        if (m_CurrentState != StatesEnemy.Patrol)
            m_setPatrol = true;

        if (m_CurrentState == StatesEnemy.Alert)
            m_SpeedRotation = 0.75f;
        else
            m_SpeedRotation = Mathf.Lerp(m_SpeedRotation, m_MaxLerpSpeedRotation, 0.5f * Time.deltaTime);

        if (m_SeePlayer && !SeePlayerHit())
        {
            m_SawPlayer = true;
            m_SeePlayer = false;
        }

        if (m_SawPlayer && m_Alert)
        {
            m_CurrentState = StatesEnemy.Chase;
            m_TimeChasing += Time.deltaTime;

            if (m_TimeChasing >= m_MaxTimeChasing || SeePlayerHit() && m_Alert)
            {
                m_SawPlayer = false;
                m_TimeChasing = 0;
            }
        }

        /*
        
        Debug.Log("STATE: " + m_CurrentState);
        Debug.Log("Distance: " + DistanceToTarget(m_PlayerPosition));
        Debug.Log("Alert:" + m_Alert);
        Debug.Log(SeePlayerConeVision());
        Debug.Log(SeePlayerHit());

         */
    }

    public void HandleHitState()
    {
    }

    public virtual void HandleIdleState()
    {
        //REPOSO
    }

    public virtual void HandlePatrolState()
    {
        m_Agent.speed = m_StartSpeed;
        m_Agent.isStopped = false;
        Transform l_PatrolPoint;

        if (m_setPatrol)
        {
            l_PatrolPoint = m_PatrolPoints[SetPatrol()];
            m_setPatrol = false;
        }

        l_PatrolPoint = m_PatrolPoints[m_CurrentPatrol];
        m_Agent.SetDestination(l_PatrolPoint.position);

        if (Vector3.Distance(l_PatrolPoint.position, transform.position) <= 4f)
            m_setPatrol = true;
    }

    public virtual void HandleAlertState()
    {
        m_Agent.isStopped = true;
        RotationToTarget(m_PlayerPosition);
        //SoundsManager.instance.PlaySoundClip(m_GoombaAlertSound, transform, 0.2f);
    }

    public virtual void HandleChaseState()
    {
        m_Agent.isStopped = false;
        m_Agent.speed = m_StartSpeed * 2;
        m_Agent.SetDestination(m_PlayerPosition);
    }

    private void ExitChaseState()
    {
        m_IsMarioDead = true;
        m_CurrentState = StatesEnemy.Idle;
        m_Alert = false;
        m_SeePlayer = false;
        m_SawPlayer = false;

        if (m_Agent.enabled == true)
            m_Agent.isStopped = true;
    }

    public void Death()
    {
        m_Death = true;
    }

    public virtual void HandleDieState()
    {
        if (m_Agent.enabled == true)
            m_Agent.isStopped = true;

        if (m_IsDead) return;

        m_IsDead = true;

        StartCoroutine(EnemyDie());
    }

    public virtual IEnumerator EnemyDie()
    {
        SoundsManager.instance.PlaySoundClip(m_EnemyStompedSound, transform, 0.2f);
        yield return new WaitForSeconds(0.4f);
        SoundsManager.instance.PlaySoundClip(m_EnemyDeathSound, transform, 0.2f);
        GameObject l_Particle = Instantiate(m_DeathParticles, transform.position + Vector3.up / 2, Quaternion.identity);
        ParticleSystem l_ParticleDeath = l_Particle.GetComponent<ParticleSystem>();
        l_ParticleDeath.Play();
        yield return new WaitForSeconds(0.65f);
        gameObject.SetActive(false);
    }

    public virtual bool SeePlayerConeVision()
    {
        Vector3 l_directionToPlayer = (m_PlayerPosition - m_CenterEnemy.transform.position); // REVISAR LAYER
        l_directionToPlayer.Normalize();

        float l_angleToPlayer = Vector3.Angle(transform.forward, l_directionToPlayer);

        Debug.DrawLine(m_CenterEnemy.transform.position, m_PlayerPosition, Color.blue);

        if (l_angleToPlayer < m_ConeAngle / 2)
        {
            return true;
        }

        return false;
    }

    public virtual bool SeePlayerHit()
    {
        Vector3 l_directionToPlayer = (m_PlayerPosition - m_CenterEnemy.transform.position);
        l_directionToPlayer.Normalize();
        RaycastHit l_hit;
        Debug.DrawRay(m_PlayerPosition, l_directionToPlayer, Color.blue);
        if (Physics.Raycast(m_CenterEnemy.transform.position, l_directionToPlayer, out l_hit, (DistanceToTarget(m_PlayerPosition) + 10.0f), m_HitLayerMask.value, QueryTriggerInteraction.Ignore))
        {
            if (l_hit.collider.CompareTag("Player"))
            {
                m_SeePlayer = true;
                return true;
            }
        }
        return false;
    }

    public virtual void RotationToTarget(Vector3 l_target)
    {
        Vector3 l_direction = (l_target - transform.position).normalized;
        Quaternion l_rotation = Quaternion.LookRotation(l_direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, l_rotation, Time.deltaTime * m_SpeedRotation);
    }

    public virtual float DistanceToTarget(Vector3 l_target)
    {
        return Vector3.Distance(l_target, transform.position);
    }

    private Vector3 GetDirectionPlayer()
    {
        Vector3 l_direction = m_PlayerPosition - transform.position;

        return l_direction.normalized;
    }

    private int SetPatrol()
    {
        if (m_FirstPatrol)
        {
            float l_minDistance = Vector3.Distance(transform.position, m_PatrolPoints[0].transform.position);
            int l_nextPatrolPosition = 0;

            for (int i = 1; i < m_PatrolPoints.Count; i++)
            {
                if (Vector3.Distance(transform.position, m_PatrolPoints[i].transform.position) < l_minDistance)
                {
                    l_minDistance = Vector3.Distance(transform.position, m_PatrolPoints[i].transform.position);
                    l_nextPatrolPosition = i;
                }
            }
            m_FirstPatrol = false;
            m_CurrentPatrol = l_nextPatrolPosition;

            return m_CurrentPatrol;
        }
        else
        {
            m_CurrentPatrol++;

            if (m_CurrentPatrol >= m_PatrolPoints.Count)
            {
                m_CurrentPatrol = 0;
            }
        }
        return m_CurrentPatrol;
    }

    private void OnDrawGizmos()
    {
        Vector3 l_forward = transform.forward;
        float l_ConeAngle = m_ConeAngle / 2;

        float l_MaxDistance = m_MinDistanceToAlert;

        Vector3 l_leftDirection = Quaternion.Euler(0, -l_ConeAngle, 0) * l_forward;
        Vector3 l_RightDirection = Quaternion.Euler(0, l_ConeAngle, 0) * l_forward;

        Debug.DrawRay(m_CenterEnemy.transform.position, l_forward * l_MaxDistance, Color.green);
        Debug.DrawRay(m_CenterEnemy.transform.position, l_leftDirection * l_MaxDistance, Color.red);
        Debug.DrawRay(m_CenterEnemy.transform.position, l_RightDirection * l_MaxDistance, Color.red);

        int l_NumRays = 10;

        for (int i = 1; i < l_NumRays; ++i)
        {
            float t = i / (float)(l_NumRays);
            Vector3 interpolatedDirection = Vector3.Slerp(l_leftDirection, l_RightDirection, t);
            Debug.DrawRay(m_CenterEnemy.transform.position, interpolatedDirection * l_MaxDistance, Color.yellow);
        }
    }

    public virtual void RestartGame()
    {
        m_Agent.enabled = true;
        m_CurrentState = StatesEnemy.Idle;
        m_Agent.velocity = Vector3.zero;
        m_Alert = false;
        m_SeePlayer = false;
        m_setPatrol = true;
        m_FirstPatrol = true;
        m_IsDead = false;
        m_IsMarioDead = false;
        transform.position = m_StartPosition;
        m_MaxHealth = 2;
    }
}
