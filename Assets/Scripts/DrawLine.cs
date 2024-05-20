using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class DrawLine : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private float _lineThickness = 0.1f, _fadeDuration = 1.0f;
    [SerializeField] private Color _lineColor = Color.black;

    [SerializeField] private Image _backgroundImage;

    private Image _drawingImage;
    private RectTransform _drawingRectTransform;
    private Texture2D _texture;
    private Vector2? _previousPoint;

    private void Start()
    {
        _drawingImage = GetComponent<Image>();
        _drawingRectTransform = GetComponent<RectTransform>();

        _texture = new Texture2D((int)_drawingRectTransform.rect.width, (int)_drawingRectTransform.rect.height);
        ClearTexture();
    }

    private void ClearTexture()
    {
        Color[] colors = new Color[_texture.width * _texture.height];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.clear;
        }
        _texture.SetPixels(colors);
        _texture.Apply();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _previousPoint = null;
        StopAllCoroutines();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _previousPoint = null;
        StartCoroutine(FadeOutAndClear());
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_drawingRectTransform, eventData.position, eventData.pressEventCamera, out Vector2 currentPoint);

        if (_previousPoint.HasValue)
        {
            DrawingLine(_previousPoint.Value, currentPoint);
        }

        _previousPoint = currentPoint;
    }

    private void DrawingLine(Vector2 start, Vector2 end)
    {
        Vector2Int startPixel = WorldToPixelCoordinates(start);
        Vector2Int endPixel = WorldToPixelCoordinates(end);

        DrawingHelper.DrawLine(_texture, _texture.width, _texture.height, startPixel.x, startPixel.y, endPixel.x, endPixel.y, _lineThickness, _lineColor);

        _texture.Apply();
        _drawingImage.sprite = Sprite.Create(_texture, new Rect(0, 0, _texture.width, _texture.height), Vector2.one * 0.5f);
    }

    private Vector2Int WorldToPixelCoordinates(Vector2 worldCoordinates)
    {
        Vector2 localPoint = worldCoordinates - (Vector2)_drawingRectTransform.rect.position;
        Vector2 pixelUV = new (localPoint.x / _drawingRectTransform.rect.width, localPoint.y / _drawingRectTransform.rect.height);
        return new Vector2Int(Mathf.RoundToInt(pixelUV.x * _texture.width), Mathf.RoundToInt(pixelUV.y * _texture.height));
    }

    private IEnumerator FadeOutAndClear()
    {
        float elapsedTime = 0f;
        Color[] originalColors = _texture.GetPixels();

        while (elapsedTime < _fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, elapsedTime / _fadeDuration);

            for (int i = 0; i < originalColors.Length; i++)
            {
                originalColors[i].a *= alpha;
            }

            _texture.SetPixels(originalColors);
            _texture.Apply();

            _drawingImage.sprite = Sprite.Create(_texture, new Rect(0, 0, _texture.width, _texture.height), Vector2.one * 0.5f);
            yield return null;
        }

        ClearTexture();
        _drawingImage.sprite = Sprite.Create(_texture, new Rect(0, 0, _texture.width, _texture.height), Vector2.one * 0.5f);
    }
}

public static class DrawingHelper
{
    public static void DrawLine(Texture2D texture, int textureWidth, int textureHeight, int x0, int y0, int x1, int y1, float thickness, Color color)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        float thicknessSq = thickness * thickness;

        while (true)
        {
            for (int i = -Mathf.RoundToInt(thickness / 2); i <= Mathf.RoundToInt(thickness / 2); i++)
            {
                for (int j = -Mathf.RoundToInt(thickness / 2); j <= Mathf.RoundToInt(thickness / 2); j++)
                {
                    if (x0 + i >= 0 && x0 + i < textureWidth && y0 + j >= 0 && y0 + j < textureHeight && (i * i + j * j) <= thicknessSq)
                    {
                        texture.SetPixel(x0 + i, y0 + j, color);
                    }
                }
            }

            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }
}
