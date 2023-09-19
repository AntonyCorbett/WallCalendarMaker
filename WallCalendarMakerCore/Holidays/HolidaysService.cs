using System.Text.Json.Serialization;
using RestSharp;

namespace WallCalendarMakerCore.Holidays;

internal class HolidaysService
{
    public class DivAndEvents
    {
        public string Division { get; set; }
        public List<AnEvent> Events { get; set; }
    }

    public class AnEvent
    {
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string Notes { get; set; }
        public bool Bunting { get; set; }
    }

    public class Root
    {
        [JsonPropertyName("england-and-wales")]
        public DivAndEvents EnglandAndWales { get; set; }

        [JsonPropertyName("scotland")]
        public DivAndEvents Scotland { get; set; }

        [JsonPropertyName("northern-ireland")]
        public DivAndEvents NorthernIreland { get; set; }
    }

    public async Task<Root?> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        using var client = new RestClient("https://www.gov.uk/");

        var request = new RestRequest("bank-holidays.json", Method.Get);

        try
        {
            return await client.GetAsync<Root>(request, cancellationToken);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
