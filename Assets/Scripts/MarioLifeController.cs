using System;
using System.Collections;
using UnityEngine;

public class MarioLifeController : MonoBehaviour
{
    public float m_MaxHealth;
    public float m_MinHealth;
    public float m_CurrentHealth;
    public bool m_IsMarioHit;
    public int m_MarioLifes;
    public bool m_IsMarioDead;

    private Animator m_MarioAnimator;
    private bool m_IsBackToMenuCalled;

    public static Action<float> OnPlayerHit;
    public static Action OnPlayerDied;

    [SerializeField] private float m_ImmunityTime;

    [Header("Sounds")]
    [SerializeField] private AudioClip m_HitSound;
    [SerializeField] private AudioClip m_BowserSound;
    [SerializeField] private AudioClip m_DeathSound;

    private void OnEnable()
    {
        CoinItem.OnCoinPicked += IncreaseHealth;
    }

    private void OnDisable()
    {
        CoinItem.OnCoinPicked -= IncreaseHealth;
    }

    private void Start()
    {
        m_MarioAnimator = GetComponent<Animator>();

        m_CurrentHealth = m_MaxHealth;
        m_IsMarioDead = false;
        m_IsMarioHit = false;
        m_IsBackToMenuCalled = false;
    }

    public IEnumerator PlayerHit(float Damage)
    {
        m_IsMarioHit = true;
        m_CurrentHealth -= Damage;
        OnPlayerHit?.Invoke(Damage);
        SoundsManager.instance.PlaySoundClip(m_HitSound, transform, 0.2f);

        if (m_CurrentHealth <= 0)
        {
            m_MarioLifes--;
            yield return new WaitForSeconds(0.01f);
            m_CurrentHealth = m_MaxHealth;
            Die();
        }

        yield return new WaitForSeconds(m_ImmunityTime);
        m_IsMarioHit = false;
    }

    private void IncreaseHealth()
    {
        if (m_CurrentHealth < m_MaxHealth)
            m_CurrentHealth++;
    }

    public void Die()
    {
        m_IsMarioDead = true;
        m_MarioAnimator.SetTrigger("Die");
        OnPlayerDied?.Invoke();
        SoundsManager.instance.PlaySoundClip(m_DeathSound, transform, 0.2f);
        SoundsManager.instance.PlaySoundClip(m_BowserSound, transform, 0.2f);
        m_IsMarioHit = false;
    }
}
