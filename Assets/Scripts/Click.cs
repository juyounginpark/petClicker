using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Click : MonoBehaviour
{
    [Header("Egg Settings")]
    [SerializeField] private float maxHp = 100f;
    [SerializeField] private float damagePerClick = 10f;

    [Header("Image Database")]
    [SerializeField] private Sprite[] eggSprites; // index 0 = 온전한 상태, 마지막 = 완전히 깨진 상태

    [Header("UI References")]
    [SerializeField] private Button eggButton;
    [SerializeField] private Image eggImage;
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("Damage Popup")]
    [SerializeField] private GameObject damagePopupPrefab;
    [SerializeField] private Canvas popupCanvas;
    [SerializeField] private int popupPoolSize = 10;
    [SerializeField] private float popupFontSize = 48f;
    [SerializeField] private float popupFallSpeed = 120f;
    [SerializeField] private float popupDuration = 1f;
    [SerializeField] private float popupPunchScale = 1.5f;
    [SerializeField] private float popupPunchDuration = 0.15f;

    [Header("Critical Hit")]
    [SerializeField] private float baseCritChance = 0.3f;  // 기본 30%
    [SerializeField] private float critMultiplier = 2f;     // 치명타 배율

    [Header("Wobble Effect")]
    [SerializeField] private float wobbleAngle = 15f;
    [SerializeField] private float wobbleDuration = 0.4f;
    [SerializeField] private int wobbleCount = 3;

    public event System.Action OnEggBrokenEvent;

    private float currentHp;
    private float prevHp;
    private bool isWobbling;
    private float bonusCritChance;

    // 팝업 오브젝트 풀
    private List<GameObject> popupPool;

    private void Start()
    {
        currentHp = maxHp;
        prevHp = maxHp;

        if (eggButton != null)
            eggButton.onClick.AddListener(OnEggClick);

        InitPopupPool();
        UpdateEggImage();
        UpdateHpText();
    }

    private void InitPopupPool()
    {
        popupPool = new List<GameObject>();
        if (damagePopupPrefab == null || popupCanvas == null) return;

        for (int i = 0; i < popupPoolSize; i++)
        {
            GameObject obj = Instantiate(damagePopupPrefab, popupCanvas.transform);
            obj.name = $"DmgPopup_{i}";

            CanvasGroup cg = obj.GetComponent<CanvasGroup>();
            if (cg == null) cg = obj.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            cg.interactable = false;

            obj.SetActive(false);
            popupPool.Add(obj);
        }
    }

    private GameObject GetPopupFromPool()
    {
        for (int i = 0; i < popupPool.Count; i++)
        {
            if (!popupPool[i].activeInHierarchy)
                return popupPool[i];
        }
        // 풀이 부족하면 하나 더 생성
        GameObject obj = Instantiate(damagePopupPrefab, popupCanvas.transform);
        obj.name = $"DmgPopup_{popupPool.Count}";
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.interactable = false;
        obj.SetActive(false);
        popupPool.Add(obj);
        return obj;
    }

    private void OnEggClick()
    {
        if (currentHp <= 0f) return;

        float damage = damagePerClick;
        bool isCrit = Random.value < (baseCritChance + bonusCritChance);
        if (isCrit)
            damage *= critMultiplier;

        ApplyDamage(damage, isCrit);

        if (!isWobbling)
            StartCoroutine(WobbleEffect());
    }

    public void AddCritChance(float amount)
    {
        bonusCritChance += amount;
    }

    public void ApplyDamage(float damage, bool isCrit = false)
    {
        if (damage <= 0f || currentHp <= 0f) return;

        prevHp = currentHp;
        currentHp -= damage;
        if (currentHp < 0f) currentHp = 0f;

        float actualDamage = prevHp - currentHp;

        UpdateEggImage();
        UpdateHpText();

        if (actualDamage > 0f)
            SpawnDamagePopup(actualDamage, isCrit);

        if (currentHp <= 0f)
            OnEggBroken();
    }

    private void UpdateHpText()
    {
        if (hpText != null)
            hpText.text = $"{currentHp}";
    }

    private void SpawnDamagePopup(float damage, bool isCrit = false)
    {
        if (damagePopupPrefab == null || popupCanvas == null || eggImage == null) return;

        GameObject popup = GetPopupFromPool();

        RectTransform popupRt = popup.GetComponent<RectTransform>();
        popupRt.anchorMin = new Vector2(0.5f, 0.5f);
        popupRt.anchorMax = new Vector2(0.5f, 0.5f);
        popupRt.pivot = new Vector2(0.5f, 0.5f);
        popupRt.localScale = Vector3.one;

        // 알 영역 내 균등 랜덤 위치
        RectTransform eggRt = eggImage.rectTransform;
        Vector3[] eggCorners = new Vector3[4];
        eggRt.GetWorldCorners(eggCorners);

        float rx = Random.Range(0f, 1f);
        float ry = Random.Range(0f, 1f);
        Vector3 worldPos = Vector3.Lerp(
            Vector3.Lerp(eggCorners[0], eggCorners[3], rx),
            Vector3.Lerp(eggCorners[1], eggCorners[2], rx),
            ry
        );

        RectTransform canvasRt = popupCanvas.GetComponent<RectTransform>();
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(popupCanvas.worldCamera, worldPos);
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, screenPoint, popupCanvas.worldCamera, out localPoint);
        popupRt.anchoredPosition = localPoint;

        float fontSize = isCrit ? popupFontSize * 2f : popupFontSize;

        TextMeshProUGUI tmp = popup.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = $"-{damage}";
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Overflow;
        }

        popupRt.sizeDelta = new Vector2(300f, fontSize * 1.5f);

        popup.SetActive(true);
        StartCoroutine(AnimateDamagePopup(popup));
    }

    private IEnumerator AnimateDamagePopup(GameObject popup)
    {
        RectTransform rt = popup.GetComponent<RectTransform>();
        TextMeshProUGUI tmp = popup.GetComponent<TextMeshProUGUI>();
        if (rt == null || tmp == null)
        {
            popup.SetActive(false);
            yield break;
        }

        Color originalColor = tmp.color;

        // 강조: 스케일 펀치
        float punchElapsed = 0f;
        while (punchElapsed < popupPunchDuration)
        {
            punchElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(punchElapsed / popupPunchDuration);
            float scale;
            if (t < 0.5f)
                scale = Mathf.Lerp(1f, popupPunchScale, Mathf.SmoothStep(0f, 1f, t * 2f));
            else
                scale = Mathf.Lerp(popupPunchScale, 1f, Mathf.SmoothStep(0f, 1f, (t - 0.5f) * 2f));
            rt.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }
        rt.localScale = Vector3.one;

        // 아래로 떨어지며 페이드아웃
        float elapsed = 0f;
        while (elapsed < popupDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / popupDuration;

            rt.anchoredPosition += new Vector2(0f, -popupFallSpeed * Time.deltaTime);

            float alpha = 1f - Mathf.Pow(progress, 2f);
            tmp.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            yield return null;
        }

        // 풀로 반환 (Destroy 대신 비활성화)
        tmp.color = originalColor;
        popup.SetActive(false);
    }

    private void UpdateEggImage()
    {
        if (eggSprites == null || eggSprites.Length == 0 || eggImage == null) return;

        float hpRatio = currentHp / maxHp;
        float damaged = 1f - hpRatio;

        int maxIndex = eggSprites.Length - 1;
        int index = Mathf.RoundToInt(damaged * maxIndex);
        index = Mathf.Clamp(index, 0, maxIndex);

        eggImage.sprite = eggSprites[index];
    }

    private IEnumerator WobbleEffect()
    {
        isWobbling = true;

        RectTransform rt = eggImage.rectTransform;
        Quaternion originalRotation = rt.localRotation;
        float halfCycle = wobbleDuration / (wobbleCount * 2);

        for (int i = 0; i < wobbleCount * 2; i++)
        {
            float startAngle = rt.localEulerAngles.z;
            if (startAngle > 180f) startAngle -= 360f;

            float targetAngle = (i % 2 == 0) ? wobbleAngle : -wobbleAngle;
            if (i == wobbleCount * 2 - 1) targetAngle = 0f;

            float damping = 1f - ((float)i / (wobbleCount * 2));
            targetAngle *= damping;

            float t = 0f;
            while (t < halfCycle)
            {
                t += Time.deltaTime;
                float progress = Mathf.Clamp01(t / halfCycle);
                float smooth = Mathf.SmoothStep(0f, 1f, progress);
                float angle = Mathf.Lerp(startAngle, targetAngle, smooth);
                rt.localRotation = Quaternion.Euler(0f, 0f, angle);
                yield return null;
            }
        }

        rt.localRotation = originalRotation;
        isWobbling = false;
    }

    private void OnEggBroken()
    {
        if (eggButton != null)
            eggButton.interactable = false;

        OnEggBrokenEvent?.Invoke();
    }

    public void SetEggActive(bool active)
    {
        if (eggImage != null)
            eggImage.gameObject.SetActive(active);
        if (eggButton != null)
            eggButton.gameObject.SetActive(active);
    }

    public void ResetEgg()
    {
        currentHp = maxHp;
        prevHp = maxHp;
        if (eggButton != null)
            eggButton.interactable = true;
        UpdateEggImage();
        UpdateHpText();
    }
}
