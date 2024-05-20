using UnityEngine;

public class WinZone : MonoBehaviour
{
    [SerializeField] private GameObject _particles;
    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Manikin"))
        {
            _particles.SetActive(true);
        }
    }
}
