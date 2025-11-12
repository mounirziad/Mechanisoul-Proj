using UnityEngine;

public class Room
{
    public string Name { get; }
    public string Description { get; set; }
    public bool cleared;

    public Room(string name, string description = "")
    {
        Name = name;
        Description = description;
        cleared = false;
    }

    public override string ToString() => Name;

    public void ClearRoom() => cleared = true;
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

    string currentRoom;
    
    public void EnterRoom(string name) => currentRoom = name;

    void UpdateUI()
    {
        //change ui to reflect new status

        //rooms cleared

        //current room
    }
}
