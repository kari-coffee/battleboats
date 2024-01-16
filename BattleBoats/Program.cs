using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace BattleBoats 
{
    internal class Program
    {
        class Boat
        {
            // object boat contains list of its pieces (coordinates where part of the boat is), so its easy to check if a boat is destroyed / destroy a piece of one
            public List<(int, int)> Pieces { get; set; }
            public Boat()
            {
                Pieces = new List<(int, int)>();
            }
            public void AddPiece(int y, int x)
            {
                Pieces.Add((y, x));
            }
        }
        // Height and width of board (both fleet & shot tracker)
        const int HEIGHT = 8;
        const int WIDTH = 8;
        // initially define boats as 5 destroyers, only change if user chooses to (i.e. if user chooses option 1 (5 destroyers), no need to change)
        public static string[] Boats = { "B", "B", "B", "B", "B" };
        static char[] letters = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H' }; // conversion from coordinate index to letter (for grid y coord)

        static void DisplayInstructions()
        {
            string instructions = "Instructions:\nBattle Boats is a game where you try to sink your opponent's\n(in this case the computer's)" +
                " ship fleet before they sink yours.\nYou start by placing your boats, and then take turns to choose\nlocations to shoot at, " +
                "until either you or your opponent's entire fleet has been sunk\nEither start new game:\n - Choose configuration options" +
                "\n - Place boats (input x, y and orientation if needed), boats\nplace from start coord and" +
                " extend either right or down depending on orientation (boats are displayed as 'B' on the grid)" +
                "\nOr load game from file.\n\nWhile playing game:\n - Input coordinate " +
                "to shoot at and your opponent will also\nchoose a location\n - Hits and misses will be indicated with (H) and (M)\n";
            Console.WriteLine(instructions);
        }
        static string InputKey(string[] keys, bool show)
        {
            // helper function to input a single key
            bool valid_input = false;
            string input = "";
            while (!valid_input)
            {
                input = Console.ReadKey(show).Key.ToString();
                foreach (string key in keys)
                {
                    if (input == key)
                    {
                        valid_input = true;
                        break;
                    }
                }
            }
            return input;
        }
        static (int, int) InputCoords()
        {
            // helper function to input a set of coordinates in form (x as number, y as letter) and return them in form (row as index, col as index)
            bool valid_input;
            int[] coords = { 0, 0 }; // (x, y)
            Dictionary<int, char[]> digits = new Dictionary<int, char[]>()
            {
                { 0, new char[] { '1', '2', '3', '4', '5', '6', '7', '8' } },
                { 1, new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H'} },
            };
            string[] output = { "x (digits 1-8 inclusive)", "y (letters A-H inclusive)" };
            for (int i = 0; i < 2; i++)
            {
                valid_input = false;
                string input = "";
                while (!valid_input)
                {
                    Console.Write($"{output[i]}: ");
                    input = Console.ReadLine().ToUpper();
                    if (input == "-1") // entering -1 is save game option
                    {
                        return (-1, -1);
                    }
                    else if (input.Length != 1)
                    {
                        continue;
                    }
                    if (digits[i].Contains(input[0]))
                    {
                        valid_input = true;
                        coords[i] = Array.IndexOf(digits[i], input[0]);
                        break;
                    }
                }
                Console.WriteLine();
            }
            return (coords[1], coords[0]); // returns (y, x) / (row, col)
        }
        static void SaveFile(char[,] Fleet1, char[,] Fleet2, char[,] Shots1, char[,] Shots2, List<Boat>Boats1, List<Boat>Boats2)
        {
            // procedure contents of grids into save file
            Console.Write("Enter filename as .txt: ");
            string filename = Console.ReadLine();
            using (StreamWriter writer = new StreamWriter(filename))
            {
                string line;
                foreach (char[,] Board in new char[][,] { Fleet1, Fleet2, Shots1, Shots2 })
                {
                    for (int row = 0; row < HEIGHT; row++)
                    {
                        line = "";
                        for (int col = 0; col < WIDTH; col++)
                        {
                            line += Board[row, col];
                        }
                        writer.WriteLine(line);
                    }
                }
                string[] coords;
                int count;
                foreach (List<Boat> Boats in new List<Boat>[] { Boats1, Boats2 })
                {
                    foreach (Boat boat in Boats)
                    {
                        line = "";
                        foreach ((int y, int x) in boat.Pieces)
                        {
                            line += y.ToString() + "," + x.ToString() + ",";
                        }
                        if (line == "")
                        {
                            writer.WriteLine("");
                        }
                        else
                        {
                            writer.WriteLine(line.Substring(0, line.Length - 1));
                        }
                    }
                }
            }
        }
        static void ReadFile(char[,] Fleet1, char[,] Fleet2, char[,] Shots1, char[,] Shots2, List<Boat>Boats1, List<Boat> Boats2)
        {
            // procedure to read a save file and put the contents into the grids
            Console.Write("Enter filename as .txt: ");
            bool valid_file = false;
            string filename = "";
            while (!valid_file)
            {
                filename = Console.ReadLine();
                if (File.Exists(filename))
                {
                    //check for file validiity
                    valid_file = true;
                    break;
                }
                Console.WriteLine("File does not exist.");
                Console.Write("Enter filename: ");
            }
            if (valid_file)
            {
                // File is formatted as follows:
                // 4 sets of 8x8 grids representing player fleet, player shot tracker, computer fleet, computer shot tracker
                // 2 sets of 5 lines representing player boat list, and computer boat list
                // a single boat in the boat list will be in the format of comma separated coords - x,y,x2,y2...
                using (StreamReader reader = new StreamReader(filename))
                {
                    string line;
                    foreach (char[,] Board in new char[][,] {Fleet1, Fleet2, Shots1, Shots2}) {
                        for (int row = 0; row < HEIGHT; row++)
                        {
                            line = reader.ReadLine();
                            for (int col = 0; col < WIDTH; col++)
                            {
                                Board[row, col] = line[col];
                            }
                        }
                    }
                    string[] coords;
                    int x, y;
                    List<Boat>[] Boats = new List<Boat>[] { Boats1, Boats2 };
                    for (int j = 0; j < 2; j++)
                    {
                        for (int row = 0; row < 5; row++)
                        {
                            Boats[j].Add(new Boat());
                            coords = reader.ReadLine().Split(',');
                            if (coords.Length == 0)
                            {
                                continue;
                            }
                            for (int i = 0; i < coords.Length - 1; i += 2)
                            {
                                y = int.Parse(coords[i]);
                                x = int.Parse(coords[i + 1]);
                                Boats[j][Boats[j].Count - 1].AddPiece(y, x);
                            }
                        }
                    }
                    // testing code
                    //foreach (List<Boat>Boatlist in new List<Boat>[] {Boats1, Boats2})
                    //{
                    //    foreach (Boat boat in Boatlist)
                    //    {
                    //        Console.WriteLine(typeof(Boat));
                    //        Console.WriteLine(typeof(Boat).GetProperties());
                    //        foreach ((int, int) piece in boat.Pieces)
                    //        {
                    //            Console.WriteLine(piece);
                    //        }
                    //    }
                    //}
                }
            }
        }
        static void DisplayBoard(char[,] board, string type)
        {
            // helper procedure to display a board with its marking letter in the top left corner
            Console.WriteLine();
            Console.WriteLine($"{type} 1 2 3 4 5 6 7 8");
            
            string line;
            for (int row = 0; row < HEIGHT; row++)
            {
                line = $"{letters[row]}";
                for (int col = 0; col < WIDTH; col++)
                {
                    line += " " + board[row, col];
                }
                Console.WriteLine(line);
            }
        }
       
        static (int, int) CheckPlacement(char[,] Fleet, int x, int y, string orientation, int length)
        {
            // function to check if a placement is valid or not (checks all the coords a ship would be placed in, given the x, y & orientation)
            int dx, dy;
            if (orientation == "H")
            {
                dx = 1;
                dy = 0;
            }
            else // orientation == "V"
            {
                dx = 0;
                dy = 1;
            }
            for (int j = 0; j < length; j++)
            {
                if (x + dx * j < WIDTH && y + dy * j < HEIGHT)
                {
                    if (Fleet[y + dy * j, x + dx * j] != 'w') // not empty
                    {
                        return (-1, -1);
                    }
                }
                else // out of bounds
                {
                    return (-1, -1);
                }
            }
            return (dy, dx);
        }
        static void InputBoard(char[,] Fleet, List<Boat> BoatList)
        {
            // helper procedure to take player input and put it into fleet board
            Console.Write("\nChoose boat configuration:\nOption [1]:\n5 single cell boats\n\nOption [2]:" +
                "\n 2 x Destroyers (1 cell)\n2 x Submarines (2 cells)\n 1 x Carrier (3 cells)\nChoice: ");
            string input = InputKey(new string[] { "D1", "D2" }, true);
            Console.WriteLine(input[1]);
            if (input == "D2") // change boat configuration
            {
                Boats = new string[] { "B", "B", "BB", "BB", "BBB" };
            }
            Console.WriteLine($"\nBoats available: [{String.Join(' ', Boats)}]");
            int x, y;
            int i = 0;
            string orientation = ""; //not char as inputkey returns string
            bool valid;
            while (i < Boats.Length)
            {
                valid = false;
                int dx, dy;
                string boat = Boats[i];
                while (!valid)
                {
                    Console.WriteLine($"Enter coordinates of boat {i + 1} ({boat}):");
                    (y, x) = InputCoords();
                    if (boat.Length > 1)
                    {
                        Console.WriteLine("Enter orientation (H or V): ");
                        orientation = InputKey(new string[] { "H", "V" }, true);
                        Console.WriteLine($"{orientation}\n");
                    }

                    // check if coords are valid and returns (dy, dx)
                    (dy, dx) = CheckPlacement(Fleet, x, y, orientation, boat.Length);

                    if (dx != -1) // only need to check dx, although both dx & dy would be -1
                    {
                        // placement is valid, add pieces to boat and put boat onto fleet board
                        BoatList.Add(new Boat());
                        for (int j = 0; j < boat.Length; j++)
                        {
                            Fleet[y + dy * j, x + dx * j] = 'B';
                            BoatList[BoatList.Count-1].AddPiece(y + dy * j, x + dx * j);
                        }
                        Console.Write($"Boat placed:");
                        valid = true; //valid is useless but no while true loops will be located in this code.
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Invalid placement.");
                    }
                }
                DisplayBoard(Fleet, "F");
                i++;
            }
        }
        static void InitialiseCompBoard(char[,] Fleet, List<Boat> BoatList)
        {
            // procedure to initialise computer's fleet board by randomly generating coordinates for placement
            string boat;
            int x, y, dx, dy;
            Random random = new Random();
            string[] D = { "H", "V" };
            string orientation;
            int i = 0;
            while (i < Boats.Length)
            {
                boat = Boats[i];
                x = random.Next(0, 8);
                y = random.Next(0, 8);
                orientation = D[random.Next(0, 2)];
                (dy, dx) = CheckPlacement(Fleet, x, y, orientation, boat.Length);
                if (dx != -1)
                {
                    BoatList.Add(new Boat());
                    for (int j = 0; j < boat.Length; j++)
                    {
                        Fleet[y + dy * j, x + dx * j] = 'B';
                        BoatList[BoatList.Count-1].AddPiece(y + dy * j, x + dx * j);
                    }
                    i++; //only increment number of boats placed if valid - if invalid, #boats placed stays the same, so loop keeps running
                }
            }
            Console.Write("\nComputer board (display for testing only)");
            DisplayBoard(Fleet, "C");
        }
        static void InitialiseBoards(char[,] Fleet1, char[,] Fleet2, char[,] Shots1, char[,] Shots2, List<Boat> Boats1, List<Boat> Boats2)
        {
            // function which calls InputBoard & InitialiseCompBoard to initialise both player & computer boards, or reads them from a file if user selects to load game
            Console.WriteLine("Load game from file? (Y/N)");
            string input = InputKey(new string[] { "Y", "N" }, true);
            if (input == "Y") // then read Board from file
            {
                ReadFile(Fleet1, Fleet2, Shots1, Shots2, Boats1, Boats2);
            }
            else if (input == "N")
            {
                Console.Write("\nEmpty fleet board: ");
                DisplayBoard(Fleet1, " ");
                InputBoard(Fleet1, Boats1);
                InitialiseCompBoard(Fleet2, Boats2);
                Console.WriteLine("Computer thinking...");
                Thread.Sleep(700);
                Console.WriteLine("Computer's boats placed!");
                Thread.Sleep(500);
            }
        }
        
        static void Quit()
        {
            Console.WriteLine("Quitting...press any key to close console.");
            Console.ReadLine();
            Environment.Exit(1);
        }
        static void Game()
        {
            // grids for both player and computer so program can be easily converted into player vs player if needed
            char[,] Fleet1 = new char[HEIGHT, WIDTH];
            char[,] Fleet2 = new char[HEIGHT, WIDTH];
            char[,] Shots1 = new char[HEIGHT, WIDTH];
            char[,] Shots2 = new char[HEIGHT, WIDTH];
            List<Boat> Boats1 = new();
            List<Boat> Boats2 = new();
            // initialise boards with the placeholder characters
            for (int row = 0; row < HEIGHT; row++)
            {
                for (int col = 0; col < WIDTH; col++)
                {
                    Fleet1[row, col] = 'w';
                    Fleet2[row, col] = 'w';
                    Shots1[row, col] = '-';
                    Shots2[row, col] = '-';
                }
            }
            InitialiseBoards(Fleet1, Fleet2, Shots1, Shots2, Boats1, Boats2);
            Console.WriteLine("------------------------\n      Game Start!\n------------------------");
            // Take turns until there is a winner:
            bool player_turn = true;
            bool hit, sunk, validshot;
            int[] BoatsToSink = { Boats1.Count, Boats2.Count }; //counters for how many boats needed to be sunk for someone to win
            // if either count hits 0, someone has won and we stop the game
            List<Boat>[] Boats = new List<Boat>[] { Boats1, Boats2 };
            for (int i = 0; i < 2; i++) 
            {
                foreach (Boat boat in Boats[i])
                {
                    if (boat.Pieces.Count == 0)
                    {
                        BoatsToSink[i]--;
                    }
                }
            }
            int x = -1;
            int y = -1;
            int shot_num = 1;
            while (!BoatsToSink.Contains(0))
            {
                Thread.Sleep(500);
                hit = false;
                sunk = false;
                validshot = false;
                if (player_turn)
                {
                    Console.Write("------------------------\n   [ Your Turn ]");
                    Console.Write("\nShot tracker board: ");
                    DisplayBoard(Shots1, "T");
                    while (!validshot) // keep asking for coords until player has not shot there before
                    {
                        Console.WriteLine($"Enter coords of shot {shot_num} (enter -1 to save current game and exit): ");
                        (y, x) = InputCoords();
                        if (y == -1)
                        {
                            SaveFile(Fleet1, Fleet2, Shots1, Shots2, Boats1, Boats2);
                            Console.WriteLine("Saving");
                            for (int k = 0; k < 3; k++)
                            {
                                Thread.Sleep(300);
                                Console.WriteLine(".");
                            }
                            Console.WriteLine("Game saved.");
                            Quit();
                        }
                        else if (Shots1[y, x] == '-')
                        {
                            validshot = true;
                            shot_num++;
                            break;
                        }
                        Console.WriteLine("You've already shot there.");
                    }
                    foreach (Boat boat in Boats2) // checking if the coordinates contains enemy boat piece
                    {
                        if (boat.Pieces.Contains((y, x)))
                        {
                            hit = true;
                            foreach ((int, int) piece in boat.Pieces)
                            {
                                if (piece == (y, x))
                                {
                                    boat.Pieces.Remove((y, x));
                                    if (boat.Pieces.Count == 0)
                                    {
                                        sunk = true;
                                        BoatsToSink[1] -= 1;
                                    }
                                    break;
                                }
                            }
                            break;
                        }
                    }
                    if (hit)
                    {
                        Shots1[y, x] = 'H';
                        Console.WriteLine("Enemy boat hit!");
                        if (sunk)
                        {
                            Console.WriteLine("Enemy boat sunk!");
                        }
                    }
                    else
                    {
                        Shots1[y, x] = 'M';
                        Console.WriteLine("You missed..");
                    }
                    player_turn = false;
                }
                else
                {
                    Console.WriteLine("------------------------\n   [ Computer Turn ]");
                    while (!validshot) // keep generating coords until computer has not shot there before
                    {
                        Random random = new Random();
                        x = random.Next(0, 8);
                        y = random.Next(0, 8);
                        if (Shots2[y, x] == '-')
                        {   
                            validshot = true;
                            Console.WriteLine($"Computer shot at {x+1}{letters[y]}");
                            break;
                        }
                    }
                    foreach (Boat boat in Boats1) // checking if the coordinates contains player boat piece
                    {
                        if (boat.Pieces.Contains((y, x)))
                        {
                            hit = true;
                            foreach ((int, int) piece in boat.Pieces)
                            {
                                if (piece == (y, x))
                                {
                                    boat.Pieces.Remove((y, x));
                                    Fleet1[y, x] = 'H';
                                    if (boat.Pieces.Count == 0)
                                    {
                                        sunk = true;
                                        BoatsToSink[0] -= 1;
                                    }
                                    break;
                                }
                            }
                            break;
                        }
                    }
                    if (hit)
                    {
                        Shots2[y, x] = 'H';
                        Console.WriteLine("Your boat was hit!");
                        if (sunk)
                        {
                            Console.WriteLine("Your boat was sunk!");
                        }
                    }
                    else
                    {
                        Shots2[y, x] = 'M';
                        Console.WriteLine("Computer missed..");
                    }
                    // testing output
                    Console.Write("Computer shot tracker board (testing): ");
                    DisplayBoard(Shots2, "T");
                    //
                    Console.Write("Your fleet board: ");
                    DisplayBoard(Fleet1, "F");
                    player_turn = true;
                }
            }
            Console.WriteLine("------------------------\n      Game Over!\n------------------------");
            Thread.Sleep(500);
            if (BoatsToSink[0] == 0) // player loses
            {
                Console.WriteLine("All your boats have been sunk!\nYou Lose!");
            }
            else // player wins
            {
                Console.WriteLine("All enemy boats have been sunk!\nA winner has become you!");
            }
            Thread.Sleep(500);
            // display all boards to give a summary of the game
            Console.Write("\nYour board: ");
            DisplayBoard(Fleet1, "F");
            Console.Write("Your shot tracker: ");
            DisplayBoard(Shots1, "T");
            Thread.Sleep(500);
            Console.Write("\nComputer board: ");
            DisplayBoard(Fleet2, "F");
            Console.Write("Computer shot tracker: ");
            DisplayBoard(Shots2, "T");
            Console.WriteLine();
            Thread.Sleep(500);
        }
        static void Menu()
        {
            Console.WriteLine("---BATTLE BOATS---\nMenu:\n1 - Play\n2 - Instructions\n3 - Quit");
            string input = InputKey(new string[] { "D1", "D2", "D3" }, true);
            switch (input) {
                case "D1":
                    Game();
                    break;
                case "D2":
                    DisplayInstructions();
                    break;
                case "D3":
                    Quit();
                    break;
            }
            Menu();
        }
        static void Main(string[] args)
        {
            Menu();
        }
    }
}