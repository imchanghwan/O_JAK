using UnityEngine;

public class Breakable : MonoBehaviour, IInteractable
{
    public GameObject destroyEffect;

    public void Interact(GameObject interactor, Vector2 direction)
    {
        Debug.Log("BreakableWall: �ı���!");
        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}
