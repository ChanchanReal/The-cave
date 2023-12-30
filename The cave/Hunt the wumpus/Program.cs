Game game = SetupGame();
game.Run();

Game SetupGame()
{
    Console.Write("Select room size: small, medium, large > ");

    string? choice = Console.ReadLine();
    (int, int) mapSize = choice.ToLower() switch {
        "small"     => (4, 4),
        "medium"    => (8, 8),
        "large"     => (12, 12),
        _           => (4, 4)
    };

    return new Game(mapSize);
}

// manage game state.

public class Game
{
    private Random random = new Random();
    public int Row { get;set; }
    public int Column { get; set; }
    public bool HaveKey { get; set; } = false;

    public Room[,] room;

    public Action input;

    public Player player;

    public List<ISense> sense;
    public bool isDead { get; set; } = false;
    public Game((int,int)mapSize )
    {
        Row = mapSize.Item1;

        Column = mapSize.Item2;

        input = new Action();

        player = new Player();

        // setting up rooms.
        room = new Room[Row, Column ];

        for(int i = 0; i < Row; i++)
            for(int j = 0; j < Column; j++)
            {
                room[i, j] = new NormalRoom();
            }
        // room placement
        room[0,0] = new EntranceRoom();
        room[Row - 1, Column - 1] = new TreasureRoom();
        room[random.Next(0, Row), random.Next(0, Column)] = new Monster();

        sense = new List<ISense>
        {
            new EntranceSense(),
            new TreasureSense(),
            new MonsterSense(),
        };
    }
    public void Run()
    {
        while (!isDead)
        {

            Display();
            if (HaveKey) {
                sense = (from s in sense
                         where !(s is TreasureSense)
                         select s)
                         .ToList();
            }
            Input();

            if (HaveKey == true && room[player.playerLocation.Item1, player.playerLocation.Item2] is EntranceRoom)
                break;

            
        }
        if (isDead != true)
            Console.WriteLine("You survive the labyrinth cave. Congrants !!");
        else
            Console.WriteLine("You died you lose.. unlucky");
    }
    public void Input()
    {
        Console.Write("What do you want to do? ");
        string? choice = Console.ReadLine();
        if(choice == "exit" || choice == "quit") Environment.Exit(0);
        Command action = input.MoveOrGrab(choice);

        switch (action)
        {
            case Command.Grab:
                if (room[player.playerLocation.Item1, player.playerLocation.Item2] is TreasureRoom)
                    HaveKey = true;
                break;

            case Command.North:
                if (player.playerLocation.Item2 > 0) {
                    player = action + player; }
                break;

            case Command.South:
                if (player.playerLocation.Item2 < Column - 1) { 
                    player = action + player; }
                break;

            case Command.West:
            if (player.playerLocation.Item1 > 0) {
                player = action + player; }
                break;

            case Command.East:
            if (player.playerLocation.Item1 < Row - 1) { 
                player = action + player; }
                break;
        }

    }

    public void Display()
    {
        Console.WriteLine($"You are at (X = {player.playerLocation.Item1}, Y = {player.playerLocation.Item2})");

        foreach (ISense s in sense)
            s.Sense(this);
    }
}
public class Action
{
    public string Actions { get; set; }

    public Command MoveOrGrab(string input)
    {
        return input switch
        {
            "north" => Command.North,
            "south" => Command.South,
            "east"  => Command.East,
            "west"  => Command.West,
            "grab"  => Command.Grab,
            _       => Command.Grab,
        };
    }
    
}
// tracks player location 
public class Player
{
    public (int, int) playerLocation { get; set; }

    public Player() { } 
    public Player(int row, int column)
    {
        playerLocation = (row, column);
    }
    public static Player operator +(Command a, Player p)
    {
        return a switch
        {
            Command.North => new Player(p.playerLocation.Item1, p.playerLocation.Item2 - 1),
            Command.South => new Player(p.playerLocation.Item1, p.playerLocation.Item2 + 1),
            Command.East => new Player(p.playerLocation.Item1 + 1, p.playerLocation.Item2),
            Command.West => new Player(p.playerLocation.Item1 - 1, p.playerLocation.Item2),
        };
    }
}
// represents room and handles postion.
public class Room {  }
public class NormalRoom : Room { }
public class TreasureRoom: Room { }
public class EntranceRoom: Room { }
// monster position, different types maybe?
public  class Monster : Room { }

// sense current place at the room.
public interface ISense 
{
    void Sense(Game game);
}

public class EntranceSense : ISense { 
    public void Sense(Game game) {
        (int r, int c) loc = game.player.playerLocation;
        if (game.room[loc.r, loc.c] is EntranceRoom)
        {
            if (game.HaveKey == true)
            {
                Console.WriteLine("Congrats you opened the labyrinth exit.");
            }
            else { Console.WriteLine("You are at the entrance."); }
        }
    } 
}
public class TreasureSense : ISense { 
    public void Sense(Game game) {
        (int r, int c) loc = game.player.playerLocation;
        if (game.room[loc.r,loc.c] is TreasureRoom)
        {
            if (game.HaveKey == false) { 
                Console.WriteLine("You see shiny thing at the ground.");
                 }
            else 
                Console.WriteLine("You looted a Labyrinth key congrats.");
        }
    } 
}
public class MonsterSense : ISense {
    public void Sense(Game game) {
        (int r, int c) loc = game.player.playerLocation;
        if (game.room[loc.r, loc.c] is Monster)
        {
            game.isDead = true;
            Console.WriteLine("You entered Shadowspire room you got killed.");
        }
    }
}

// translate this to change position at rooms.
public enum Command { North, South, East, West , Grab}