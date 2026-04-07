using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public enum SlotState
{
    Empty,
    Filled
}

public class Slot : MonoBehaviour
{
    [SerializeField] private GameObject[] slotPrefabs;
    [SerializeField] private Transform slotParent;
    [SerializeField] private GradeDatabase gradeDatabase;
    [SerializeField] private Click click;
    [SerializeField] private CoinSystem coinSystem;

    [Header("Explain Popup")]
    [SerializeField] private GameObject explainPrefab; // Explain 프리팹
    [SerializeField] private Transform explainSpawnParent; // 생성될 부모 (Canvas)

    private SlotState[] states;
    private Image[] itemImages;
    private Image[] gradeImages;
    private GameObject[] itemObjects;
    private GameObject[] gradeObjects;
    private ItemEffect[] activeEffects;
    private Sprite[] slotSprites;
    private RectTransform[] slotRects;

    // Explain
    private GameObject explainInstance;
    private RectTransform explainRt;
    private TextMeshProUGUI nameText;
    private TextMeshProUGUI descText;
    private TextMeshProUGUI rejectText;
    private Button rejectButton;
    private RectTransform rejectRt;
    private Image explainItemImage;
    private int selectedSlotIndex = -1;
    private int explainOpenFrame = -1;

    public event System.Action OnSlotChanged;

    public int Count => slotPrefabs != null ? slotPrefabs.Length : 0;

    private void Awake()
    {
        InitSlots();
        InitExplain();
    }

    private void InitSlots()
    {
        int count = slotPrefabs.Length;
        states = new SlotState[count];
        itemImages = new Image[count];
        gradeImages = new Image[count];
        itemObjects = new GameObject[count];
        gradeObjects = new GameObject[count];
        activeEffects = new ItemEffect[count];
        slotSprites = new Sprite[count];
        slotRects = new RectTransform[count];

        for (int i = 0; i < count; i++)
        {
            if (slotPrefabs[i] == null) continue;

            GameObject obj = Instantiate(slotPrefabs[i], slotParent);
            obj.name = $"Slot_{i}";
            slotRects[i] = obj.GetComponent<RectTransform>();

            Transform itemChild = obj.transform.Find("Item");
            if (itemChild != null)
            {
                itemObjects[i] = itemChild.gameObject;
                itemImages[i] = itemChild.GetComponent<Image>();
                itemImages[i].raycastTarget = false;
                itemObjects[i].SetActive(false);
            }

            Transform gradeChild = obj.transform.Find("Grade");
            if (gradeChild != null)
            {
                gradeObjects[i] = gradeChild.gameObject;
                gradeImages[i] = gradeChild.GetComponent<Image>();
                gradeImages[i].raycastTarget = false;
                gradeObjects[i].SetActive(false);
            }

            // 클릭 이벤트 등록
            int index = i;
            EventTrigger trigger = obj.GetComponent<EventTrigger>();
            if (trigger == null) trigger = obj.AddComponent<EventTrigger>();

            EventTrigger.Entry clickEntry = new EventTrigger.Entry();
            clickEntry.eventID = EventTriggerType.PointerClick;
            clickEntry.callback.AddListener((data) => OnSlotClick(index, (PointerEventData)data));
            trigger.triggers.Add(clickEntry);

            states[i] = SlotState.Empty;
        }
    }

    private void InitExplain()
    {
        if (explainPrefab == null || explainSpawnParent == null) return;

        explainInstance = Instantiate(explainPrefab, explainSpawnParent);
        explainRt = explainInstance.GetComponent<RectTransform>();

        foreach (TextMeshProUGUI tmp in explainInstance.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            string parentName = tmp.transform.parent.name;
            if (parentName == "Name") nameText = tmp;
            else if (parentName == "Description") descText = tmp;
            else if (parentName == "Reject") rejectText = tmp;
        }

        Transform rejectChild = explainInstance.transform.Find("Reject");
        if (rejectChild != null)
        {
            rejectRt = rejectChild.GetComponent<RectTransform>();
            rejectButton = rejectChild.GetComponent<Button>();
            if (rejectButton != null)
                rejectButton.onClick.AddListener(OnRejectClick);
        }

        // Image 자식 위에 아이템 이미지 오버레이 생성
        Transform imageChild = explainInstance.transform.Find("Image");
        if (imageChild != null)
        {
            GameObject itemOverlay = new GameObject("ItemOverlay", typeof(RectTransform), typeof(Image));
            itemOverlay.transform.SetParent(imageChild, false);

            RectTransform rt = itemOverlay.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = new Vector3(3.5f, 3.5f, 1f);

            explainItemImage = itemOverlay.GetComponent<Image>();
            explainItemImage.preserveAspect = true;
            explainItemImage.raycastTarget = false;
        }

        explainInstance.SetActive(false);
    }

