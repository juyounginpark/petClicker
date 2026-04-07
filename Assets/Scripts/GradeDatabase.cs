using UnityEngine;

[System.Serializable]
public class GradeData
{
    public Sprite gradeSprite;    // 등급 표시 이미지 (A, B, C 아이콘)
    public GradeItem[] items;     // 해당 ��급의 아이템 목록
}

[CreateAssetMenu(fileName = "GradeDatabase", menuName = "Game/Grade Database")]
public class GradeDatabase : ScriptableObject
{
    [Header("Grade A")]
    [SerializeField] private GradeData gradeA;

    [Header("Grade B")]
    [SerializeField] private GradeData gradeB;

    [Header("Grade C")]
    [SerializeField] private GradeData gradeC;

    public GradeData GetGradeData(GradeType type)
    {
        switch (type)
        {
            case GradeType.A: return gradeA;
            case GradeType.B: return gradeB;
            case GradeType.C: return gradeC;
            default:          return gradeC;
        }
    }

    public Sprite GetGradeSprite(GradeType type)
    {
        return GetGradeData(type)?.gradeSprite;
    }

    /// <summary>
    /// 등급 + 번호로 GradeItem 검색. (예: GradeType.C, 1 → C1 아이템)
    /// </summary>
    public GradeItem GetItem(GradeType type, int number)
    {
        GradeData data = GetGradeData(type);
        if (data?.items == null) return null;

        for (int i = 0; i < data.items.Length; i++)
        {
            if (data.items[i].number == number)
                return data.items[i];
        }
        return null;
    }

    /// <summary>
    /// 스프라이트에서 직접 GradeItem 검색. (예: "C1" 스프라이트 → C등급 1번 아이��)
    /// </summary>
    public GradeItem GetItem(Sprite sprite)
    {
        GradeType type = Grade.Parse(sprite);
        int number = Grade.ParseNumber(sprite);
        return GetItem(type, number);
    }

    /// <summary>
    /// 모든 등급의 itemSprite를 모아서 반환. (Prize 랜덤 선택용)
    /// </summary>
    public Sprite[] GetAllItemSprites()
    {
        var list = new System.Collections.Generic.List<Sprite>();
        CollectSprites(gradeA, list);
        CollectSprites(gradeB, list);
        CollectSprites(gradeC, list);
        return list.ToArray();
    }

    private void CollectSprites(GradeData data, System.Collections.Generic.List<Sprite> list)
    {
        if (data?.items == null) return;
        for (int i = 0; i < data.items.Length; i++)
        {
            if (data.items[i].itemSprite != null)
                list.Add(data.items[i].itemSprite);
        }
    }
}
