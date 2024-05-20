using System.Collections;
using UnityEngine;

public class Manikins : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private GameObject  _body, _particle;
    [SerializeField] private bool _isPlayer = false;
    private DrawMove _drawMoveScript;
    private string _enemyTag = "Enemy", _winTag = "Win";


    void Start()
    {
        if (_drawMoveScript == null)
        {
            _drawMoveScript = FindObjectOfType<DrawMove>();
        }
        if (_isPlayer) 
        {
            _animator.SetTrigger("Run");
        }
    }

    public void SetPlayer()
    {
        _isPlayer = true;
        _animator.SetTrigger("Run");
        transform.rotation = Quaternion.identity;
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag(gameObject.tag))
        {
            Transform otherTransform = col.transform;

            if (_drawMoveScript != null && !_drawMoveScript.Manikins.Contains(otherTransform))
            {
                _drawMoveScript.AddManikin(otherTransform);
                otherTransform.GetComponent<Manikins>().SetPlayer();
            }
        }
        else if (col.gameObject.CompareTag(_enemyTag))
        {
            if (_drawMoveScript != null)
            {
                _drawMoveScript.DeleteManikin(transform);
            }

            StartCoroutine(Dead());
        }
        else if (col.gameObject.CompareTag(_winTag))
        {
            _animator.SetBool("Win", true);
            _drawMoveScript.endLevel = true;
        }
    }
        
    private IEnumerator Dead()
    {
        _animator.SetTrigger("Dead");
        yield return new WaitForSeconds(0.4f);
        _body.SetActive(false);
        _particle.SetActive(true);
        Destroy(gameObject, 0.5f);
    }
}
