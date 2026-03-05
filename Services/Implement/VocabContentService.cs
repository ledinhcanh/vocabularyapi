using API.Models.External;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace API.Services.Implement
{
    public class VocabContentService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public VocabContentService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<(string? phonetic, string? audioUrl, string? example, string? imageUrl)> GetWordInfoAsync(string word)
        {
            try
            {
                var response = await _httpClient.GetAsync($"https://api.dictionaryapi.dev/api/v2/entries/en/{word}");

                if (!response.IsSuccessStatusCode) return (null, null, null, null);

                var jsonString = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var data = JsonSerializer.Deserialize<List<DictionaryApiResponse>>(jsonString, options);

                if (data == null || data.Count == 0) return (null, null, null, null);

                var entry = data[0];

                // --- SỬA ĐỔI LOGIC LẤY AUDIO TẠI ĐÂY ---

                string audio = null;
                string phonetic = entry.Phonetic; // Mặc định lấy phonetic chung

                if (entry.Phonetics != null && entry.Phonetics.Count > 0)
                {
                    // BƯỚC 1: Cố gắng tìm Audio có đuôi "-us.mp3" (Ưu tiên Mỹ)
                    var usPhoneticObj = entry.Phonetics.FirstOrDefault(p =>
                        !string.IsNullOrEmpty(p.Audio) && p.Audio.Contains("-us.mp3"));

                    if (usPhoneticObj != null)
                    {
                        audio = usPhoneticObj.Audio;
                        // Nếu tìm thấy audio US, thì lấy luôn phiên âm (text) của US cho đồng bộ
                        if (!string.IsNullOrEmpty(usPhoneticObj.Text))
                        {
                            phonetic = usPhoneticObj.Text;
                        }
                    }
                    else
                    {
                        // BƯỚC 2: Nếu không có US, thì tìm cái nào có Audio bất kỳ (UK, AU...)
                        var anyAudioObj = entry.Phonetics.FirstOrDefault(p => !string.IsNullOrEmpty(p.Audio));
                        if (anyAudioObj != null)
                        {
                            audio = anyAudioObj.Audio;
                            // Lấy text đi kèm nếu phonetic chung bị null
                            if (string.IsNullOrEmpty(phonetic)) phonetic = anyAudioObj.Text;
                        }
                    }
                }

                // Fix trường hợp vẫn null phonetic thì lấy cái đầu tiên có text
                if (string.IsNullOrEmpty(phonetic) && entry.Phonetics != null)
                {
                    phonetic = entry.Phonetics.FirstOrDefault(p => !string.IsNullOrEmpty(p.Text))?.Text;
                }

                // ----------------------------------------

                // Lấy ví dụ (Example) - Giữ nguyên logic cũ
                string example = null;
                if (entry.Meanings != null)
                {
                    foreach (var meaning in entry.Meanings)
                    {
                        var def = meaning.Definitions?.FirstOrDefault(d => !string.IsNullOrEmpty(d.Example));
                        if (def != null)
                        {
                            example = def.Example;
                            break;
                        }
                    }
                }

                // Nếu DictionaryAPI không có ví dụ, thử dùng AI (Gemini) để tạo
                if (string.IsNullOrEmpty(example))
                {
                    example = await GenerateExampleSentenceWithAI(word, null);
                }

                // --- THÊM LOGIC LẤY ẢNH (Unsplash Source API) ---
                // Dùng Unsplash Source API: sẽ tự động redirect tới 1 ảnh placeholder phù hợp với từ khóa
                // Mặc định trả về link ảnh Unsplash Source (không cần API key, nhưng đôi khi chậm).
                // Thực tế nên dùng Pixabay API lấy link tĩnh. Nhưng ở đây tạm dùng link định dạng Unsplash:
                string imageUrl = $"https://source.unsplash.com/400x400/?{Uri.EscapeDataString(word)}";

                return (phonetic, audio, example, imageUrl);
            }
            catch
            {
                return (null, null, null, null);
            }
        }

        private async Task<string> GenerateExampleSentenceWithAI(string word, string meaning)
        {
            try
            {
                var apiKey = _configuration["Gemini:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    // Fallback nếu chưa cấu hình key
                    return $"I need to learn how to use the word '{word}' in a sentence.";
                }

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={apiKey}";
                
                string prompt = string.IsNullOrEmpty(meaning) 
                    ? $"Generate a single, short, simple English example sentence using the word '{word}'. Do not include translations or extra details. Just the sentence."
                    : $"Generate a single, short, simple English example sentence using the word '{word}' (meaning: {meaning}). Do not include translations or extra details. Just the sentence.";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[] { new { text = prompt } }
                        }
                    }
                };

                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var resultJson = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(resultJson);
                    var text = doc.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();
                    
                    return text?.Trim()?.Replace("\"", "") ?? $"Here is an example with '{word}'.";
                }

                return $"Here is an example with '{word}'.";
            }
            catch
            {
                return $"For example: '{word}'.";
            }
        }
    }
}