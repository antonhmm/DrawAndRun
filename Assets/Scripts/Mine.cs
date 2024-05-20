using UnityEngine;

public class Mine : MonoBehaviour
{
    [SerializeField] private GameObject _particle1, _particle2, _particle3, _particle4;
    private void OnTriggerEnter(Collider other)
    {
        GetComponent<MeshRenderer>().enabled = false;
        _particle1.SetActive(true);
        _particle2.SetActive(true);
        _particle3.SetActive(true);
        _particle4.SetActive(true);
        Destroy(this, 1f);
    }
}
