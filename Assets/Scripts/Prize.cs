using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Prize : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Click click;
    [SerializeField] private Slot slot;
    [SerializeField] private CoinSystem coinSystem;
    [SerializeField] private GradeDatabase gradeDatabase;

    [Header("Prize UI")]
    [SerializeField] private GameObject prizePanel;     // 기본 비활성화 상태의 패널
    [SerializeField] private Image prizeImage;          // 당첨 아이템 이미지
    [SerializeField] private Image prizeGradeImage;     // 당첨 등급 이미지
    [SerializeField] private TextMeshProUGUI prizeNameText;   // 아이템 ��름 표시
    [SerializeField] private TextMeshProUGUI prizeDescText;   // 아이템 설명 표시
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button rejectButton;

    private Sprite[] prizeSprites;
    private Sprite selectedPrize;

    private void Start()
    {
        if (gradeDatabase != null)
            prizeSprites = gradeDatabase.GetAllItemSprites();

        if (prizePanel != null)
            prizePanel.SetActive(false);

        if (click != null)
            click.OnEggBrokenEvent += OnEggBroken;

        if (slot != null)
            slot.OnSlotChanged += UpdateConfirmButton;

        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirm);

        if (rejectButton != null)
            rejectButton.onClick.AddListener(OnReject);
    }

    private void OnDestroy()
    {
        if (click != null)
            click.OnEggBrokenEvent -= OnEggBroken;
        if (slot != null)
            slot.OnSlotChanged -= UpdateConfirmButton;
    }

    private void OnEggBroken()
    {
        if (prizeSprites == null || prizeSprites.Length == 0) return;

        click.SetEggActive(false);

        // 랜덤 보상 선택
        selectedPrize = prizeSprites[Random.Range(0, prizeSprites.Length)];

        // 아이템 이미지
        if (prizeImage != null)
        {
            prizeImage.sprite = selectedPrize;
            prizeImage.preserveAspect = true;
        }

        // 등급 이미지 + 이름/설명 (스프라이트 이름으로 파싱)
        if (gradeDatabase != null)
        {
            if (prizeGradeImage != null)
            {
                GradeType grade = Grade.Parse(selectedPrize);
                prizeGradeImage.sprite = gradeDatabase.GetGradeSprite(grade);
                prizeGradeImage.preserveAspect = true;
            }

            GradeType parsedGrade = Grade.Parse(selectedPrize);
            int parsedNumber = Grade.ParseNumber(selectedPrize);
            Debug.Log($"[Prize] sprite.name=\"{selectedPrize.name}\" → Grade={parsedGrade}, Number={parsedNumber}");

            GradeItem item = gradeDatabase.GetItem(selectedPrize);
            if (item != null)
            {
                Debug.Log($"[Prize] Found item: name=\"{item.itemName}\", desc=\"{item.description}\"");
                if (prizeNameText != null) prizeNameText.text = item.itemName;
                if (prizeDescText != null) prizeDescText.text = item.description;
            }
            else
            {
                Debug.LogWarning($"[Prize] GradeItem not found for Grade={parsedGrade}, Number={parsedNumber}");
            }
        }

        UpdateConfirmButton();
        prizePanel.SetActive(true);
    }

    private void UpdateConfirmButton()
    {
        if (confirmButton != null)
            confirmButton.interactable = slot != null && slot.HasEmptySlot();
    }

    private void OnConfirm()
    {
        if (slot == null || selectedPrize == null) return;
        if (!slot.HasEmptySlot()) return;

        slot.AddToSlot(selectedPrize);
        ClosePrize();
    }

    private void OnReject()
    {
        // reject 시 아이템 가격만큼 코인 지급
        if (coinSystem != null && gradeDatabase != null && selectedPrize != null)
        {
            GradeItem item = gradeDatabase.GetItem(selectedPrize);
            if (item != null && item.price > 0)
            {
                int bonus = Mathf.RoundToInt(item.price * coinSystem.GetCoinBonusRate());
                coinSystem.AddCoins(item.price + bonus);
            }
        }

        ClosePrize();
    }

    private void ClosePrize()
    {
        prizePanel.SetActive(false);
        selectedPrize = null;

        click.SetEggActive(true);
        click.ResetEgg();
    }
}
