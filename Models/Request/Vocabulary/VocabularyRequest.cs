namespace API.Models.Request.Vocabulary
{
    public class CreateVocabularyRequest
    {
        public int TopicId { get; set; }
        public string Word { get; set; }
        public string Meaning { get; set; }
        public string? Phonetic { get; set; }
        public string? AudioUrl { get; set; }
        public string? ImageUrl { get; set; }
        public string? ExampleSentence { get; set; }
    }

    public class UpdateVocabularyRequest : CreateVocabularyRequest
    {
        public int Id { get; set; }
    }
    public class SubmitReviewRequest
    {
        public int VocabId { get; set; }
        public int Quality { get; set; }
    }
}