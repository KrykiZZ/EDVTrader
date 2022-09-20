using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace EDVTrader.API
{
    public class JournalAPI
    {
        public static readonly string JournalsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
            "Saved Games", "Frontier Developments", "Elite Dangerous");

        public static CommanderInfo? GetCommanderInfo()
        {
            FileInfo? file = new DirectoryInfo(JournalsDirectory)
                    .GetFiles().Where(x => x.Name.StartsWith("Journal."))
                    .OrderByDescending(x => x.LastWriteTime).FirstOrDefault();

            if (file == null)
                return null;

            var commander = new CommanderInfo();

            using (FileStream stream = File.Open(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(stream))
            {
                string[] lines = reader.ReadToEnd().Trim().Split('\n');
                foreach (string line in lines)
                {
                    JsonDocument doc = JsonDocument.Parse(line);
                    if (doc.RootElement.GetProperty("event").GetString() == "LoadGame")
                    {
                        commander.Name = doc.RootElement.GetProperty("Commander").GetString();
                        commander.Balance = doc.RootElement.GetProperty("Credits").GetInt64();
                    }
                    else if (doc.RootElement.GetProperty("event").GetString() == "Location")
                    {
                        if (doc.RootElement.TryGetProperty("StationName", out JsonElement stationNameProperty))
                            commander.Station = stationNameProperty.GetString();
                        else if (doc.RootElement.TryGetProperty("Body", out JsonElement bodyProperty))
                            commander.Body = bodyProperty.GetString();

                        commander.System = doc.RootElement.GetProperty("StarSystem").GetString();
                    }
                }

                return commander;
            }
        }
    }

    public class CommanderInfo
    {
        public string? Name { get; set; }
        public string? System { get; set; }
        public string? Station { get; set; }
        public string? Body { get; set; }
        public long Balance { get; set; }
    }
}
