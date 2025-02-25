﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        var game = new Game();
        game.Start();
    }
}

class Game
{
    private GameState gameState;
    private Dictionary<(int, int), Location> gameMap;
    private bool isRunning = true;
    private const string SAVE_FILE = "savegame.json";
    private readonly Random random = new Random();

    public Game()
    {
        InitializeMap();
    }

    private void InitializeMap()
    {
        gameMap = new Dictionary<(int, int), Location>();

        // Initialize locations with more varied interactions and items
        gameMap.Add((0, 0), new Location(
            "Village Square",
            "You find yourself in a bustling village square. A mysterious old well sits in the center. Villagers hurry about their daily routines.",
            new List<InteractiveElement> {
                new InteractiveElement("Well", "You peer into the well. There seems to be something glinting at the bottom...",
                    onInteract: (state) => {
                        if (!state.HasItem("Rope")) {
                            Console.WriteLine("The well is too deep to reach the bottom. Maybe you need something to help you get down there?");
                            return false;
                        }
                        if (!state.HasItem("Key")) {
                            Console.WriteLine("Using the rope, you climb down and find a rusty key!");
                            state.AddItem("Key");
                        }
                        return true;
                    }),
                new InteractiveElement("Villagers", "A group of villagers chat nearby. They mention something about an old rope in the forest...")
            }
        ));

        gameMap.Add((1, 0), new Location(
            "Abandoned House",
            "A creaky old house stands before you. The door appears to be locked. Weathered windows peer out like ancient eyes.",
            new List<InteractiveElement> {
                new InteractiveElement("Door", "The door is locked. Maybe there's a key somewhere?",
                    requiresItem: "Key",
                    onInteract: (state) => {
                        if (state.HasItem("Key")) {
                            Console.WriteLine("The key fits! The door creaks open to reveal...");
                            return true;
                        }
                        return false;
                    }),
                new InteractiveElement("Windows", "The windows are too dirty to see through, but you might be able to clean them.",
                    requiresItem: "Cloth",
                    onInteract: (state) => {
                        if (state.HasItem("Cloth")) {
                            Console.WriteLine("You clean the window and peek inside. You see strange symbols written on the walls...");
                            state.AddNote("Strange symbols seen inside the house");
                            return true;
                        }
                        return false;
                    })
            }
        ));

        gameMap.Add((0, 1), new Location(
            "Forest Clearing",
            "A peaceful clearing in the forest. An old man sits on a log. Sunlight filters through the canopy.",
            new List<InteractiveElement> {
                new InteractiveElement("Old Man", "The old man looks at you thoughtfully. 'I lost my key near the well... But you'll need a rope to reach it.'",
                    onInteract: (state) => {
                        if (!state.HasItem("Rope") && random.Next(2) == 0) {
                            Console.WriteLine("The old man gives you a piece of rope!");
                            state.AddItem("Rope");
                        }
                        return true;
                    }),
                new InteractiveElement("Fallen Tree", "A fallen tree covered in soft moss. Something might be hidden underneath.",
                    onInteract: (state) => {
                        if (!state.HasItem("Cloth")) {
                            Console.WriteLine("You find an old piece of cloth!");
                            state.AddItem("Cloth");
                        }
                        return true;
                    })
            }
        ));
    }

    public void Start()
    {
        Console.Title = "The Lost Key - Text Adventure";
        while (isRunning)
        {
            ShowMainMenu();
        }
    }

    private void ShowMainMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("╔═══════════════════════╗");
            Console.WriteLine("║     The Lost Key      ║");
            Console.WriteLine("╚═══════════════════════╝");
            Console.WriteLine("\n1. New Game");
            Console.WriteLine("2. Load Game");
            Console.WriteLine("3. Credits");
            Console.WriteLine("4. Exit");
            Console.Write("\nSelect an option (1-4): ");

