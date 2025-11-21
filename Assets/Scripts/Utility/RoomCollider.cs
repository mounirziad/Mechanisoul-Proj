using UnityEngine;

public class RoomCollider : MonoBehaviour
{
    public string roomName;

    LevelMap levelMap;

    private void Start()
    {
        levelMap = GameObject.Find("Map").GetComponent<LevelMap>();
    }


    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log($"Room Collider Tag: {other.gameObject.tag}");
        if (other.gameObject.CompareTag("Player")) 
            levelMap.EnterRoom(roomName);
    }
}