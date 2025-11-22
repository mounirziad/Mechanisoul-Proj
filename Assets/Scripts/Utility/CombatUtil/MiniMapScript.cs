using UnityEngine;

public class MiniMapScript : MonoBehaviour
{
    public Transform player;

    private void Start()
    {
        FindPlayerIfNeeded();
    }

    private void LateUpdate()
    {
        if (player == null)
        {
            FindPlayerIfNeeded();
        }

        if (player != null)
        {
            Vector3 newPosition = player.position;
            newPosition.y = transform.position.y;
            transform.position = newPosition;
        }
    }

    private void FindPlayerIfNeeded()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }
}