            if (int.TryParse(Console.ReadLine(), out int choice))
            {
                switch (choice)
                {
                    case 1: StartNewGame(); return;
                    case 2: LoadGame(); return;
                    case 3: ShowCredits(); continue;
                    case 4: isRunning = false; return;
                    default: ShowInvalidInput(); continue;
                }
            }
            ShowInvalidInput();
        }
    }

    private void StartNewGame()
    {
        gameState = new GameState();
        Console.Clear();
        Console.WriteLine("You arrive in a mysterious village, searching for adventure...");
        Console.WriteLine("Press any key to begin your journey...");
        Console.ReadKey();
        GameLoop();
    }

    private void GameLoop()
    {
        bool gameRunning = true;
        while (gameRunning && isRunning)
        {
            Console.Clear();
            Location currentLocation = gameMap[(gameState.PlayerX, gameState.PlayerY)];

            // Display current status
            DisplayStatus(currentLocation);

            // Show available actions
            Console.WriteLine("\nActions:");
            Console.WriteLine("1. Move");
            Console.WriteLine("2. Interact");
            Console.WriteLine("3. Check Inventory");
            Console.WriteLine("4. View Notes");
            Console.WriteLine("5. Save Game");
            Console.WriteLine("6. Return to Main Menu");
            Console.Write("\nSelect an action (1-6): ");

            if (int.TryParse(Console.ReadLine(), out int choice))
            {
                switch (choice)
                {
                    case 1: Move(); break;
                    case 2: Interact(currentLocation); break;
                    case 3: ShowInventory(); break;
                    case 4: ShowNotes(); break;
                    case 5: SaveGame(); break;
                    case 6: gameRunning = false; break;
                    default: ShowInvalidInput(); break;
                }
            }
            else
            {
                ShowInvalidInput();
            }

            // Check win condition
            if (gameState.HasItem("Key") && gameMap[(1, 0)].Interactive.Any(x => x.Name == "Door" && x.HasInteracted))
            {
                ShowEnding();
                gameRunning = false;
            }
        }
    }

    private void DisplayStatus(Location currentLocation)
    {
        Console.WriteLine($"\nLocation: {currentLocation.Name}");
        Console.WriteLine(new string('═', currentLocation.Name.Length + 10));
        Console.WriteLine(currentLocation.Description);

        if (currentLocation.Interactive.Any())
        {
            Console.WriteLine("\nYou can interact with:");
            foreach (var interactive in currentLocation.Interactive)
            {
                Console.WriteLine($"- {interactive.Name}");
            }
        }
    }

    private void Move()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Where would you like to move?");
            Console.WriteLine("1. North");
            Console.WriteLine("2. South");
            Console.WriteLine("3. East");
            Console.WriteLine("4. West");
            Console.WriteLine("5. Cancel");
            Console.Write("\nSelect direction (1-5): ");

            if (int.TryParse(Console.ReadLine(), out int choice))
            {
                int newX = gameState.PlayerX;
                int newY = gameState.PlayerY;

                switch (choice)
                {
                    case 1: newY--; break;
                    case 2: newY++; break;
                    case 3: newX++; break;
                    case 4: newX--; break;
                    case 5: return;
                    default: ShowInvalidInput(); continue;
                }

                if (gameMap.ContainsKey((newX, newY)))
                {
                    gameState.PlayerX = newX;
                    gameState.PlayerY = newY;
                    return;
                }
                else
                {
                    Console.WriteLine("\nYou cannot move in that direction.");
                    Console.WriteLine("Press any key to try again...");
                    Console.ReadKey();
                }
            }
            else
            {
                ShowInvalidInput();
            }
        }
    }

    private void Interact(Location location)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("What would you like to interact with?");
            for (int i = 0; i < location.Interactive.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {location.Interactive[i].Name}");
            }
            Console.WriteLine($"{location.Interactive.Count + 1}. Cancel");
            Console.Write($"\nSelect option (1-{location.Interactive.Count + 1}): ");

            if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= location.Interactive.Count + 1)
            {
                if (choice == location.Interactive.Count + 1)
                    return;

                var interactive = location.Interactive[choice - 1];
                if (interactive.RequiresItem != null && !gameState.HasItem(interactive.RequiresItem))
                {
                    Console.WriteLine($"\nYou need a {interactive.RequiresItem} to interact with this.");
                }
                else
                {
                    Console.WriteLine($"\n{interactive.Description}");
                    if (interactive.OnInteract != null)
                    {
                        interactive.HasInteracted = interactive.OnInteract(gameState);
                    }
                    else
                    {
                        interactive.HasInteracted = true;
                    }
                }
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                return;
            }
            ShowInvalidInput();
        }
    }

    private void ShowInventory()
    {
        Console.Clear();
        Console.WriteLine("╔═══════════════╗");
        Console.WriteLine("║   Inventory   ║");
        Console.WriteLine("╚═══════════════╝");

        if (gameState.Inventory.Any())
        {
            foreach (string item in gameState.Inventory)
            {
                Console.WriteLine($"- {item}");
            }
        }
        else
        {
            Console.WriteLine("Your inventory is empty.");
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }

    private void ShowNotes()
    {
        Console.Clear();
        Console.WriteLine("╔═══════════════╗");
        Console.WriteLine("║    Notes      ║");
        Console.WriteLine("╚═══════════════╝");

        if (gameState.Notes.Any())
        {
            foreach (string note in gameState.Notes)
            {
                Console.WriteLine($"- {note}");
            }
        }
        else
        {
            Console.WriteLine("You haven't discovered any notes yet.");
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }

    private void SaveGame()
    {
        try
        {
            string json = JsonSerializer.Serialize(gameState);
            File.WriteAllText(SAVE_FILE, json);
            Console.WriteLine("\nGame saved successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError saving game: {ex.Message}");
        }
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    private void LoadGame()
    {
        try
        {
            if (!File.Exists(SAVE_FILE))
            {
                Console.WriteLine("\nNo save file found.");
                Console.WriteLine("Press any key to return to main menu...");
                Console.ReadKey();
                return;
            }

            string json = File.ReadAllText(SAVE_FILE);
            gameState = JsonSerializer.Deserialize<GameState>(json);
            GameLoop();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError loading game: {ex.Message}");
            Console.WriteLine("Press any key to return to main menu...");
            Console.ReadKey();
        }
    }

    private void ShowEnding()
    {
        Console.Clear();
        Console.WriteLine("╔════════════════════════════════════════╗");
        Console.WriteLine("║           Congratulations!             ║");
        Console.WriteLine("║                                        ║");
        Console.WriteLine("║ You've unlocked the door and completed ║");
        Console.WriteLine("║           your adventure!              ║");
        Console.WriteLine("╚════════════════════════════════════════╝");
        Console.WriteLine("\nPress any key to return to the main menu...");
        Console.ReadKey();
    }

    private void ShowCredits()
    {
        Console.Clear();
        Console.WriteLine("╔═══════════════════════╗");
        Console.WriteLine("║       Credits         ║");
        Console.WriteLine("╚═══════════════════════╝");
        Console.WriteLine("\nGame designed and developed by:");
        Console.WriteLine("- Ali");
        Console.WriteLine("\nPress any key to return to the main menu...");
        Console.ReadKey();
    }

    private void ShowInvalidInput()
    {
        Console.WriteLine("\nInvalid input. Press any key to try again...");
        Console.ReadKey();
    }
}

class Location
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<InteractiveElement> Interactive { get; set; }

    public Location(string name, string description, List<InteractiveElement> interactive = null)
    {
        Name = name;
        Description = description;
        Interactive = interactive ?? new List<InteractiveElement>();
    }
}

class InteractiveElement
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string RequiresItem { get; set; }
    public bool HasInteracted { get; set; }
    public Func<GameState, bool> OnInteract { get; set; }

    public InteractiveElement(string name, string description, string requiresItem = null, Func<GameState, bool> onInteract = null)
    {
        Name = name;
        Description = description;
        RequiresItem = requiresItem;
        HasInteracted = false;
        OnInteract = onInteract;
    }
}

class GameState
{
    public int PlayerX { get; set; }
    public int PlayerY { get; set; }
    public List<string> Inventory { get; set; } = new List<string>();
    public List<string> Notes { get; set; } = new List<string>();

    public bool HasItem(string item)
    {
        return Inventory.Contains(item);
    }

    public void AddItem(string item)
    {
        if (!Inventory.Contains(item))
        {
            Inventory.Add(item);
        }
    }

    public void AddNote(string note)
    {
        if (!Notes.Contains(note))
        {
            Notes.Add(note);
        }
    }
}