    private void Update()
    {
        // Explain이 열려있을 때 다른 곳 클릭하면 닫기
        if (explainInstance != null && explainInstance.activeSelf
            && Input.GetMouseButtonDown(0) && Time.frameCount > explainOpenFrame)
        {
            bool insideExplain = RectTransformUtility.RectangleContainsScreenPoint(explainRt, Input.mousePosition, null);
            bool insideReject = rejectRt != null && RectTransformUtility.RectangleContainsScreenPoint(rejectRt, Input.mousePosition, null);
            if (!insideExplain && !insideReject)
                CloseExplain();
        }
    }

    private void OnSlotClick(int index, PointerEventData eventData)
    {
        Debug.Log($"[Slot] OnSlotClick called: index={index}, state={states[index]}, explainInstance={explainInstance != null}");
        if (explainInstance == null) return;

        // 이미 열려있으면 닫기
        if (selectedSlotIndex == index && explainInstance.activeSelf)
        {
            CloseExplain();
            return;
        }

        if (states[index] != SlotState.Filled) return;
        if (gradeDatabase == null) return;

        selectedSlotIndex = index;

        GradeItem item = gradeDatabase.GetItem(slotSprites[index]);
        if (item == null) return;

        if (nameText != null) nameText.text = item.itemName;
        if (descText != null) descText.text = item.description;
        if (rejectText != null) rejectText.text = $"판매:C{item.price}";

        if (explainItemImage != null && item.itemSprite != null)
        {
            explainItemImage.sprite = item.itemSprite;
            explainItemImage.SetNativeSize();
        }

        explainInstance.SetActive(true);
        explainInstance.transform.SetAsLastSibling();
        explainOpenFrame = Time.frameCount;
    }

    private void OnRejectClick()
    {
        if (selectedSlotIndex < 0 || selectedSlotIndex >= states.Length) return;
        if (states[selectedSlotIndex] != SlotState.Filled) return;

        if (coinSystem != null && gradeDatabase != null)
        {
            GradeItem item = gradeDatabase.GetItem(slotSprites[selectedSlotIndex]);
            if (item != null && item.price > 0)
            {
                int bonus = Mathf.RoundToInt(item.price * coinSystem.GetCoinBonusRate());
                coinSystem.AddCoins(item.price + bonus);
            }
        }

        ClearSlot(selectedSlotIndex);
        CloseExplain();
    }

    private void CloseExplain()
    {
        selectedSlotIndex = -1;
        if (explainInstance != null)
            explainInstance.SetActive(false);
    }

    public bool AddToSlot(Sprite itemSprite)
    {
        for (int i = 0; i < states.Length; i++)
        {
            if (states[i] == SlotState.Empty)
            {
                slotSprites[i] = itemSprite;

                if (itemImages[i] != null)
                {
                    itemImages[i].sprite = itemSprite;
                    itemImages[i].preserveAspect = true;
                    itemObjects[i].SetActive(true);
                }

                if (gradeImages[i] != null && gradeDatabase != null)
                {
                    GradeType grade = Grade.Parse(itemSprite);
                    gradeImages[i].sprite = gradeDatabase.GetGradeSprite(grade);
                    gradeImages[i].preserveAspect = true;
                    gradeObjects[i].SetActive(true);
                }

                ActivateEffect(i, itemSprite);

                states[i] = SlotState.Filled;
                OnSlotChanged?.Invoke();
                return true;
            }
        }
        return false;
    }

    public bool HasEmptySlot()
    {
        for (int i = 0; i < states.Length; i++)
        {
            if (states[i] == SlotState.Empty) return true;
        }
        return false;
    }

    private void ActivateEffect(int slotIndex, Sprite itemSprite)
    {
        if (gradeDatabase == null) return;

        GradeItem gradeItem = gradeDatabase.GetItem(itemSprite);
        if (gradeItem == null || gradeItem.scriptPrefab == null) return;

        System.Type effectType = gradeItem.scriptPrefab.GetType();
        ItemEffect effect = gameObject.AddComponent(effectType) as ItemEffect;
        if (effect != null)
        {
            effect.Init(click, coinSystem);
            effect.Activate();
            activeEffects[slotIndex] = effect;
        }
    }

    private void DeactivateEffect(int slotIndex)
    {
        if (activeEffects[slotIndex] != null)
        {
            activeEffects[slotIndex].Deactivate();
            Destroy(activeEffects[slotIndex]);
            activeEffects[slotIndex] = null;
        }
    }

    public SlotState GetSlotState(int index)
    {
        if (index < 0 || index >= states.Length) return SlotState.Empty;
        return states[index];
    }

    public void ClearSlot(int index)
    {
        if (index < 0 || index >= states.Length) return;
        if (states[index] == SlotState.Empty) return;

        DeactivateEffect(index);

        if (itemObjects[index] != null)
            itemObjects[index].SetActive(false);

        if (gradeObjects[index] != null)
            gradeObjects[index].SetActive(false);

        slotSprites[index] = null;
        states[index] = SlotState.Empty;
        OnSlotChanged?.Invoke();
    }
}
