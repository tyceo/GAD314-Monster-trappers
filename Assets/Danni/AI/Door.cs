using UnityEngine;

public class Door : MonoBehaviour
{
    public Collider doorCollider;

    private void Awake()
    {
        if (!doorCollider) doorCollider = GetComponent<Collider>();
    }

    public void SetOpen(bool isOpen)
    {
        //if (doorCollider) doorCollider.enabled = !isOpen;
        gameObject.SetActive(!isOpen);
    }
}
