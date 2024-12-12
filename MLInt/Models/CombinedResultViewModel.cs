namespace MLInt.Models
{
    public class CombinedResultViewModel
    {
        public SentimentResultViewModel? SentimentResult { get; set; }
        public SentimentOutputModel? UserOutput { get; set; }
        public TextSummaryModel? TextSummary{ get; set; }
    }
}
