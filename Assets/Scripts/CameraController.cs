using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform m_FollowObject;

    private float m_Yaw = 0;
    private float m_Pitch = 0;
    private float m_Timer;
    [SerializeField] private float m_MaxTimeToGoToDefault;
    [SerializeField] private float m_TimeToDefaultPosition;

    public float m_MinPitch;
    public float m_MaxPitch;
    public float m_MinCameraDistance;
    public float m_MaxCameraDistance;
    public float m_YawSpeed = 0;
    public float m_PitchSpeed = 0;
    public LayerMask m_LayerMask;
    public float m_OffsetHit;

    private Vector3 m_PreviousPosition;
    private bool m_IsMoving;
    private float m_LerpTimerCameradDefaultPos = 0;
    private bool m_MovingToDefault = false;
    private MarioController m_MarioController;

    private PlayerControls m_PlayerControls;

    private void Awake()
    {
        m_PlayerControls = new PlayerControls();
    }
    private void OnEnable()
    {
        m_PlayerControls.Gameplay.Enable();
    }

    private void Start()
    {
        m_MarioController = GameManager.instance.GetPlayer().GetComponent<MarioController>();
    }
    private void LateUpdate()
    {
        float l_HorizontalAxis = Input.GetAxis("Mouse X");
        float l_VerticalAxis = Input.GetAxis("Mouse Y");

        if (((l_HorizontalAxis != 0 || l_VerticalAxis != 0) || m_MarioController.m_HasMovement) || m_PlayerControls.Gameplay.Look.IsPressed())
        {
            m_IsMoving = true;
            m_MovingToDefault = false;
            m_Timer = 0;
            m_LerpTimerCameradDefaultPos = 0;
        }
        else
        {
            m_IsMoving = false;
        }


        if (!m_IsMoving)
        {
            m_Timer += Time.deltaTime;

            if (m_Timer >= m_MaxTimeToGoToDefault)
            {
                m_MovingToDefault = true;
                Vector3 l_BackDirection = m_FollowObject.forward;
                float l_YawDesired = Mathf.Atan2(l_BackDirection.x, l_BackDirection.z) * Mathf.Rad2Deg;
                float l_PitchDesired = -22.0f;

                m_LerpTimerCameradDefaultPos += Time.deltaTime;

                float l_YawInRadians = Mathf.Lerp(m_Yaw * Mathf.Deg2Rad, l_YawDesired * Mathf.Deg2Rad, m_LerpTimerCameradDefaultPos / 3.0f);
                float l_PitchInRadians = Mathf.Lerp(m_Pitch * Mathf.Deg2Rad, l_PitchDesired * Mathf.Deg2Rad, m_LerpTimerCameradDefaultPos / 3.0f);


                m_Yaw = NormalizeAngle(l_YawInRadians * Mathf.Rad2Deg);
                m_Pitch = l_PitchInRadians * Mathf.Rad2Deg;

                if (m_LerpTimerCameradDefaultPos >= 3.0f)
                {
                    m_MovingToDefault = false;
                    m_LerpTimerCameradDefaultPos = 0;
                }
            }
        }

        if (!m_MovingToDefault)
        {
            if (m_PlayerControls.Gameplay.Look.IsPressed())
            {
                Vector2 l_AxisGamepad = m_PlayerControls.Gameplay.Look.ReadValue<Vector2>();
                l_AxisGamepad.Normalize();
                m_Yaw += l_AxisGamepad.x * m_YawSpeed / 3.60f * Time.deltaTime;
                m_Pitch += l_AxisGamepad.y * m_PitchSpeed / 3.60f * Time.deltaTime;
                m_Pitch = Mathf.Clamp(m_Pitch, m_MinPitch, m_MaxPitch);
            }
            else
            {
                m_Yaw += l_HorizontalAxis * m_YawSpeed * Time.deltaTime;
                m_Pitch += l_VerticalAxis * m_PitchSpeed * Time.deltaTime;
                m_Pitch = Mathf.Clamp(m_Pitch, m_MinPitch, m_MaxPitch);
            }
            m_Yaw = NormalizeAngle(m_Yaw);
        }

        float l_YawInRadiansFinal = m_Yaw * Mathf.Deg2Rad;
        float l_PitchInRadiansFinal = m_Pitch * Mathf.Deg2Rad;

        Vector3 l_CameraForward = new Vector3(
            Mathf.Sin(l_YawInRadiansFinal) * Mathf.Cos(l_PitchInRadiansFinal),
            Mathf.Sin(l_PitchInRadiansFinal),
            Mathf.Cos(l_YawInRadiansFinal) * Mathf.Cos(l_PitchInRadiansFinal)
        );

        float l_DistanceToPlayer = Mathf.Clamp((m_FollowObject.position - transform.position).magnitude, m_MinCameraDistance, m_MaxCameraDistance);
        Vector3 l_DesiredPosition = m_FollowObject.position - l_CameraForward * l_DistanceToPlayer;


        Ray l_Ray = new Ray(m_FollowObject.position, -l_CameraForward);
        RaycastHit l_Hit;

        if (Physics.Raycast(l_Ray, out l_Hit, l_DistanceToPlayer, m_LayerMask.value))
        {
            l_DesiredPosition = l_Hit.point + l_CameraForward * m_OffsetHit;
        }

        transform.position = l_DesiredPosition;
        transform.LookAt(m_FollowObject.position);
    }

    private float NormalizeAngle(float angle)
    {
        while (angle > 180) angle -= 360;
        while (angle < -180) angle += 360;
        return angle;
    }
}
