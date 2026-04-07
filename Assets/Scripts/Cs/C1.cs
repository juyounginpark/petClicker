using UnityEngine;
using System.Collections;

/// <summary>
/// C1: 2초마다 5 데미지
/// </summary>
public class C1 : ItemEffect
{
    private float interval = 5f;
    private float damage = 5f;
    private Coroutine autoDamageCoroutine;

    public override void Activate()
    {
        autoDamageCoroutine = StartCoroutine(AutoDamage());
    }

    public override void Deactivate()
    {
        if (autoDamageCoroutine != null)
        {
            StopCoroutine(autoDamageCoroutine);
            autoDamageCoroutine = null;
        }
    }

    private IEnumerator AutoDamage()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            if (click != null)
                click.ApplyDamage(damage);
        }
    }
}
