using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public class HangmanGame
{
    // Configuration for game categories and words
    private static readonly Dictionary<Difficulty, Dictionary<string, List<string>>> WordCategories = new Dictionary<Difficulty, Dictionary<string, List<string>>>
    {
        { 
            Difficulty.Easy, 
            new Dictionary<string, List<string>> 
            {
                { "Animals", new List<string> { "cat", "dog", "fish", "bird", "mouse" } },
                { "Technology", new List<string> { "mouse", "keyboard", "screen", "phone", "tablet" } },
                { "Countries", new List<string> { "usa", "spain", "india", "egypt", "peru" } }
            }
        },
        { 
            Difficulty.Medium, 
            new Dictionary<string, List<string>> 
            {
                { "Animals", new List<string> { "elephant", "giraffe", "tiger", "penguin", "dolphin" } },
                { "Technology", new List<string> { "computer", "network", "system", "server", "database" } },
                { "Countries", new List<string> { "germany", "brazil", "canada", "japan", "mexico" } }
            }
        },
        { 
            Difficulty.Hard, 
            new Dictionary<string, List<string>> 
            {
                { "Animals", new List<string> { "hippopotamus", "chimpanzee", "orangutan", "rhinoceros", "komodo" } },
                { "Technology", new List<string> { "algorithm", "encryption", "cybersecurity", "blockchain", "microprocessor" } },
                { "Countries", new List<string> { "switzerland", "kazakhstan", "mozambique", "madagascar", "uzbekistan" } }
            }
        }
    };

    public enum Difficulty { Easy, Medium, Hard }

    public class Player
    {
        public string Name { get; set; }
        public int TotalScore { get; set; }
        public List<int> RoundScores { get; } = new List<int>();

        public void AddRoundScore(int score)
        {
            RoundScores.Add(score);
            TotalScore += score;
        }
    }

    public class HighScore
    {
        public string PlayerName { get; set; }
        public int Score { get; set; }
        public DateTime Date { get; set; }

        public override string ToString()
        {
            return $"{PlayerName},{Score},{Date:yyyy-MM-dd HH:mm:ss}";
        }

        public static HighScore FromString(string line)
        {
            try
            {
                var parts = line.Split(',');
                if (parts.Length != 3)
                {
                    return null;
                }
                
                return new HighScore
                {
                    PlayerName = parts[0],
                    Score = int.Parse(parts[1]),
                    Date = DateTime.Parse(parts[2])
                };
            }
            catch
            {
                return null;
            }
        }
    }

    public static void Main()
    {
        Console.WriteLine("Welcome to Advanced Hangman!");
        Player player = GetPlayerDetails();
        bool playAgain = true;

        while (playAgain)
        {
            PlayRound(player);

            Console.Write("Would you like to play again? (Y/N): ");
            playAgain = Console.ReadKey().KeyChar.ToString().ToUpper() == "Y";
            Console.WriteLine();
        }

        DisplayFinalResults(player);
    }

    private static Player GetPlayerDetails()
    {
        Console.Write("Enter your name: ");
        string name = Console.ReadLine();
        return new Player { Name = name };
    }

    private static void PlayRound(Player player)
    {
        Difficulty difficulty = ChooseDifficulty();
        string category = ChooseCategory();
        
        GameState gameState = InitializeGame(difficulty, category);
        PlayGame(gameState);

        UpdatePlayerScore(player, gameState.RoundScore);
        SaveHighScore(player, gameState.RoundScore);
    }

    private static Difficulty ChooseDifficulty()
    {
        while (true)
        {
            Console.Write("Choose difficulty (Easy/Medium/Hard): ");
            string input = Console.ReadLine().ToLower();
            
            if (Enum.TryParse(input, true, out Difficulty difficulty))
                return difficulty;
            
            Console.WriteLine("Invalid difficulty. Please choose Easy, Medium, or Hard.");
        }
    }

    private static string ChooseCategory()
    {
        var categories = WordCategories[Difficulty.Easy].Keys.ToList();
        
        while (true)
        {
            Console.WriteLine("Available Categories:");
            for (int i = 0; i < categories.Count; i++)
                Console.WriteLine($"{i + 1}. {categories[i]}");

            Console.Write("Choose a category number: ");
            if (int.TryParse(Console.ReadLine(), out int choice) && 
                choice > 0 && choice <= categories.Count)
            {
                return categories[choice - 1];
            }
            
            Console.WriteLine("Invalid category. Please choose a number from the list.");
        }
    }

    private class GameState
    {
        public string WordToGuess { get; set; }
        public char[] GuessedWord { get; set; }
        public HashSet<char> GuessedLetters { get; set; }
        public int AttemptsLeft { get; set; }
        public int RoundScore { get; set; }
        public bool HintGiven { get; set; }
    }

    private static GameState InitializeGame(Difficulty difficulty, string category)
    {
        string wordToGuess = GetRandomWord(difficulty, category);
        
        return new GameState
        {
            WordToGuess = wordToGuess,
            GuessedWord = new string('_', wordToGuess.Length).ToCharArray(),
            GuessedLetters = new HashSet<char>(),
            AttemptsLeft = GetAttemptsByDifficulty(difficulty),
            RoundScore = 0,
            HintGiven = false
        };
    }

    private static string GetRandomWord(Difficulty difficulty, string category)
    {
        var random = new Random();
        var words = WordCategories[difficulty][category];
        return words[random.Next(words.Count)].ToLower();
    }

    private static int GetAttemptsByDifficulty(Difficulty difficulty)
    {
        return difficulty switch
        {
            Difficulty.Easy => 8,
            Difficulty.Hard => 4,
            _ => 6
        };
    }

    private static void PlayGame(GameState gameState)
    {
        while (gameState.AttemptsLeft > 0 && gameState.GuessedWord.Contains('_'))
        {
            DisplayGameStatus(gameState);
            ProcessUserGuess(gameState);
        }

        DisplayGameResult(gameState);
    }

    private static void DisplayGameStatus(GameState gameState)
    {
        Console.Clear();
        DrawHangman(gameState.AttemptsLeft);
        
        Console.WriteLine("Word to guess: " + new string(gameState.GuessedWord));
        Console.WriteLine("Attempts remaining: " + gameState.AttemptsLeft);
        Console.WriteLine("Guessed letters: " + string.Join(", ", gameState.GuessedLetters));
        Console.WriteLine("Current score: " + gameState.RoundScore);

        // Smart hint system
        if (!gameState.HintGiven && gameState.AttemptsLeft <= 3)
        {
            Console.WriteLine($"Hint: The first letter is '{gameState.WordToGuess[0]}'");
            gameState.HintGiven = true;
        }
    }

    private static void ProcessUserGuess(GameState gameState)
    {
        Console.Write("Enter a letter: ");
        char guess = char.ToLower(Console.ReadKey().KeyChar);
        Console.WriteLine();

        if (!char.IsLetter(guess) || gameState.GuessedLetters.Contains(guess))
        {
            Console.WriteLine("Invalid input or letter already guessed.");
            System.Threading.Thread.Sleep(1000);
            return;
        }

        gameState.GuessedLetters.Add(guess);

        if (gameState.WordToGuess.Contains(guess))
        {
            UpdateCorrectGuess(gameState, guess);
        }
        else
        {
            UpdateWrongGuess(gameState);
        }

        System.Threading.Thread.Sleep(1000);
    }

    private static void UpdateCorrectGuess(GameState gameState, char guess)
    {
        Console.WriteLine("Good job! The letter is in the word.");
        for (int i = 0; i < gameState.WordToGuess.Length; i++)
        {
            if (gameState.WordToGuess[i] == guess)
            {
                gameState.GuessedWord[i] = guess;
                gameState.RoundScore += 10;
            }
        }
    }

    private static void UpdateWrongGuess(GameState gameState)
    {
        Console.WriteLine("Sorry, that letter is not in the word.");
        gameState.AttemptsLeft--;
    }

    private static void DisplayGameResult(GameState gameState)
    {
        Console.Clear();
        DrawHangman(gameState.AttemptsLeft);

        if (gameState.AttemptsLeft > 0)
        {
            Console.WriteLine("Congratulations! You guessed the word: " + gameState.WordToGuess);
            gameState.RoundScore += 50;
        }
        else
        {
            Console.WriteLine("Game over! The word was: " + gameState.WordToGuess);
        }
    }

    private static void UpdatePlayerScore(Player player, int roundScore)
    {
        player.AddRoundScore(roundScore);
        Console.WriteLine($"Your score for this round: {roundScore}");
        Console.WriteLine($"Total score: {player.TotalScore}");
    }

    private static void SaveHighScore(Player player, int roundScore)
    {
        string filePath = "highscores.txt";
        List<HighScore> highScores = new List<HighScore>();

        try
        {
            // Read existing high scores
            if (File.Exists(filePath))
            {
                foreach (string line in File.ReadAllLines(filePath))
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        var score = HighScore.FromString(line);
                        if (score != null)
                        {
                            highScores.Add(score);
                        }
                    }
                }
            }

            // Add new score
            highScores.Add(new HighScore
            {
                PlayerName = player.Name,
                Score = roundScore,
                Date = DateTime.Now
            });

            // Sort by score (descending) and keep only top 10
            highScores = highScores.OrderByDescending(s => s.Score).Take(10).ToList();

            // Save to file
            File.WriteAllLines(filePath, highScores.Select(s => s.ToString()));

            // Display high scores
            Console.WriteLine("\nTop 10 High Scores:");
            Console.WriteLine("Rank\tPlayer\t\tScore\tDate");
            Console.WriteLine("----------------------------------------");
            for (int i = 0; i < highScores.Count; i++)
            {
                var score = highScores[i];
                Console.WriteLine($"{i + 1}.\t{score.PlayerName,-12}\t{score.Score}\t{score.Date:yyyy-MM-dd}");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error saving high score: " + e.Message);
            // Create a new file if there was an error reading the existing one
            try
            {
                var newScore = new HighScore
                {
                    PlayerName = player.Name,
                    Score = roundScore,
                    Date = DateTime.Now
                };
                File.WriteAllText(filePath, newScore.ToString());
                Console.WriteLine("Created new high scores file with your score.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not create new high scores file: " + ex.Message);
            }
        }
    }

    private static void DisplayFinalResults(Player player)
    {
        Console.WriteLine($"\nFinal Game Results for {player.Name}:");
        Console.WriteLine($"Total Score: {player.TotalScore}");
        Console.WriteLine("Individual Round Scores: " + string.Join(", ", player.RoundScores));
    }

    private static void DrawHangman(int attemptsLeft)
    {
        string[] hangmanStages = new[]
        {
            "   _______\n   |     |\n   |     O\n   |    /|\\\n   |    / \\\n   |\n___|___\n",
            "   _______\n   |     |\n   |     O\n   |    /|\\\n   |    /\n   |\n___|___\n",
            "   _______\n   |     |\n   |     O\n   |    /|\\\n   |\n   |\n___|___\n",
            "   _______\n   |     |\n   |     O\n   |    /|\n   |\n   |\n___|___\n",
            "   _______\n   |     |\n   |     O\n   |     |\n   |\n   |\n___|___\n",
            "   _______\n   |     |\n   |     O\n   |\n   |\n   |\n___|___\n",
            "   _______\n   |     |\n   |\n   |\n   |\n   |\n___|___\n"
        };

        if (attemptsLeft >= 0 && attemptsLeft <= 6)
        {
            Console.WriteLine(hangmanStages[6 - attemptsLeft]);
        }
    }
}