using System.Collections;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    [SerializeField] private GameObject _bullet, _particles;
    private bool _fire = false;
    private float _fireRateTimer = 3f;

    private void Update()
    {
        if (!_fire)
        {
            StartCoroutine(Fire());
        }
    }

    private IEnumerator Fire()
    {
        _fire = true;
        _bullet.SetActive(true);
        StartCoroutine(Explosion());
        yield return new WaitForSeconds(_fireRateTimer);
        _fire = false;
    }

    private IEnumerator Explosion()
    {
        _particles.SetActive(true);
        yield return new WaitForSeconds(1f);
        _particles.SetActive(false);
    }
}
