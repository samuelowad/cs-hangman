using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class HangmanGame
{
    static void Main()
    {
        Console.WriteLine("Welcome to Hangman!");

        bool playAgain = true;
        int totalScore = 0;

        while (playAgain)
        {
            // Select difficulty
            Console.Write("Choose difficulty (Easy, Medium, Hard): ");
            string difficulty = Console.ReadLine().ToLower();
            int attemptsLeft;
            switch (difficulty)
            {
                case "easy":
                    attemptsLeft = 8;
                    break;
                case "hard":
                    attemptsLeft = 4;
                    break;
                default:
                    attemptsLeft = 6;
                    break;
            }

            // Select category
            Console.Write("Choose a category (Animals, Technology, Countries): ");
            string category = Console.ReadLine().ToLower();
            string wordToGuess = GetRandomWord(category);

            if (string.IsNullOrEmpty(wordToGuess))
            {
                Console.WriteLine("Invalid category. Using a random word.");
                wordToGuess = GetRandomWord("random");
            }

            char[] guessedWord = new string('_', wordToGuess.Length).ToCharArray();
            HashSet<char> guessedLetters = new HashSet<char>();
            bool hintGiven = false;
            int roundScore = 0;

            while (attemptsLeft > 0 && guessedWord.Contains('_'))
            {
                Console.Clear();
                DrawHangman(attemptsLeft);
                Console.WriteLine("Word to guess: " + new string(guessedWord));
                Console.WriteLine("Attempts remaining: " + attemptsLeft);
                Console.WriteLine("Guessed letters: " + string.Join(", ", guessedLetters));
                Console.WriteLine("Score: " + roundScore);

                // Give a hint if necessary
                if (!hintGiven && attemptsLeft <= 3)
                {
                    Console.WriteLine($"Hint: The first letter is '{wordToGuess[0]}'");
                    hintGiven = true;
                }

                Console.Write("Enter a letter: ");
                char guess = Console.ReadKey().KeyChar;
                Console.WriteLine();

                // Validate input
                if (!char.IsLetter(guess) || guessedLetters.Contains(guess))
                {
                    Console.WriteLine("Invalid input or letter already guessed. Try again.");
                    System.Threading.Thread.Sleep(1000);
                    continue;
                }

                guessedLetters.Add(guess);

                if (wordToGuess.Contains(guess))
                {
                    Console.WriteLine("Good job! The letter is in the word.");
                    for (int i = 0; i < wordToGuess.Length; i++)
                    {
                        if (wordToGuess[i] == guess)
                        {
                            guessedWord[i] = guess;
                            roundScore += 10;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Sorry, that letter is not in the word.");
                    attemptsLeft--;
                }

                System.Threading.Thread.Sleep(1000);
            }

            Console.Clear();
            DrawHangman(attemptsLeft);
            if (attemptsLeft > 0)
            {
                Console.WriteLine("Congratulations! You guessed the word: " + wordToGuess);
                roundScore += 50;
            }
            else
            {
                Console.WriteLine("Game over! The word was: " + wordToGuess);
            }

            totalScore += roundScore;
            Console.WriteLine("Your score for this round: " + roundScore);
            Console.WriteLine("Total score: " + totalScore);

            SaveScore("Player", totalScore);

            Console.Write("Would you like to play again? (Y/N): ");
            playAgain = Console.ReadKey().KeyChar.ToString().ToUpper() == "Y";
            Console.WriteLine();
        }

        Console.WriteLine("Thank you for playing! Your final score: " + totalScore);
    }

    // Method to draw the hangman based on remaining attempts
    static void DrawHangman(int attemptsLeft)
    {
        string[] hangmanStages = new string[]
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
        else
        {
            Console.WriteLine("Error: Invalid number of attempts left.");
        }
    }


    // Method to get a random word from a specific category
    static string GetRandomWord(string category)
    {
        Dictionary<string, List<string>> wordCategories = new Dictionary<string, List<string>>()
        {
            { "animals", new List<string> { "elephant", "giraffe", "alligator", "zebra", "tiger" } },
            { "technology", new List<string> { "computer", "algorithm", "network", "keyboard", "encryption" } },
            { "countries", new List<string> { "germany", "canada", "brazil", "japan", "sweden" } },
            { "random", new List<string> { "mystery", "adventure", "puzzle", "guitar", "hangman" } }
        };

        if (wordCategories.ContainsKey(category))
        {
            Random random = new Random();
            List<string> words = wordCategories[category];
            return words[random.Next(words.Count)];
        }
        
        return null;
    }

    // Method to save the score to a file
    static void SaveScore(string playerName, int score)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter("highscores.txt", true))
            {
                writer.WriteLine($"{DateTime.Now}: {playerName} - Score: {score}");
            }
            Console.WriteLine("Score saved to highscores.txt");
        }
        catch (Exception e)
        {
            Console.WriteLine("Error saving score: " + e.Message);
        }
    }
}
