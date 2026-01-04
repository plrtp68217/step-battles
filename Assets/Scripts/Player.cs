using Mirror;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] float speed = 5f;

    void Update()
    {
        if (!isOwned) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 movement = speed * Time.deltaTime * new Vector3(h, 0, v);
        transform.Translate(movement);
    }
}