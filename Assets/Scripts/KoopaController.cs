using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class KoopaController : Enemy
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

    [SerializeField] private GameObject m_ShellGameObject;
    [SerializeField] private GameObject m_BodyKoopa;
    [SerializeField] private PhysicMaterial m_PhysicsMaterial;
    [SerializeField] private float m_MaxTimeShell;
    [SerializeField] private float m_MaxTimeToDie;
    [SerializeField] private GameObject m_ParticleShell;

    public bool m_ShellState = false;
    private bool m_SetShell = false;
    private bool m_CatchState = false;
    public bool m_LaunchState = false;
    private float m_TimeShellState = 0.0f;
    private BoxCollider m_BoxCollider;
    private Rigidbody m_RigidBody;
    private float m_TimeToDie;
    private bool m_CanBeLaunched = false;
    private bool m_ShellMoving;

    [Header("Sounds")]
    [SerializeField] private AudioClip m_KoopaDeathSound;
    [SerializeField] private AudioClip m_KoopaStompedSound;

    public override void Start()
    {
        m_BoxCollider = GetComponent<BoxCollider>();
        m_RigidBody = GetComponent<Rigidbody>();

        base.Start();
    }

    public override void Update()
    {
        m_ShellMoving = false;

        if (!m_ShellState)
        {
            base.Update();
        }
        else
        {
            if (!m_SetShell)
            {
                m_EnemyAnimator.SetBool("Crouch", true);
                SoundsManager.instance.PlaySoundClip(m_KoopaStompedSound, transform, 0.2f);

                AnimatorStateInfo l_stateInfo = m_EnemyAnimator.GetCurrentAnimatorStateInfo(0);

                if (l_stateInfo.normalizedTime >= 1 && m_Death == false)
                {
                    m_BodyKoopa.SetActive(false);
                    m_ShellGameObject.SetActive(true);
                    m_SetShell = true;
                    m_CanBeLaunched = false;
                }
            }
            else
            {
                if (!m_CatchState)
                {
                    if (m_RigidBody.velocity.magnitude <= 0.5)
                    {
                        m_ParticleShell.gameObject.SetActive(false);

                        m_TimeShellState += Time.deltaTime;

                        if (m_TimeShellState >= m_MaxTimeShell)
                        {
                            SoundsManager.instance.PlaySoundClip(m_KoopaDeathSound, transform, 0.2f);
                            m_RigidBody.isKinematic = true;
                            EnableComponents(true);
                            m_BodyKoopa.SetActive(true);
                            m_ShellGameObject.SetActive(false);
                            m_TimeShellState = 0.0f;
                            m_EnemyAnimator.SetBool("Crouch", false);
                            m_SetShell = false;
                            m_ShellState = false;
                        }
                        else if (m_TimeShellState >= 1.0f)
                            m_CanBeLaunched = true;

                        m_LaunchState = false;
                    }
                    else
                    {
                        m_ShellMoving = true;
                        m_ParticleShell.gameObject.SetActive(true);

                        m_TimeToDie += Time.deltaTime;

                        if (m_TimeToDie >= m_MaxTimeToDie)
                        {
                            SoundsManager.instance.PlaySoundClip(m_KoopaDeathSound, transform, 0.2f);
                            GameObject l_Particle = Instantiate(m_DeathParticles, transform.position + Vector3.up / 2, Quaternion.identity);
                            ParticleSystem l_ParticleDeath = l_Particle.GetComponent<ParticleSystem>();
                            l_ParticleDeath.Play();
                            m_Death = true;
                            gameObject.SetActive(false);
                            m_TimeToDie = 0.0f;
                        }

                        m_LaunchState = true;
                    }
                }
            }
        }
    }

    public void EnableComponents(bool enable)
    {
        if (m_Agent.isActiveAndEnabled)
            m_Agent.isStopped = true;

        m_Agent.enabled = enable;
        m_RigidBody.velocity = Vector3.zero;
        m_RigidBody.angularVelocity = Vector3.zero;
        m_RigidBody.useGravity = false;
        m_BoxCollider.enabled = enable;
    }

    public void LaunchShell(Vector3 l_Direction)
    {
        m_BoxCollider.enabled = true;
        m_RigidBody.useGravity = true;
        m_BoxCollider.material = m_PhysicsMaterial;
        m_RigidBody.isKinematic = false;
        m_RigidBody.AddForce(l_Direction * 1500);
        m_CatchState = false;
    }

    public void SetCatchState()
    {
        m_CatchState = true;
        m_TimeShellState = 0;
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
            if (m_ShellMoving || m_ShellState == false)
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
        }

        if (other.CompareTag("Shell"))
        {
            KoopaController l_KoopaController = other.GetComponentInParent<KoopaController>();

            if (l_KoopaController.m_LaunchState)
            {
                if (m_ShellState && m_CanBeLaunched)
                {
                    EnableComponents(false);
                    LaunchShell(other.transform.forward);
                }
                else
                {
                    m_RigidBody.velocity = Vector3.zero;
                    m_RigidBody.angularVelocity = Vector3.zero;
                    m_ShellState = true;
                }
            }
        }

        if (other.CompareTag("Punch"))
        {
            m_MaxHealth -= 1;
            if (m_MaxHealth <= 0)
            {
                m_ShellState = true;
                m_MaxHealth = 2;
            }
        }
    }

    public override void RestartGame()
    {
        base.RestartGame();
        m_ShellState = false;
    }

    /*
    Debug.Log("CurrentState:" + m_CurrentState);
    Debug.Log("SeePlayerHit: " + SeePlayerHit());
    Debug.Log("ConeAngle: " + SeePlayerConeVision());   
    */
}
