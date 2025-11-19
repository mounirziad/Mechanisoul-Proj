using UnityEngine;

public class Room
{
    public string Name { get; }
    public string Description { get; set; }
    public bool Cleared;

    GameObject roomObject;
    SpriteRenderer roomRenderer;

    public Sprite hiddenSprite;
    public Sprite inRoomSprite;
    public Sprite clearedSprite;
    public Sprite currentSprite;

    public Room(string name, string description = "")
    {
        Name = name;
        Description = description;
        Cleared = false;
        currentSprite = hiddenSprite;
    }

    public void CreateGameObject()
    {
        roomObject = new GameObject(Name, typeof(SpriteRenderer));
        roomRenderer = roomObject.GetComponent<SpriteRenderer>();
        roomRenderer.sprite = currentSprite;
    }

    public override string ToString() => Name;

    public void ClearRoom() => Cleared = true;
}

public class LevelMap : MonoBehaviour
{

    //temporary room names, replace with actual rooms later
    private static readonly Room[,] Rooms =
            {
                { new Room("Rocky Trail"), new Room("South of House"), new Room("Canyon View") },
                {new Room("Forest"), new Room("West of House"), new Room("Behind House") },
                {new Room("Dense Woods"), new Room("North of House"), new Room("Clearing") }
            };

    Room currentRoom;

    private void Start()
    {
        foreach (var room in Rooms)
        {
            room.CreateGameObject();
        }
    }

    public void EnterRoom(string name)
    {
        currentRoom.currentSprite = currentRoom.clearedSprite;

        foreach (Room room in Rooms)
        {
            if (room.Name == name)
            {
                currentRoom = room;
                room.currentSprite = room.inRoomSprite;
                break;
            }
        }
    }

    void UpdateUI()
    {
        //change ui to reflect new status

        //rooms cleared

        //current room
    }
}
