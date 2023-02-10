using HtmlAgilityPack;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EDVTrader.API
{
    public class InaraAPI
    {
        public static NumberFormatInfo NumberFormatInfo
        {
            get
            {
                var nfi = NumberFormatInfo.CurrentInfo.Clone() as NumberFormatInfo;
                nfi.NumberDecimalSeparator = ".";
                return nfi;
            }
        }

        private Logger _logger = LogManager.GetCurrentClassLogger();
        private HttpClient _http;

        public InaraAPI(HttpClient http)
        {
            _http = http;
        }

        public async Task<InaraResult<List<StationInfo>>> GetNearestCommodities(string nearSystem, ShipSize padSize, int commodityId, int maxDistance, bool useFleetCarriers, bool usePlanetarySations)
        {
            string url =
                $"https://inara.cz/elite/commodities/" +
                $"?pi1=1" +
                $"&pa1[]={commodityId}" + 
                $"&ps1={nearSystem}" +
                $"&pi10=1" +
                $"&pi11={maxDistance}" +
                $"&pi3={(int)padSize}" +
                $"&pi9=0" +
                $"&pi4={(usePlanetarySations ? 0 : 1)}" +
                $"&pi5=72" +
                $"&pi12=0" +
                $"&pi7=0" +
                $"&pi8={(useFleetCarriers ? 0 : 1)}";

            var result = await _http.GetAsync(url);
            string html = await result.Content.ReadAsStringAsync();

            if (html.Contains("There is currently Inara update or maintenance ongoing."))
                return new() { Status = InaraResultStatus.Maintenance };

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var nothingFoundDiv = doc.DocumentNode.SelectSingleNode("/html/body/div[3]/div[2]/div[3]");
            if (nothingFoundDiv != null && nothingFoundDiv.InnerText == "No commodities were found.")
                return new() { Status = InaraResultStatus.Success, Result = new() };

            List<StationInfo> stations = new();
            foreach (HtmlNode row in doc.DocumentNode.SelectNodes("/html/body/div[3]/div[2]/div[3]/table/tbody/tr").Skip(1))
            {
                try
                {
                    StationInfo stationInfo;
                    
                    string typeNumber = Regex.Match(row.SelectSingleNode("td[1]/a/span[1]/div").Attributes["style"].Value, @": -(\d*)px").Groups[1].Value;
                    stations.Add(stationInfo = new StationInfo
                    {
                        Type = typeNumber switch
                        {
                            "13" => StationType.Coriolis,

                            "26" => StationType.Outpost,
                            "39" => StationType.Outpost,
                            "65" => StationType.Outpost,
                            "104" => StationType.Outpost,
                            "117" => StationType.Outpost,
                            "130" => StationType.Outpost,

                            "156" => StationType.Orbis,
                            "169" => StationType.Ocellus,
                            "182" => StationType.Planetary,
                            "195" => StationType.Planetary,
                            "247" => StationType.Asteroid,
                            "260" => StationType.Megaship,
                            "507" => StationType.Fleet,
                            "780" => StationType.Planetary,
                            _ => StationType.Unknown
                        },

                        System = row.SelectSingleNode("td[1]/a/span[2]").InnerText,
                        Name = string.Join("", row.SelectSingleNode("td[1]/a/span[1]").InnerText.SkipLast(3)),

                        PadSize = row.SelectSingleNode("//td[2]").InnerText switch
                        {
                            "S" => ShipSize.Small,
                            "M" => ShipSize.Medium,
                            "L" => ShipSize.Large,
                            _ => ShipSize.Unknown
                        },

                        DistanceToSystem = decimal.Parse(row.SelectSingleNode("td[4]").Attributes["data-order"].Value, NumberStyles.Float, NumberFormatInfo),
                        DistanceToStation = int.Parse(row.SelectSingleNode("td[3]").Attributes["data-order"].Value),

                        Supply = int.Parse(row.SelectSingleNode("td[5]").Attributes["data-order"].Value),
                        Price = int.Parse(row.SelectSingleNode("td[6]").Attributes["data-order"].Value),

                        UpdatedAt = new DateTime(1970, 1, 1, 0, 0, 0, 0).Add(TimeZoneInfo.Local.BaseUtcOffset).AddSeconds(long.Parse(row.SelectSingleNode("td[7]").Attributes["data-order"].Value))
                    });

                    if (stationInfo.Type == StationType.Unknown)
                        _logger.Warn($"Found unknown StationType. Station: {stationInfo.Name}, Number: {typeNumber}, Url: {url}.");
                }
                catch(Exception ex)
                {
                    _logger.Error($"An exception thrown while parsing stations:\n{ex}");
                }
            }

            return new InaraResult<List<StationInfo>>
            {
                Status = InaraResultStatus.Success,
                Result = stations
            };
        }

        public async Task<InaraResult<Dictionary<int, string>>> GetCommodityIds()
        {
            try
            {
                var result = await _http.GetAsync("https://inara.cz/elite/commodities/?pi1=1&pi2=46&ps1=Aeternitas&pi10=1&pi11=50&pi3=1&pi9=0&pi4=0&pi5=72&pi12=0&pi7=0&pi8=0");
                string html = await result.Content.ReadAsStringAsync();

                if (html.Contains("There is currently Inara update or maintenance ongoing."))
                    return new() { Status = InaraResultStatus.Maintenance };

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var dropdown = doc.DocumentNode.SelectSingleNode("//*[@id=\"tokenizeitems\"]");
                return new()
                {
                    Status = InaraResultStatus.Success,
                    Result = dropdown.SelectNodes("option").Select(x => KeyValuePair.Create(int.Parse(x.Attributes["value"].Value), x.InnerText)).ToDictionary(x => x.Key, x => x.Value)
                };
            }
            catch(Exception ex)
            {
                _logger.Error($"An exception thrown while parsing commodity id's:\n{ex}");
                return new() { Status = InaraResultStatus.Error };
            }
        }
    }

    public enum StationType
    {
        Unknown,
        Coriolis,
        Outpost,
        Orbis,
        Ocellus,
        Asteroid,
        Megaship,
        Fleet,
        Planetary
    }

    public enum ShipSize : byte
    {
        Unknown,
        Small = 1,
        Medium = 2,
        Large = 3
    }

    public class StationInfo
    {
        public StationType Type { get; set; }

        public string? System { get; set; }
        public string? Name { get; set; }

        public ShipSize PadSize { get; set; }

        public decimal DistanceToSystem { get; set; }
        public int DistanceToStation { get; set; }

        public int Supply { get; set; }
        public int Price { get; set; }

        public DateTime UpdatedAt { get; set; }
    }

    public enum InaraResultStatus
    {
        Maintenance,
        Error,
        Success
    }

    public class InaraResult<T>
    {
        public InaraResultStatus Status { get; set; }
        public T? Result { get; set; }
    }
}
