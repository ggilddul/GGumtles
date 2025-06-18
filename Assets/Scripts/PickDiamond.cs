using UnityEngine;

public class PickDiamond : MonoBehaviour
{
    public void Pick()
    {
        GameManager.Instance?.PickDiamond();

        Destroy(gameObject);
    }
}