using System;
using System.Collections.Generic;

class Game
{
    static void Main()
    {
        MainMenu();
    }

    static void MainMenu()
    {
        Console.Clear();
        Console.WriteLine("Welcome to the Enhanced Adventure Game!");
        Console.WriteLine("1. New Game");
        Console.WriteLine("2. Credits");
        Console.WriteLine("3. Exit");
        Console.Write("Choose an option: ");
        
        string choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                NewGame();
                break;
            case "2":
                Credits();
                break;
            case "3":
                Environment.Exit(0);
                break;
            default:
                Console.WriteLine("Invalid choice, please try again.");
                MainMenu();
                break;
        }
    }

    static void NewGame()
    {
        Console.Clear();
        Player player = new Player();
        Map map = new Map();
        bool gameOver = false;

        Console.WriteLine("New Game Started! Your adventure begins...\n");

        while (!gameOver)
        {
            map.ShowMap(player);

            Console.WriteLine("\nChoose a direction to move: (N = North, S = South, E = East, W = West)");
            Console.WriteLine("Press 'I' for Inventory, 'Q' to Quit the game.");

            var key = Console.ReadKey(true).Key;

            if (key == ConsoleKey.N)
                player.Move(0, -1); // Move North
            else if (key == ConsoleKey.S)
                player.Move(0, 1); // Move South
            else if (key == ConsoleKey.E)
                player.Move(1, 0); // Move East
            else if (key == ConsoleKey.W)
                player.Move(-1, 0); // Move West
            else if (key == ConsoleKey.I)
            {
                player.ShowInventory(); // Show Inventory
            }
            else if (key == ConsoleKey.Q)
            {
                Console.WriteLine("Exiting the game...");
                break; // Quit the game
            }

            gameOver = map.CheckGameEnding(player);
        }

        Console.WriteLine("Game Over! Returning to Main Menu...");
        Console.ReadLine();
        MainMenu();
    }

    static void Credits()
    {
        Console.Clear();
        Console.WriteLine("Credits:");
        Console.WriteLine("Game Design: Your Name");
        Console.WriteLine("Programming: Your Name");
        Console.WriteLine("Art: Source of Art");
        Console.WriteLine("Music: Source of Music");
        Console.WriteLine("\nPress Enter to return to the Main Menu.");
        Console.ReadLine();
        MainMenu();
    }
}

class Player
{
    public int X { get; set; }
    public int Y { get; set; }
    public List<string> Inventory { get; set; }

    public Player()
    {
        X = 0;
        Y = 0;
        Inventory = new List<string>();
    }

    public void Move(int dx, int dy)
    {
        X += dx;
        Y += dy;
    }

    public void AddToInventory(string item)
    {
        Inventory.Add(item);
    }

    public void ShowInventory()
    {
        Console.WriteLine("\nYour Inventory:");
        if (Inventory.Count == 0)
        {
            Console.WriteLine("Your inventory is empty.");
        }
        else
        {
            foreach (var item in Inventory)
            {
                Console.WriteLine("- " + item);
            }
        }
    }
}

class Map
{
    private Dictionary<string, Location> locations;

    public Map()
    {
        locations = new Dictionary<string, Location>
        {
            { "0,0", new Location("Starting Point", "You are at the starting point of your adventure.") },
            { "1,0", new Location("Mysterious Forest", "The forest is dark and full of sounds. You feel uneasy.") },
            { "0,1", new Location("Ancient Temple", "The temple stands tall with an old door. It might be locked.", true) },
            { "1,1", new Location("Hidden Cave", "A hidden cave entrance lies before you, cold air breezes from within.", true) }
        };
    }

    public void ShowMap(Player player)
    {
        string key = $"{player.X},{player.Y}";

        if (locations.ContainsKey(key))
        {
            Location currentLocation = locations[key];
            Console.WriteLine($"You are at: {currentLocation.Name}");
            Console.WriteLine(currentLocation.Description);

            if (currentLocation.Interactive)
            {
                Console.WriteLine("There's something interactive here. Press any key to interact...");
                Console.ReadKey();
                currentLocation.Interact(player);
            }
        }
        else
        {
            Console.WriteLine("You are in an empty area with no special location.");
        }

        player.ShowInventory();
    }

    public bool CheckGameEnding(Player player)
    {
        if (player.X == 1 && player.Y == 1) // Example of ending location
        {
            Console.WriteLine("You've reached the Hidden Cave and discovered the treasure! You win!");
            return true;
        }
        return false;
    }
}

class Location
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool Interactive { get; set; }

    public Location(string name, string description, bool interactive = false)
    {
        Name = name;
        Description = description;
        Interactive = interactive;
    }

    public void Interact(Player player)
    {
        if (Name == "Ancient Temple")
        {
            Console.WriteLine("You try to open the temple door... It's locked, but you find a key on the ground.");
            player.AddToInventory("Temple Key");
        }
        else if (Name == "Hidden Cave")
        {
            Console.WriteLine("You enter the cave and find a treasure chest! You have discovered the ancient treasure.");
            player.AddToInventory("Ancient Treasure");
        }
    }
}
