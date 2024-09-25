namespace MLInt.Models{
    public class SentimentResultViewModel
        {
            public double PositiveScore { get; set; }
            public double NegativeScore { get; set; }
            public double NeutralScore { get; set; }
            public double CompoundScore { get; set; }
        }
}