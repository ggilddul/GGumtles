using UnityEngine;

public class PickAcorn : MonoBehaviour
{
    public void Pick()
    {
        GameManager.Instance?.PickAcorn();

        Destroy(gameObject);
    }
}
