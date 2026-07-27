
/*
 * C# Code to fetch a published Google Doc HTML, parse a table of coordinates, and print a secret message. 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
/*
 * uses HttpClient to fetch the published Google Doc HTML.  
 * HTML parsing library via your terminal or Package Manager 
 */
using HtmlAgilityPack;

class Program
{
    static async Task Main(string[] args)
    {
        string docUrl = "https://docs.google.com/document/u/0/d/e/2PACX-1vTMOmshQe8YvaRXi6gEPKKlsC6UpFJSMAk4mQjLm_u1gmHdVVTaeh7nBNFBRlui0sTZ-snGwZM4DBCT/pub?pli=1";

        // Call the asynchronous function and block until complete for a console app template
        await PrintGoogleDocGridAsync(docUrl);
    }

    public static async Task PrintGoogleDocGridAsync(string url)
    {
        try
        {
            // 1. Fetch HTML content from the Google Doc URL
            using var client = new HttpClient();
            string htmlContent = await client.GetStringAsync(url);

            // 2. Load the HTML structure using HtmlAgilityPack
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            // Find the table element inside the document
            var table = doc.DocumentNode.SelectSingleNode("//table");
            if (table == null)
            {
                Console.WriteLine("Error: Data table not found in the document.");
                return;
            }

            // Extract all rows from the table
            var rows = table.SelectNodes(".//tr");
            if (rows == null || rows.Count <= 1)
            {
                Console.WriteLine("Error: Missing data rows.");
                return;
            }

            // 3. Find column indices dynamically using header labels
            var headerCells = rows[0].SelectNodes(".//td | .//th");
            int xIdx = 0, charIdx = 1, yIdx = 2; // Default fallbacks

            if (headerCells != null)
            {
                var headers = headerCells.Select(c => c.InnerText.Trim()).ToList();
                int foundX = headers.IndexOf("x-coordinate");
                int foundChar = headers.IndexOf("Character");
                int foundY = headers.IndexOf("y-coordinate");

                if (foundX != -1 && foundChar != -1 && foundY != -1)
                {
                    xIdx = foundX;
                    charIdx = foundChar;
                    yIdx = foundY;
                }
            }

            // 4. Parse rows into a structural list of coordinate points
            var dataPoints = new List<(int X, string Char, int Y)>();
            int maxX = 0;
            int maxY = 0;

            // Skip the first row (the header)
            foreach (var row in rows.Skip(1))
            {
                var cells = row.SelectNodes(".//td | .//th");
                if (cells == null || cells.Count < 3) continue;

                // Strip interior HTML spaces safely
                string rawX = cells[xIdx].InnerText.Trim();
                string rawChar = cells[charIdx].InnerText.Trim();
                string rawY = cells[yIdx].InnerText.Trim();

                if (int.TryParse(rawX, out int x) && int.TryParse(rawY, out int y))
                {
                    // Handle empty character cells safely as a text space
                    string character = string.IsNullOrEmpty(rawChar) ? " " : rawChar;

                    dataPoints.Add((x, character, y));

                    // Keep track of the grid boundaries dynamically
                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;
                }
            }

            if (dataPoints.Count == 0)
            {
                Console.WriteLine("Error: No coordinates could be parsed.");
                return;
            }

            // 5. Setup an empty structural array grid based on maximum sizes found
            // Coordinate boundaries are 0-indexed, so size requires max + 1
            string[,] grid = new string[maxY + 1, maxX + 1];

            // Fill array blocks initially with space padding
            for (int r = 0; r <= maxY; r++)
            {
                for (int c = 0; c <= maxX; c++)
                {
                    grid[r, c] = " ";
                }
            }

            // 6. Map coordinates directly onto the array space
            foreach (var point in dataPoints)
            {
                grid[point.Y, point.X] = point.Char;
            }

            // 7. Output rows sequentially to print the secret string graphic
            for (int r = 0; r <= maxY; r++)
            {
                for (int c = 0; c <= maxX; c++)
                {
                    Console.Write(grid[r, c]);
                }
                Console.WriteLine(); // New row line break
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
        }
    }
}