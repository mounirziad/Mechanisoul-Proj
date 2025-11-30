using UnityEngine;

public class RoomCollider : MonoBehaviour
{
    public string roomName;

    private LevelMap levelMap;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (levelMap == null)
            {
                levelMap = FindObjectOfType<LevelMap>();
            }

            if (levelMap != null)
            {
                levelMap.EnterRoom(roomName);
            }
            else
            {
                Debug.LogWarning($"RoomCollider '{roomName}': LevelMap not found. Make sure GameUI prefab is loaded.");
            }
        }
    }
}