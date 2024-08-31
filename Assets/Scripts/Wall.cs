
using UnityEngine;

public class Wall : MonoBehaviour
{
    public bool Installed { get; set; } = false;
    public bool IsHorizontal => Mathf.RoundToInt(transform.eulerAngles.y / 90f) % 2 == 0;

    Vector3 _originalPosition;
    private void Awake()
    {
        _originalPosition = transform.position;
    }
    public void Rotate()
    {
        transform.Rotate(Vector3.up, 90f);
    }
    public void ReturnToOriginalPosition()
    {
        transform.position = _originalPosition;
    }
    private void OnMouseDown()
    {
        if (!Installed)
            PlayerInput.Instance.TrySelectWall(this);
    }
}