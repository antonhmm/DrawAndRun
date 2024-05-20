using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float _speed, _lifeTime = 3f;
    private float _currentLifeTime;
    private Vector3 _startPosition;
    private void Start()
    {
        _startPosition = transform.position;
        _currentLifeTime = _lifeTime;
    }

    private void OnEnable()
    {
        _currentLifeTime = _lifeTime;
    }

    private void Update()
    {
        if (_currentLifeTime > 0)
        {
            _currentLifeTime -= Time.deltaTime;
        }
        else
        {
            transform.position = _startPosition;
            _currentLifeTime = _lifeTime;
            gameObject.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        transform.localPosition += _speed * -1 * Time.fixedDeltaTime * Vector3.forward;
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Manikin"))
        {
            transform.position = _startPosition;
            enabled = false;
        }
    }
}
