using UnityEngine;

[RequireComponent (typeof(SpriteRenderer))]
public class PlayerShadow : MonoBehaviour
{
    Transform playerTransform;
    PlayerLocomotion playerLocomotion;
    SpriteRenderer spriteRenderer;

    private void Awake()
    {
        playerLocomotion = transform.root.gameObject.GetComponent<PlayerLocomotion>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerTransform = transform.parent;
    }

    private void Update()
    {
        if (playerLocomotion.isGrounded)
        {
            transform.position = playerTransform.position;
            spriteRenderer.enabled = false;
        }
        else
        {
            spriteRenderer.enabled = true;
            if (Physics.Raycast(playerTransform.position, Vector3.down,
                            out RaycastHit hit, 20f))
            {
                transform.position = hit.point + Vector3.up * 0.02f;
            }


            Debug.DrawRay(playerTransform.position, Vector3.down, Color.aliceBlue, 0.5f);

        }
    }
}
