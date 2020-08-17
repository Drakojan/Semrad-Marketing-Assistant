using System;
using System.Collections.Generic;
using System.Linq;

namespace Semrad_marketing_assistant
{
    class StartUp
    {
        static void Main(string[] args)
        {

            ReadAndFormatData(out List<string> emailList, out List<string> SemradEntriesList);

            int counterOfMatches = ProcessDataForMatches(emailList, SemradEntriesList);

            PrintResults(emailList, SemradEntriesList, counterOfMatches);
        }

        private static void ReadAndFormatData(out List<string> emailList, out List<string> SemradEntriesList)
        {

            Console.WriteLine("Please input the list from the e-mail and type end after you're done. Format is FirstName LastName");
            var currentEmail = Console.ReadLine();

            emailList = new List<string>();
            while (currentEmail != "end")
            {
                emailList.Add(currentEmail);
                currentEmail = Console.ReadLine();
            }

            Console.WriteLine("Please input the list from the Semrad Portal and type end after you're done. Format is LastName, FirstName MiddleInitial");
            var currentSemradPortalEntry = Console.ReadLine();

            SemradEntriesList = new List<string>();
            while (currentSemradPortalEntry != "end")
            {
                string formatedSemradPortalEntry = FormatSemradPortalEntriesToFirstNameLastName(currentSemradPortalEntry);

                SemradEntriesList.Add(formatedSemradPortalEntry);

                currentSemradPortalEntry = Console.ReadLine();
            }
        }
        private static string FormatSemradPortalEntriesToFirstNameLastName(string currentSemradPortalEntry)
        {
            var currentEntryArr = currentSemradPortalEntry.Split(", ").ToArray();
            var reversedEntry = currentEntryArr.Reverse().ToList(); //format is currently [FirstName MiddleInitial] [LastName]

            if (reversedEntry[0].Contains(' '))
            {
                var firstNameWithoutInitial = reversedEntry[0].TakeWhile(x => x != ' ');
                var firstNameNoInitialToAdd = string.Join("", firstNameWithoutInitial);

                reversedEntry.RemoveAt(0);
                reversedEntry.Insert(0, firstNameNoInitialToAdd);
            } // this if removes any middle initials and changes format to FirstName LastName

            string readyToAdd = string.Join(" ", reversedEntry).Trim(); // trim is becase sometimes they write name , name
            return readyToAdd;
        }   
        private static int ProcessDataForMatches(List<string> emailList, List<string> SemradEntriesList)
        {
            int counterOfMatches = 0;

            for (int i = 0; i < emailList.Count; i++)
            {
                for (int a = 0; a < SemradEntriesList.Count; a++)
                { //checking for exact matches first and only processing remains through Levenshtein should improve performance, but this is rarely used for more than 300 entries.

                    if (string.IsNullOrWhiteSpace(SemradEntriesList[a]))
                    {
                        SemradEntriesList.Remove(SemradEntriesList[a]);
                        break;
                    }

                    int distance = ComputeLevenshteinDistance(SemradEntriesList[a], emailList[i]);

                    if (distance <= 2) // This number can be increased to match more loosely 
                    {
                        emailList[i] += " OK";
                        counterOfMatches++;
                        SemradEntriesList.Remove(SemradEntriesList[a]);
                        break;  
                    }
                }
            }

            return counterOfMatches;
        }
        private static void PrintResults(List<string> emailList, List<string> SemradEntriesList, int counterOfMatches)
        {
            foreach (var client in SemradEntriesList)
            {
                Console.WriteLine(client + " exists in Semrad Portal but not in the e-mail");
            }

            Console.WriteLine("\n---------------------------------------\n");

            foreach (var client in emailList)
            {
                Console.WriteLine(client);
            }

            Console.WriteLine($"\nFound {counterOfMatches} matches");
        }

        // Using Levenshten's distance algorithm we calculate how many actions does it take to transform one string into the other
        public static int ComputeLevenshteinDistance(string source, string target)
        {
            
            if ((string.IsNullOrWhiteSpace(source)) || (string.IsNullOrWhiteSpace(target))) return 1000;
            // returning 1000 so we are cetartain it doesn't pass similarity check
            if (source == target) return 0;

            int sourceSymbolCount = source.Length;
            int targetSymbolCount = target.Length;          

            int[,] distance = new int[sourceSymbolCount + 1, targetSymbolCount + 1];

            // Step 1
            for (int i = 0; i <= sourceSymbolCount; distance[i, 0] = i++) ;
            for (int j = 0; j <= targetSymbolCount; distance[0, j] = j++) ;

            for (int i = 1; i <= sourceSymbolCount; i++)
            {
                for (int j = 1; j <= targetSymbolCount; j++)
                {
                    // Step 2
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

                    // Step 3
                    distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
                }
            }
            return distance[sourceSymbolCount, targetSymbolCount];
        }
    }
}
