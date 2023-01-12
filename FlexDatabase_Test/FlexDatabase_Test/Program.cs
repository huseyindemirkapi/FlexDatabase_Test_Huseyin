using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Diagnostics;
using NLog;

namespace FlexDatabase_Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            Word();

        }

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static void Word()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            long startMemory = GC.GetTotalMemory(false);

            try
            {
                WebClient client = new WebClient();
                client.DownloadFile("https://www.gutenberg.org/files/4300/4300-0.txt", "book.txt");

                // Read the file into a string
                string bookText = File.ReadAllText("book.txt");

                // Split the text into words
                string[] words = bookText.Split(new char[] { ' ', '\r', '\n', '.', ',', '_', '—', '?', '!' }, StringSplitOptions.RemoveEmptyEntries);

                // Count the occurrences of each word
                Dictionary<string, int> wordCounts = new Dictionary<string, int>();
                foreach (string word in words)
                {
                    if (wordCounts.ContainsKey(word))
                    {
                        wordCounts[word]++;
                    }
                    else
                    {
                        wordCounts[word] = 1;
                    }
                }

                //Get unique words
                var uniqeWordCount = wordCounts.Count();
                Console.WriteLine("Number of Unique Words: " + uniqeWordCount);

                // Get the top 20 words
                var topWords = wordCounts.OrderByDescending(pair => pair.Value).Take(20);
                Console.WriteLine("Top 20 Words and Counts:");
                foreach (var word in topWords)
                {
                    Console.WriteLine("{0} - {1}", word.Key, word.Value);
                }

                //Get the top 5 of the words standing next to these top 20 words.
                foreach (var word in topWords)
                {

                    Console.WriteLine("Top 5 words next to '{0}'", word.Key);
                    //count the top 5 words next to the top 20 words
                    var topNextWords = FindNextWords(words, word.Key, 5);
                    foreach (var nextWord in topNextWords)
                    {
                        Console.WriteLine("{0} - {1}", nextWord.Key, nextWord.Value);
                    }

                }
            }
            catch (WebException ex)
            {
                logger.Error("An error occurred while downloading the file: " + ex.Message);
                Console.WriteLine("An error occurred while downloading the file: ", ex.Message);
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while processing the text: " + ex.Message);
                Console.WriteLine("An error occurred while processing the file: ", ex.Message);
            }
            finally
            {
                stopwatch.Stop();
                long endMemory = GC.GetTotalMemory(false);
                Console.WriteLine("Time taken: {0} ms", stopwatch.ElapsedMilliseconds);
                Console.WriteLine("Memory used: {0} bytes", endMemory - startMemory);
            }
            
        }
        private static Dictionary<string, int> FindNextWords(string[] words, string word, int count)
        {
            var nextWords = new Dictionary<string, int>();
            for (int i = 0; i < words.Length - 1; i++)
            {
                if (words[i].Equals(word, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (nextWords.ContainsKey(words[i + 1]))
                    {
                        nextWords[words[i + 1]]++;
                    }
                    else
                    {
                        nextWords[words[i + 1]] = 1;
                    }
                }
            }
            return nextWords.OrderByDescending(pair => pair.Value).Take(count).ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}