
using UnityEngine;

public class Pawn : MonoBehaviour
{
    private void OnMouseDown()
    {
        PlayerInput.Instance.TrySelectPawn(this);
    }
}