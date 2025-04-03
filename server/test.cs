/*
using System.Net.Http.Json;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var handler = new HttpClientHandler
{
    AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
};
HttpClient client = new HttpClient(handler);
client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");

// In-memory cache
Dictionary<string, bool> tileCache = new();
Dictionary<int, object> seatInfoCache = new();
Dictionary<int, (string Platform, string Name)> sectorInfoCache = new();

// Pre-fetch and build cache of seat information by ID
var seatMetaUrl = "https://billet.fck.dk/Stadium/GetWGLSeats?eventId=5811";
try
{
    using var seatResponse = await client.GetAsync(seatMetaUrl);
    string seatJson = await seatResponse.Content.ReadAsStringAsync();

    if (seatResponse.IsSuccessStatusCode)
    {
        using var doc = JsonDocument.Parse(seatJson);
        var seats = doc.RootElement.GetProperty("seats");

        foreach (var seat in seats.EnumerateArray())
        {
            int id = seat.GetProperty("id").GetInt32();
            string label = seat.GetProperty("label").GetString() ?? "";
            string row = seat.GetProperty("row").GetString() ?? "";
            int paId = seat.GetProperty("paId").GetInt32();
            int sectorId = seat.GetProperty("sectorId").GetInt32();

            seatInfoCache[id] = new
            {
                Label = label,
                Row = row,
                PriceAreaId = paId,
                SectorId = sectorId
            };
        }
    }
    else
    {
        Console.WriteLine("Seat metadata request failed with non-success status code.");
    }
}
catch (Exception ex)
{
    Console.WriteLine("Exception while fetching seat metadata:");
    Console.WriteLine(ex);
}

// Fetch sector metadata
var sectorMetaUrl = "https://billet.fck.dk/Stadium/GetWGLSectors?eventId=5811";
try
{
    using var sectorResponse = await client.GetAsync(sectorMetaUrl);
    string sectorJson = await sectorResponse.Content.ReadAsStringAsync();

    if (sectorResponse.IsSuccessStatusCode)
    {
        using var doc = JsonDocument.Parse(sectorJson);
        var sectors = doc.RootElement.GetProperty("sectors");

        foreach (var sector in sectors.EnumerateArray())
        {
            int id = sector.GetProperty("id").GetInt32();
            string name = sector.GetProperty("name").GetString() ?? "";
            string platform = sector.GetProperty("platform").GetString() ?? "";

            sectorInfoCache[id] = (platform, name);
        }
    }
    else
    {
        Console.WriteLine("Sector metadata request failed with non-success status code.");
    }
}
catch (Exception ex)
{
    Console.WriteLine("Exception while fetching sector metadata:");
    Console.WriteLine(ex);
}

app.MapGet("/findAvailableSeats", async () =>
{
    List<object> validSeats = new();

    string combinedTiles = string.Join("v", TileConstants.AllTiles);
    string url = $"https://billet.fck.dk/Stadium/GetWGLSeatsOccInfo?eventId=5811&vaoKeysForCache={combinedTiles}";

    try
    {
        using var response = await client.GetAsync(url);
        string responseBody = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(responseBody) && responseBody != "[]")
        {
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            foreach (var seat in root.EnumerateArray())
            {
                if (seat.GetProperty("occ").GetInt32() == 0)
                {

                    int seatId = seat.GetProperty("id").GetInt32();

                    if (seatInfoCache.TryGetValue(seatId, out var infoObj))
                    {
                        dynamic info = infoObj;
                        int sectorId = info.SectorId;

                        sectorInfoCache.TryGetValue(sectorId, out var sectorInfo);

                        validSeats.Add(new
                        {
                            Id = seatId,
                            Label = info.Label,
                            Row = info.Row,
                            PriceAreaId = info.PriceAreaId,
                            SectorId = sectorId,
                            Sector = new { Platform = sectorInfo.Platform, Name = sectorInfo.Name },
                            anyRight = seat.GetProperty("anyRight").GetBoolean(),
                            hasSgRight = seat.GetProperty("hasSgRight").GetBoolean(),
                            hasResRight = seat.GetProperty("hasResRight").GetBoolean(),
                            occ = seat.GetProperty("occ")
                        });
                    }
                }
            }
        }
    }
    catch
    {
        return Results.Problem("Error fetching or parsing availability data.");
    }

    return Results.Ok(new { seats = validSeats });
})
.WithName("FindAvailableSeats");

app.Run();
*/