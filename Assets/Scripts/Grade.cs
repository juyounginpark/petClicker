using UnityEngine;

public enum GradeType
{
    C,
    B,
    A
}

[System.Serializable]
public class GradeItem
{
    public int number;                          // 아이템 번호 (C1 → 1)
    public string itemName;                     // 아이템 이름
    public Sprite itemSprite;                   // 아이템 이미지
    [TextArea] public string description;       // 설명 텍스트
    public int price;                           // 판매 가격 (reject 시 코인 획득)
    public ItemEffect scriptPrefab;             // 붙일 아이템 효과 스크립트
}

public static class Grade
{
    /// <summary>
    /// 스프라이트 이름 첫 글자로 등급 파싱. (예: "A2" → GradeType.A)
    /// </summary>
    public static GradeType Parse(Sprite sprite)
    {
        if (sprite == null || string.IsNullOrEmpty(sprite.name))
            return GradeType.C;

        char first = char.ToUpper(sprite.name[0]);
        switch (first)
        {
            case 'A': return GradeType.A;
            case 'B': return GradeType.B;
            default:  return GradeType.C;
        }
    }

    /// <summary>
    /// 스��라이트 이름에서 번호 파싱. (예: "C12" → 12)
    /// </summary>
    public static int ParseNumber(Sprite sprite)
    {
        if (sprite == null || sprite.name.Length < 2)
            return 0;

        string name = sprite.name;
        // Unity 스프라이트 접미사 제거 (예: "C1_0" → "C1")
        int underscoreIndex = name.IndexOf('_');
        if (underscoreIndex > 0)
            name = name.Substring(0, underscoreIndex);

        // 숫자가 아닌 문자 건너뛰기 (예: "Bs1" → "1")
        int startIndex = 1;
        while (startIndex < name.Length && !char.IsDigit(name[startIndex]))
            startIndex++;

        string numStr = name.Substring(startIndex);
        int.TryParse(numStr, out int num);
        return num;
    }
}
