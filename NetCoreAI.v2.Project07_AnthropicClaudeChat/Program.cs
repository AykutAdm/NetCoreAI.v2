using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

class Program
{
    static async Task Main(string[] args)
    {
        string apiKey = "your-api-key";
        Console.Write("Lütfen sormak istediğiniz soruyu yazınız: ");
        string prompt = Console.ReadLine();

        using var client = new HttpClient();
        client.BaseAddress = new Uri("https://api.anthropic.com");
        client.DefaultRequestHeaders.Add("x-api-key", apiKey);
        client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var requestBody = new
        {
            model = "claude-sonnet-4-20250514",
            max_tokens = 1000,
            temperature = 0.7,
            messages = new[]
            {
                new
                {
                    role="user",
                    content=prompt
                }
            }
        };
        var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await client.PostAsync("v1/messages", jsonContent);
        var responseString = await response.Content.ReadAsStringAsync();

        var doc = JsonDocument.Parse(responseString);
        var contentElement = doc.RootElement.GetProperty("content")[0];
        var text = contentElement.GetProperty("text").GetString();

        // ANSI renk kodları
        string yellow = "\u001b[33m";
        string green = "\u001b[32m";
        string cyan = "\u001b[36m";
        string reset = "\u001b[0m";

        Console.WriteLine();
        Console.WriteLine($"{cyan}{new string('-', 60)}{reset}");
        Console.WriteLine($"{yellow} CLAUDE YANITI{reset}");
        Console.WriteLine($"{cyan}{new string('-', 60)}{reset}");
        Console.WriteLine();
        Console.WriteLine($"{green}{text}{reset}");
        Console.WriteLine();
        Console.WriteLine($"{cyan}{new string('-', 60)}{reset}");
        Console.WriteLine($"{yellow} Cevap tamamlandı.{reset}");
        Console.WriteLine($"{cyan}{new string('-', 60)}{reset}");
    }
}