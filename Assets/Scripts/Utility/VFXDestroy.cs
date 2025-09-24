using UnityEngine;
using System.Collections;

public class VFXDestroy : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(DestroyVFX());
    }

    private IEnumerator DestroyVFX()
    {
        yield return new WaitForSeconds(3f);
        Destroy(this.gameObject);
    }
}
