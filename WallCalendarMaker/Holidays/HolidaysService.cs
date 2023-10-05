using System.Text.Json.Serialization;
using RestSharp;

namespace WallCalendarMaker.Holidays;

internal static class HolidaysService
{
    public sealed class DivAndEvents
    {
        public string? Division { get; set; }

        public List<AnEvent>? Events { get; set; }
    }

    public sealed class AnEvent
    {
        public string? Title { get; set; }

        public DateTime Date { get; set; }

        public string? Notes { get; set; }

        public bool Bunting { get; set; }
    }

    public sealed class Root
    {
        [JsonPropertyName("england-and-wales")]
        public DivAndEvents? EnglandAndWales { get; set; }

        [JsonPropertyName("scotland")]
        public DivAndEvents? Scotland { get; set; }

        [JsonPropertyName("northern-ireland")]
        public DivAndEvents? NorthernIreland { get; set; }
    }

    public static async Task<Root?> ExecuteAsync(CancellationToken cancellationToken = default)
    {
#pragma warning disable S1075
        using var client = new RestClient("https://www.gov.uk/");
#pragma warning restore S1075

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
