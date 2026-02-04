using NAudio.Wave;
using System.Net.Http.Headers;
using System.Speech.Synthesis;
using System.Text;
using System.Text.Json;


class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("🎤 Sesli Chatbot Başladı. Konuşmak için Enter tuşuna basınız...");

        while (true)
        {
            Console.ReadLine();
            string audioFilePath = "recorded.wav";

            //1. Mikrofon Kaydı Al
            Console.WriteLine("🎙️ Konuşmaya başlayın...");
            RecordAudio(audioFilePath);
            Console.WriteLine("🛑 Kayıt Tamamlandı...");

            //2. OpenAI Whisper Api ile yazıya çevir
            string transcription = await TranscribeAudioAsync(audioFilePath);
            Console.WriteLine($"👤 Sen: {transcription}");

            //3. ChatGpt ye Soruyu Göder
            string reply = await AskChatgptAsync(transcription);
            Console.WriteLine($"🤖 Chatbot: {reply}");

            //4. Yanıtı Seslendir
            var synth = new SpeechSynthesizer();
            synth.Speak(reply);
        }
    }

    static void RecordAudio(string outputFilePath)
    {
        using var waveIn = new WaveInEvent();
        waveIn.WaveFormat = new WaveFormat(16000, 1);
        using var writer = new WaveFileWriter(outputFilePath, waveIn.WaveFormat);
        waveIn.DataAvailable += (s, a) => writer.Write(a.Buffer, 0, a.BytesRecorded);
        waveIn.StartRecording();
        Thread.Sleep(10000);
        waveIn.StopRecording();
    }

    static async Task<string> TranscribeAudioAsync(string audioFilePath)
    {
        string apiKey = "your-api-key";
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        using var form = new MultipartFormDataContent();
        using var fs = File.OpenRead(audioFilePath);
        form.Add(new StreamContent(fs), "file", Path.GetFileName(audioFilePath));
        form.Add(new StringContent("whisper-1"), "model");

        var response = await httpClient.PostAsync("https://api.openai.com/v1/audio/transcriptions", form);
        var result = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(result);
        return doc.RootElement.GetProperty("text").GetString();
    }

    static async Task<string> AskChatgptAsync(string userMessage)
    {
        string apiKey = "your-api-key";
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        var payload = new
        {
            model = "gpt-3.5-turbo",
            messages = new[]
            {
            new
            {
                role="user",
                content=userMessage
            }
        }
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
        var result = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(result);
        return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
    }
}