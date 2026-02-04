using System.Net.Http.Headers;
using System.Text.Json;

var apiKey = "your-api-key";
var filePath = "Skillet.mp3";

if (!File.Exists(filePath))
{
    Console.WriteLine("Dosya Bulunamadı!");
    return;
}

using var client = new HttpClient();
client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", apiKey);
using var fileStream = File.OpenRead(filePath);

var content = new StreamContent(fileStream);
content.Headers.ContentType = new MediaTypeHeaderValue("audio/mp3");

var response = await client.PostAsync("https://api.deepgram.com/v1/listen", content);
var json = await response.Content.ReadAsStringAsync();

try
{
    var doc = JsonDocument.Parse(json);
    var transcript = doc.RootElement
        .GetProperty("results")
        .GetProperty("channels")[0]
        .GetProperty("alternatives")[0]
        .GetProperty("transcript")
        .GetString();
    Console.WriteLine();
    Console.WriteLine("Transkript Metni: \n");
    Console.WriteLine(transcript);
}
catch (Exception ex)
{
    Console.WriteLine("Json Çözümleme sırasında bir hata oluştu");
    Console.WriteLine(ex.Message);
    Console.WriteLine("\n Gelen Yanıt \n" + json);
    throw;
}