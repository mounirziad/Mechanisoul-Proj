using UnityEngine;
using UnityEngine.UI;

public class Room
{
    public string Name { get; }
    public string Description { get; set; }
    public bool Cleared;
    public bool Empty;

    GameObject roomObject;
    Image roomImage;

    public Sprite hiddenSprite;
    public Sprite inRoomSprite;
    public Sprite clearedSprite;
    public Sprite currentSprite;

    public Room(string name, string description = "")
    {
        Name = name;
        Description = description;
        Cleared = false;
    }

    public Room(bool empty)
    {
        Empty = empty;
        Name = "";
    }



    public void CreateGameObject()
    {
        if (Empty) { return; }
        roomObject = new GameObject(Name, typeof(Image));
        roomImage = roomObject.GetComponent<Image>();
    }

    public void SetParent(GameObject parent)
    {
        if (Empty) { return; }
        roomObject.transform.SetParent(parent.transform);
        roomObject.transform.position = parent.transform.position;
    }

    public void Translate(float x, float y)
    {
        if (Empty) { return; }
        roomObject.transform.Translate(new Vector3(x * 100, y * 100, 0));
        roomObject.transform.localScale = Vector3.one;
    }

    public void SetSprites(Sprite hiddenSprite, Sprite inRoomSprite, Sprite clearedSprite)
    {
        if (Empty) { return; }
        this.hiddenSprite = hiddenSprite;
        this.inRoomSprite = inRoomSprite;
        this.clearedSprite = clearedSprite;
        currentSprite = hiddenSprite;
        UpdateSprite();
    }

    public override string ToString() => Name;

    public void ClearRoom() => Cleared = true;

    public void UpdateSprite() => roomImage.sprite = currentSprite;
}

public class LevelMap : MonoBehaviour
{
    public Sprite hiddenSprite;
    public Sprite inRoomSprite;
    public Sprite clearedSprite;

    //temporary room names, replace with actual rooms later
    private static readonly Room[,] Rooms =
            {
                { new Room(true), new Room(true), new Room(true) },
                {new Room("Room 1"), new Room("Room 2"), new Room("Room 3") },
                {new Room(true), new Room(true), new Room(true) }
            };

    Room currentRoom;
    GameObject mapObject;

    private void Start()
    {
        mapObject = gameObject;

        for (int i = 0; i < Rooms.GetLength(0); i++)
        {
            for (int j = 0; j < Rooms.GetLength(1); j++)
            {
                Rooms[i, j].CreateGameObject();
                Rooms[i, j].SetParent(mapObject);
                Rooms[i, j].Translate(i, j);
                Rooms[i, j].SetSprites(hiddenSprite, inRoomSprite, clearedSprite);
            }
        }
    }

    public void EnterRoom(string name)
    {
        if (currentRoom != null)
        {
            currentRoom.currentSprite = currentRoom.clearedSprite;
            currentRoom.UpdateSprite();
        }

        foreach (Room room in Rooms)
        {
            if (room.Name.Equals(name))
            {
                currentRoom = room;
                room.currentSprite = room.inRoomSprite;
                room.UpdateSprite();
                break;
            }
        }
    }
}
