using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class MarioController : MonoBehaviour, IRestartGame
{
    private CharacterController m_CharacterController;
    private MarioLifeController m_MarioLifeController;
    private Animator m_Animator;
    public Camera m_Camera;

    public enum TPunchType
    {
        RIGHT_HAND,
        LEFT_HAND,
        RIGHT_FOOT
    }

    [Header("Physics")]
    [SerializeField][Range(0.2f, 0.75f)] private float m_TimeToStop = 0.2f;
    public float m_WalkSpeed = 2.0f;
    public float m_RunSpeed = 8.0f;
    private float m_Speed;
    public float m_LerpRotationPct = 0.8f;
    public float m_VerticalSpeed = 0f;
    public float m_GravityForce;
    [SerializeField] private float m_JumpForce;
    [SerializeField] private float m_BridgeForce;
    private Vector3 m_MovementDirection = Vector3.zero;
    private float m_ElapsedTimeNoMovement = 1.0f;
    public GameObject m_MarioTarget;
    private float m_NotGroundedTime = 0;
    [SerializeField] private GameObject m_CollisionGround;
    public bool m_HasMovement = false;

    [Header("Input")]
    public KeyCode m_LeftKeyCode = KeyCode.A;
    public KeyCode m_RightKeyCode = KeyCode.D;
    public KeyCode m_UpKeyCode = KeyCode.W;
    public KeyCode m_DownKeyCode = KeyCode.S;
    public KeyCode m_RunKeyCode = KeyCode.LeftShift;
    public KeyCode m_JumpKeyCode = KeyCode.Space;
    private PlayerControls m_PlayerControls;

    [Header("WallJump")]
    [SerializeField] private List<Transform> m_WallJumpRays;
    [SerializeField] private float m_WallJumpForce = 10.0f;
    [SerializeField] private float m_WallJumpCooldown = 0.5f;
    private float m_LastWallJumpTime = 0.0f;
    private bool m_IsTouchingWall = false;

    [Header("Punchs")]
    public GameObject m_LeftHandPunchHitCollider;
    public GameObject m_RightHandPunchHitCollider;
    public GameObject m_RightFootPunchHitCollider;

    private int m_PunchHitButton = 0;
    private int m_CurrentPunchId = 0;
    private float m_LastPunchTime;
    public float m_PunchComboAvailableTime = 0.6f;
    private float m_WaitStartJumpTime = 0.08f;
    private int m_CurrentJumpId = 0;
    private bool m_CanJump;
    private float m_LastJumpTime = 0.0f;
    private float m_JumpComboIsAviable = 1.0f;

    [Header("Goomba")]
    public float m_KillJumpVerticalSpeed;
    public float m_MaxAngleNeededToKillGoomba;
    public float m_MinVerticalSpeedToKillGoomba;

    [Header("Elevator")]
    [SerializeField] private float m_MaxAngleToAttachElevator;
    private Collider m_CurrentElevator = null;

    [Header("Checkpoint")]
    private Vector3 m_StartPosition;
    private Quaternion m_StartRotation;

    [Header("Shell")]
    [SerializeField] private GameObject m_ShellPosition;
    private bool m_CanCatchShell = false;
    private bool m_ShellCatched = false;
    private KoopaController m_Koopa;
    private Transform m_PreviousParent = null;

    [Header("Animations")]
    [SerializeField] float m_MaxIdleBreakTime = 10.0f;
    private float m_IdleBreakTimer = 0.0f;

    [Header("Particles")]
    [SerializeField] private ParticleSystem m_RunningParticles;
    [SerializeField] private ParticleSystem m_HitHeadEnemy;

    [Header("Sounds")]
    public AudioClip m_StepSound;
    [SerializeField] private AudioClip[] m_JumpSounds;
    [SerializeField] private AudioClip[] m_PunchSounds;

    private bool m_GamePadActive;

    private void Awake()
    {
        m_PlayerControls = new PlayerControls();
        m_CharacterController = GetComponent<CharacterController>();
        m_Animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        m_PlayerControls.Gameplay.Enable();
    }

    private void Start()
    {
        GameManager.instance.SetPlayer(this);
        GameManager.instance.AddRestartGame(this);

        m_MarioLifeController = GetComponent<MarioLifeController>();
        m_Animator.fireEvents = false;
        m_LeftHandPunchHitCollider.SetActive(false);
        m_RightHandPunchHitCollider.SetActive(false);
        m_RightFootPunchHitCollider.SetActive(false);

        m_StartPosition = transform.position;
        m_StartRotation = transform.rotation;

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        m_HasMovement = false;

        if (m_PlayerControls.Gameplay.Move.IsPressed() || m_PlayerControls.Gameplay.Look.IsInProgress())
            m_GamePadActive = true;
        else
            m_GamePadActive = false;

        // Procesar dirección de movimiento según la cámara
        Vector3 l_Forward = m_Camera.transform.forward;
        Vector3 l_Right = m_Camera.transform.right;
        l_Forward.y = 0; l_Right.y = 0;
        l_Forward.Normalize();
        l_Right.Normalize();

        if (!m_GamePadActive && m_MarioLifeController.m_IsMarioDead == false)
        {
            if (Input.GetKey(m_RightKeyCode))
            {
                m_MovementDirection = l_Right / 2;
                m_HasMovement = true;

            }
            else if (Input.GetKey(m_LeftKeyCode))
            {
                m_MovementDirection = -l_Right / 2;
                m_HasMovement = true;
            }
            if (Input.GetKey(m_DownKeyCode))
            {
                m_MovementDirection -= l_Forward;
                m_HasMovement = true;
            }
            else if (Input.GetKey(m_UpKeyCode))
            {
                m_MovementDirection += l_Forward;
                m_HasMovement = true;
            }
        }
        else if (!m_MarioLifeController.m_IsMarioDead && m_GamePadActive)
        {
            if (m_PlayerControls.Gameplay.Move.IsPressed())
                m_HasMovement = true;

            Vector2 inputMovement = m_PlayerControls.Gameplay.Move.ReadValue<Vector2>();
            m_MovementDirection = new Vector3(inputMovement.x, 0, inputMovement.y);
            m_MovementDirection = l_Forward * m_MovementDirection.z + l_Right * m_MovementDirection.x;
        }

        if (m_HasMovement)
        {
            m_ElapsedTimeNoMovement = 0.0f;

            if ((Input.GetKey(m_RunKeyCode) || m_PlayerControls.Gameplay.Run.IsPressed()) && m_CanJump)
            {
                m_Speed = m_RunSpeed;
                m_Animator.SetFloat("Speed", 1f);
            }
            else
            {
                m_Speed = m_WalkSpeed;
                m_Animator.SetFloat("Speed", 0.2f);
            }

            Quaternion l_DesiredRotation = Quaternion.LookRotation(m_MovementDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, l_DesiredRotation, m_LerpRotationPct * Time.deltaTime);
        }
        else
        {
            m_ElapsedTimeNoMovement += Time.deltaTime / m_TimeToStop;
            m_Speed = Mathf.Lerp(m_WalkSpeed, 0.0f, m_ElapsedTimeNoMovement);

            if (m_ElapsedTimeNoMovement >= 1)
                m_Animator.SetFloat("Speed", 0);
        }

        if (((Input.GetKeyDown(m_JumpKeyCode) || m_PlayerControls.Gameplay.Jump.triggered && IsGrounded()) && IsGrounded() &&
            m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Idle Break") == false) &&
            !m_MarioLifeController.m_IsMarioDead)
        {
            ComboJump();
            m_CanJump = false;
        }

        if (m_CharacterController.velocity.magnitude == m_RunSpeed && m_CharacterController.isGrounded)
        {
            m_RunningParticles.loop = true;
            m_RunningParticles.Play();
        }
        else if (m_CharacterController.velocity.magnitude <= 0.0f || !m_CharacterController.isGrounded)
        {
            m_RunningParticles.loop = false;
        }

        m_MovementDirection.Normalize();
        m_MovementDirection *= m_Speed * Time.deltaTime;
        //Comprobar si hace un tiempo no colisiona Correcaminos
        m_VerticalSpeed += Physics.gravity.y * m_GravityForce * Time.deltaTime;
        m_MovementDirection.y = m_VerticalSpeed * Time.deltaTime;
        CollisionFlags l_CollisionFlags = m_CharacterController.Move(m_MovementDirection);

        if (((l_CollisionFlags & CollisionFlags.Below) != 0 && m_VerticalSpeed < 0.0f) ||
            (l_CollisionFlags & CollisionFlags.Above) != 0 && m_VerticalSpeed > 0.0f)
            m_VerticalSpeed = 0.0f;

        if (IsGrounded())
        {
            m_CanJump = true;
            m_Animator.SetBool("Falling", false);
            m_NotGroundedTime = 0;
        }
        else
        {
            m_NotGroundedTime += Time.deltaTime;

            if (m_NotGroundedTime >= 0.1f)
            {
                m_CanJump = false;
            }
        }

        if ((l_CollisionFlags & CollisionFlags.Above) != 0)
        {
            m_Animator.SetBool("Falling", true);
        }

        UpdateWallJump();
        UpadatePunch();
        UpdateIdleBreak();
        UpdateElevator();

        if (m_CanCatchShell)
        {
            if (Input.GetMouseButtonDown(1) || m_PlayerControls.Gameplay.Catch.triggered)
            {
                m_Koopa.EnableComponents(false);
                m_Koopa.SetCatchState();
                m_Koopa.transform.position = m_ShellPosition.transform.position;
                m_Koopa.transform.SetParent(m_ShellPosition.transform);
                m_Animator.SetLayerWeight(1, 0.5f);

                m_ShellCatched = true;
                m_CanCatchShell = false;
            }
        }

        if (m_ShellCatched)
        {
            if (Input.GetMouseButtonDown(0) || m_PlayerControls.Gameplay.Punch.triggered)
            {
                m_Animator.SetLayerWeight(1, 0.0f);
                m_Koopa.transform.parent = m_PreviousParent;
                m_Koopa.LaunchShell(transform.forward);
                m_ShellCatched = false;
            }
        }
    }

    private void LateUpdate()
    {
        Vector3 l_Angles = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(0.0f, l_Angles.y, 0.0f);
    }

    public void SetStartPosition(Vector3 l_Position)
    {
        m_StartPosition = l_Position;
    }

    private void UpdateIdleBreak()
    {
        if (m_Speed <= 0.0f && IsGrounded() == true)
        {
            m_IdleBreakTimer += Time.deltaTime;

            if (m_IdleBreakTimer >= m_MaxIdleBreakTime)
            {
                if (m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Idle Break"))
                {
                    return;
                }

                m_Animator.SetTrigger("IdleBreak");
                m_IdleBreakTimer = 0.0f;
            }
        }
        else
        {
            m_IdleBreakTimer = 0.0f;
        }
    }

    private void ComboJump()
    {
        float l_CurrentTime = Time.time - m_LastJumpTime;

        if (l_CurrentTime <= m_JumpComboIsAviable)
        {
            m_CurrentJumpId = (m_CurrentJumpId + 1) % 3;
        }
        else
            m_CurrentJumpId = 0;

        m_Animator.SetInteger("JumpCombo", m_CurrentJumpId);
        m_LastJumpTime = Time.time;
        Jump();
    }

    private void Jump()
    {
        m_IdleBreakTimer = 0.0f;
        m_Animator.SetTrigger("Jump");
        StartCoroutine(ExecuteJump());

        int l_RandomNumber = Random.Range(0, 3);
        SoundsManager.instance.PlaySoundClip(m_JumpSounds[l_RandomNumber], transform, 0.2f);
    }

    IEnumerator ExecuteJump()
    {
        yield return new WaitForSeconds(m_WaitStartJumpTime);
        m_VerticalSpeed = m_JumpForce + m_CurrentJumpId * 2;
    }

    private void UpdateWallJump()
    {
        m_IsTouchingWall = false;

        foreach (Transform l_WallRay in m_WallJumpRays)
        {
            Ray l_Ray = new Ray(l_WallRay.position, transform.forward);
            RaycastHit l_Hit;
            float l_MaxDistance = 0.6f;

            Debug.DrawRay(l_WallRay.transform.position, transform.forward * l_MaxDistance, Color.red);

            if (Physics.Raycast(l_Ray, out l_Hit, l_MaxDistance))
            {
                if (l_Hit.collider.CompareTag("World"))
                {
                    m_IsTouchingWall = true;
                    break;
                }
            }
        }

        if (m_IsTouchingWall && !IsGrounded() && (Time.time - m_LastWallJumpTime > m_WallJumpCooldown))
        {
            if (Input.GetKeyDown(m_JumpKeyCode) || m_PlayerControls.Gameplay.Jump.triggered)
            {
                PerformWallJump();
                m_LastWallJumpTime = Time.time;
            }
        }
    }

    private void PerformWallJump()
    {
        Vector3 l_WallJumpDirection = -transform.forward + Vector3.up;
        l_WallJumpDirection.Normalize();

        m_VerticalSpeed = 0.0f;
        m_VerticalSpeed += m_WallJumpForce;

        m_MovementDirection = l_WallJumpDirection * m_WallJumpForce;
    }

    private void UpadatePunch()
    {
        if ((Input.GetMouseButtonDown(m_PunchHitButton) && CanPunch()) || (m_PlayerControls.Gameplay.Punch.triggered && CanPunch()))
        {
            PunchCombo();
        }
    }

    private bool CanPunch()
    {
        AnimatorStateInfo stateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.tagHash != Animator.StringToHash("Punch") && !m_ShellCatched && m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Idle Break") == false &&
            !m_MarioLifeController.m_IsMarioDead)
        {
            return true;
        }

        return false;
    }

    private void PunchCombo()
    {
        m_IdleBreakTimer = 0.0f;
        m_Animator.SetTrigger("Punch");
        float l_DiffTime = Time.time - m_LastPunchTime;

        if (l_DiffTime <= m_PunchComboAvailableTime)
        {
            m_CurrentPunchId = (m_CurrentPunchId + 1) % 3;


            /*m_CurrentPunchId++;

            if (m_CurrentPunchId >= 3)
                m_CurrentPunchId = 0;*/
        }
        else
            m_CurrentPunchId = 0;

        m_Animator.SetInteger("PunchCombo", m_CurrentPunchId);
        SoundsManager.instance.PlaySoundClip(m_PunchSounds[m_CurrentPunchId], transform, 0.2f);

        m_LastPunchTime = Time.time;
    }

    public void EnableHitCollider(TPunchType PunchType, bool Active)
    {
        switch (PunchType)
        {
            case TPunchType.LEFT_HAND:
                m_LeftHandPunchHitCollider.SetActive(Active);
                break;
            case TPunchType.RIGHT_HAND:
                m_RightHandPunchHitCollider.SetActive(Active);
                break;
            case TPunchType.RIGHT_FOOT:
                m_RightFootPunchHitCollider.SetActive(Active);
                break;
        }
    }

    /*public void Step(int StepId)
    {
        SoundsManager.instance.PlaySoundClip(m_StepSound, transform, 0.2f);
    }*/

    public bool IsUpperHit(Transform GoombaTransform)
    {
        Vector3 l_EnemyDirection = transform.position - GoombaTransform.position;
        l_EnemyDirection.Normalize();

        float l_DotAngle = Vector3.Dot(l_EnemyDirection, Vector3.up);

        if (l_DotAngle >= Mathf.Cos(m_MaxAngleNeededToKillGoomba * Mathf.Deg2Rad) &&
            m_VerticalSpeed <= m_MinVerticalSpeedToKillGoomba)
            return true;

        return false;
    }

    public IEnumerator RepulseOverTime(Vector3 Direction, float Distance, float Speed)
    {
        float l_TraveledDistance = 0.0f;

        while (l_TraveledDistance < Distance)
        {
            float l_Step = Speed * Time.deltaTime;

            Vector3 l_marioMove = Direction * l_Step;
            m_CharacterController.Move(l_marioMove);

            l_TraveledDistance += l_Step;

            yield return null;
        }
    }

    private bool IsGrounded()
    {
        Vector3 l_Origin = m_CollisionGround.transform.position + Vector3.up * 0.1f; // Levanta un poco el raycast para evitar penetración
        float l_RayLength = 0.2f;
        float l_SphereRadius = 0.2f;

        Debug.DrawRay(l_Origin, Vector3.down * l_RayLength, Color.blue);

        // Realiza un SphereCast para detectar el suelo
        if (Physics.SphereCast(m_CollisionGround.transform.position, l_SphereRadius, Vector3.down, out RaycastHit hit, l_RayLength))
        {
            if (hit.collider.gameObject.CompareTag("World") || hit.collider.gameObject.CompareTag("Bridge"))
                return true;
        }

        return false;
    }

    private bool CanAttachElevator(Collider Elevator)
    {
        if (m_CurrentElevator != null)
        {
            return false;
        }

        return IsAttachableElevator(Elevator);
    }

    private bool IsAttachableElevator(Collider Elevator)
    {
        float l_DotAngle = Vector3.Dot(Elevator.transform.forward, Vector3.up);

        if (l_DotAngle >= Mathf.Cos(m_MaxAngleToAttachElevator * Mathf.Deg2Rad))
        {
            return true;
        }

        return false;
    }

    private void AttachElevator(Collider Elevator)
    {
        transform.SetParent(Elevator.transform.parent);
        m_CurrentElevator = Elevator;
    }

    private void DetachElevator()
    {
        m_CurrentElevator = null;
        transform.SetParent(null);
    }

    private void UpdateElevator()
    {
        if (m_CurrentElevator == null)
            return;

        if (!IsAttachableElevator(m_CurrentElevator))
            DetachElevator();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("Goomba") && IsUpperHit(hit.transform))
        {
            m_HitHeadEnemy.Play();
            hit.gameObject.GetComponent<GoombaController>().Death();
            m_VerticalSpeed = m_KillJumpVerticalSpeed;
        }
        else if (hit.collider.CompareTag("Koopa") && IsUpperHit(hit.transform))
        {
            m_HitHeadEnemy.Play();
            m_VerticalSpeed = m_KillJumpVerticalSpeed;
            KoopaController l_KoopaController = hit.gameObject.GetComponent<KoopaController>();

            if (l_KoopaController.m_ShellState == true)
            {
                l_KoopaController.EnableComponents(false);
                l_KoopaController.LaunchShell(transform.forward);
            }
            else
                l_KoopaController.m_ShellState = true;
        }
        else if (hit.gameObject.CompareTag("Bridge"))
        {
            hit.rigidbody.AddForceAtPosition(-hit.normal * m_BridgeForce, hit.point);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Elevator"))
        {
            if (CanAttachElevator(other))
                AttachElevator(other);
        }
        if (other.gameObject.CompareTag("Shell") && !m_ShellCatched)
        {
            m_CanCatchShell = true;
            m_Koopa = other.gameObject.transform.parent.gameObject.GetComponent<KoopaController>();
            m_PreviousParent = other.gameObject.transform.parent.gameObject.transform.parent;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Elevator") && other == m_CurrentElevator)
        {
            DetachElevator();
        }
        if (other.gameObject.CompareTag("Shell") && !m_ShellCatched)
        {
            m_CanCatchShell = false;
            m_Koopa = null;
        }
    }

    public void RestartGame()
    {
        transform.position = m_StartPosition;
        transform.rotation = m_StartRotation;
    }
}
