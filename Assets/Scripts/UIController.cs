using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("HUD - Health - Lifes")]
    [SerializeField] private MarioLifeController m_MarioLifeController;
    [SerializeField] private GameObject m_HUD;
    [SerializeField] private Animator m_HUDAnimator;
    [SerializeField] private Image m_PowerDiscImage;
    [SerializeField] private Image m_DamageGlossImage;
    [SerializeField] private Image m_EmptyDiscImage;
    [SerializeField] private TMP_Text m_MarioLifesText;
    [SerializeField] private float m_TimeToHideHUD;
    private float m_Timer = 0;

    [SerializeField] private TMP_Text m_CoinsText;
    private int m_CoinCount;

    [SerializeField] private TMP_Text m_StarsText;
    private int m_StarsCount;

    [Header("Death")]
    [SerializeField] private GameObject m_DeathUI;
    public static Action OnMarioRevive;

    private void OnEnable()
    {
        CoinItem.OnCoinPicked += IncreaseCoinCount;
        StarItem.OnStarPicked += IncreaseStarsCount;
        MarioLifeController.OnPlayerHit += DecreaseHealthDisc;
        MarioLifeController.OnPlayerDied += MarioDied;
    }

    private void OnDisable()
    {
        CoinItem.OnCoinPicked -= IncreaseCoinCount;
        StarItem.OnStarPicked -= IncreaseStarsCount;
        MarioLifeController.OnPlayerHit -= DecreaseHealthDisc;
        MarioLifeController.OnPlayerDied -= MarioDied;
    }

    private void Start()
    {
        m_HUD.SetActive(true);
        m_DeathUI.SetActive(false);
    }

    private void Update()
    {
        CheckMarioHealth();

        m_Timer += Time.deltaTime;

        if (m_Timer >= m_TimeToHideHUD)
        {
            HideHUD();
        }

        if (m_MarioLifeController.m_IsMarioHit)
        {
            ShowHUD();
        }
    }

    private void CheckMarioHealth()
    {
        m_MarioLifeController.m_MarioLifes = Mathf.Max(m_MarioLifeController.m_MarioLifes, 0);
        m_MarioLifesText.text = "<sprite index=" + m_MarioLifeController.m_MarioLifes + ">";

        if (m_MarioLifeController.m_CurrentHealth <= 0)
        {
            m_PowerDiscImage.fillAmount = 1.0f;
        }
        else if (m_MarioLifeController.m_CurrentHealth >= 8.0f)
        {
            string l_BlueHexColor = "#0000FF";
            ColorUtility.TryParseHtmlString(l_BlueHexColor, out Color NewColor);
            m_PowerDiscImage.color = NewColor;
            m_DamageGlossImage.color = NewColor;

            string l_DarkBlueHexColor = "#000080";
            ColorUtility.TryParseHtmlString(l_DarkBlueHexColor, out Color NewColor2);
            m_EmptyDiscImage.color = NewColor2;
        }
        else if (m_MarioLifeController.m_CurrentHealth <= 7.0f && m_MarioLifeController.m_CurrentHealth >= 4.0f)
        {
            string l_YellowHexColor = "#FAFF00";
            ColorUtility.TryParseHtmlString(l_YellowHexColor, out Color NewColor);
            m_PowerDiscImage.color = NewColor;
            m_DamageGlossImage.color = NewColor;

            string l_DarkYellowHexColor = "#C8BE00";
            ColorUtility.TryParseHtmlString(l_DarkYellowHexColor, out Color NewColor2);
            m_EmptyDiscImage.color = NewColor2;
        }
        else if (m_MarioLifeController.m_CurrentHealth <= 3.0f)
        {
            string l_RedHexColor = "#FF0000";
            ColorUtility.TryParseHtmlString(l_RedHexColor, out Color NewColor);
            m_PowerDiscImage.color = NewColor;
            m_DamageGlossImage.color = NewColor;

            string l_DarkRedHexColor = "#800000";
            ColorUtility.TryParseHtmlString(l_DarkRedHexColor, out Color NewColor2);
            m_EmptyDiscImage.color = NewColor2;
        }
    }

    private void ShowHUD()
    {
        m_Timer = 0;
        m_HUDAnimator.SetBool("ShowHUD", true);
        m_HUDAnimator.SetBool("HideHUD", false);
    }

    private void HideHUD()
    {
        m_HUDAnimator.SetBool("ShowHUD", false);
        m_HUDAnimator.SetBool("HideHUD", true);
    }

    private void DecreaseHealthDisc(float l_Damage)
    {
        m_PowerDiscImage.fillAmount -= (1.0f / 8.0f) * l_Damage;
    }

    private void IncreaseCoinCount()
    {
        if (m_MarioLifeController.m_CurrentHealth < 8.0f)
            ShowHUD();

        m_CoinCount++;
        m_PowerDiscImage.fillAmount += (1.0f / 8.0f);

        string l_Result = "";

        foreach (var number in m_CoinCount.ToString())
        {
            int spriteIndex = number - '0';

            l_Result += "<sprite index=" + spriteIndex + ">";
        }
        m_CoinsText.text = l_Result;
    }

    private void IncreaseStarsCount()
    {
        m_StarsCount++;
        string l_Result = "";

        foreach (var number in m_StarsCount.ToString())
        {
            int spriteIndex = number - '0';

            l_Result += "<sprite index=" + spriteIndex + ">";
        }
        m_StarsText.text = l_Result;
    }

    private void MarioDied()
    {
        StartCoroutine(MarioDiedCoroutine());
    }

    private IEnumerator MarioDiedCoroutine()
    {
        m_HUDAnimator.SetBool("HideHUD", false);
        m_HUDAnimator.SetBool("ShowHUD", false);
        m_DeathUI.SetActive(true);
        m_HUD.SetActive(false);
        m_HUDAnimator.SetTrigger("Death");

        yield return new WaitForSeconds(2.0f);
        OnMarioRevive?.Invoke();
        yield return new WaitForSeconds(2.0f);
        m_HUDAnimator.SetTrigger("Revive");

        yield return new WaitForSeconds(1.5f);
        m_DeathUI.SetActive(false);
        m_HUD.SetActive(true);
    }

    public void PlaySound(AudioClip AudioClip)
    {
        SoundsManager.instance.PlaySoundClip(AudioClip, transform, 0.2f);
    }
}
