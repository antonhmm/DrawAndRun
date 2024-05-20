using Dreamteck.Splines;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawMove : MonoBehaviour
{
    [SerializeField] private RectTransform _drawingArea;
    [SerializeField] private GameObject _player, _playerArea, _drawAreaHolder, _endCanvas;
    [SerializeField] private List<Transform> _manikins = new();
    [SerializeField] private float _moveDuration = 1.0f;
    [SerializeField] private BoxCollider _playerDrawingArea;

    private List<Vector2> _points = new();
    private bool isDrawing = false;

    public bool endLevel = false;

    public List<Transform> Manikins { get => _manikins; }

    private void Start()
    {
        foreach (Transform child in _playerArea.transform)
        {
            _manikins.Add(child);
        }
    }

    private void Update()
    {
        if (_manikins.Count <= 0)
        {
            endLevel = true;
        }

        if (!endLevel)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(_drawingArea, Input.mousePosition))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    isDrawing = true;
                    _points.Clear();
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    isDrawing = false;
                    ArrangeObjects();
                }

                if (isDrawing)
                {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(_drawingArea, Input.mousePosition, null, out Vector2 localMousePosition);
                    _points.Add(localMousePosition);
                }
            }
        }
        else
        {
            StartCoroutine(EndLevel());
        }
    }

    private void LateUpdate()
    {
        _manikins.RemoveAll(item => item == null);
    }

    public void AddManikin(Transform manikin)
    {
        if (manikin == null) return;
        _manikins.RemoveAll(item => item == null);
        _manikins.Add(manikin);
        manikin.SetParent(_playerArea.transform);
    }

    public void DeleteManikin(Transform manikin)
    {
        _manikins.Remove(manikin);
        _manikins.RemoveAll(item => item == null);
    }

    private IEnumerator EndLevel()
    {
        yield return new WaitForSeconds(1f);

        _player.GetComponent<SplineFollower>().follow = false;
        ArrangeChessPattern();
        _drawAreaHolder.SetActive(false);

        _endCanvas.SetActive(true);
    }

    private void ArrangeObjects()
    {
        _manikins.RemoveAll(item => item == null);

        if (_points.Count < 2)
        {
            return;
        }

        Bounds bounds = _playerDrawingArea.bounds;
        Vector2 playerAreaSize = new(bounds.size.x, bounds.size.z);

        if (_manikins.Count == 1)
        {
            Vector2 centerPoint = GetCenterPoint(_points);
            Vector3 localPosition = new(
                centerPoint.x * playerAreaSize.x / _drawingArea.rect.width,
                _manikins[0].localPosition.y,
                centerPoint.y * playerAreaSize.y / _drawingArea.rect.height);
            StartCoroutine(MoveObject(_manikins[0], localPosition, _moveDuration));
        }
        else
        {
            float totalLength = GetTotalPathLength(_points);
            if (totalLength == 0)
            {
                return;
            }

            float segmentLength = totalLength / (_manikins.Count - 1);

            for (int i = 0; i < _manikins.Count; i++)
            {
                
                float distance = (i * segmentLength) + 1;
                Vector2 pointOnPath = GetPointAtDistance(_points, distance);
                Vector3 localPosition = new(
                    pointOnPath.x * playerAreaSize.x / _drawingArea.rect.width,
                    _manikins[i].localPosition.y,
                    pointOnPath.y * playerAreaSize.y / _drawingArea.rect.height);

                if (float.IsNaN(localPosition.x) || float.IsNaN(localPosition.z))
                {
                    Debug.LogWarning($"NaN detected in localPosition calculation for object {i}. Skipping.");
                    continue;
                }

                StartCoroutine(MoveObject(_manikins[i], localPosition, _moveDuration));
            }
        }
    }

    private IEnumerator MoveObject(Transform obj, Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = obj.localPosition;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            obj.localPosition = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        obj.localPosition = targetPosition;
    }

    private float GetTotalPathLength(List<Vector2> path)
    {
        float length = 0;
        for (int i = 0; i < path.Count - 1; i++)
        {
            length += Vector2.Distance(path[i], path[i + 1]);
        }
        return length;
    }

    private Vector2 GetPointAtDistance(List<Vector2> path, float distance)
    {
        float coveredDistance = 0;

        for (int i = 0; i < path.Count - 1; i++)
        {
            float segmentLength = Vector2.Distance(path[i], path[i + 1]);
            if (coveredDistance + segmentLength >= distance)
            {
                float remainingDistance = distance - coveredDistance;
                float t = remainingDistance / segmentLength;
                return Vector2.Lerp(path[i], path[i + 1], t);
            }
            coveredDistance += segmentLength;
        }

        return path[path.Count - 1];
    }

    private Vector2 GetCenterPoint(List<Vector2> path)
    {
        Vector2 sum = Vector2.zero;
        foreach (Vector2 point in path)
        {
            sum += point;
        }
        return sum / path.Count;
    }

    private void ArrangeChessPattern()
    {
        Bounds bounds = _playerDrawingArea.bounds;
        Vector2 playerAreaSize = new Vector2(bounds.size.x, bounds.size.z);

        int rowCount = Mathf.CeilToInt(Mathf.Sqrt(_manikins.Count));
        int colCount = Mathf.CeilToInt((float)_manikins.Count / rowCount);

        float xStep = playerAreaSize.x / colCount;
        float zStep = playerAreaSize.y / rowCount;

        int index = 0;
        for (int row = 0; row < rowCount; row++)
        {
            for (int col = 0; col < colCount; col++)
            {
                if (index >= _manikins.Count) return;

                float xPos = col * xStep + (row % 2 == 0 ? 0 : xStep / 2);
                float zPos = row * zStep;

                Vector3 localPosition = new Vector3(
                    xPos - playerAreaSize.x / 2,
                    _manikins[index].localPosition.y,
                    zPos - playerAreaSize.y / 2);

                StartCoroutine(MoveObject(_manikins[index], localPosition, _moveDuration));
                index++;
            }
        }
    }
}
