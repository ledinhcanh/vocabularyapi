namespace API.Models.External
{
    // Cấu trúc JSON trả về của Free Dictionary API
    public class DictionaryApiResponse
    {
        public string Word { get; set; }
        public string Phonetic { get; set; } // IPA chung
        public List<PhoneticData> Phonetics { get; set; }
        public List<MeaningData> Meanings { get; set; }
    }

    public class PhoneticData
    {
        public string Text { get; set; } // IPA chi tiết
        public string Audio { get; set; } // Link MP3
    }

    public class MeaningData
    {
        public List<DefinitionData> Definitions { get; set; }
    }

    public class DefinitionData
    {
        public string Definition { get; set; }
        public string Example { get; set; }
    }
}