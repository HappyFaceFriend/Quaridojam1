
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour, ISelectable
{
    [SerializeField] Material _selectableMaterial;

    MeshRenderer _meshRenderer;
    Material _originalMaterial;

    bool _isSelectable = false;

    Vector2Int _coordinates;

    public Vector2Int Coordinates => _coordinates;
    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _originalMaterial = _meshRenderer.sharedMaterial;
    }
    public void SetCoordinates(Vector2Int coordinates)
    {
        _coordinates = coordinates;
    }

    public void SetSelectable(bool selectable)
    {
        if (selectable)
        {
            _meshRenderer.sharedMaterial = _selectableMaterial;
        }
        else
        {
            _meshRenderer.sharedMaterial = _originalMaterial;
        }
        _isSelectable = selectable;
    }
    private void OnMouseDown()
    {
        if (_isSelectable)
        {
            PlayerInput.Instance.SelectTile(this);
        }
    }
}