// File: Services/SentimentAnalysisService.cs
using VaderSharp2;
using MLInt.Models;
using System.IO;
using System.Text;

namespace MLInt.Analyzers
{
    public class VaderSentimentAnalysis
    {
        /// <summary>
        /// Analyzes sentiment from the content of the file at the given path.
        /// </summary>
        /// <param name="filePath">Path to the text file to analyze.</param>
        /// <returns>A SentimentResultViewModel containing the sentiment scores.</returns>
        public SentimentResultViewModel AnalyzeSentiment(string filePath)
        {
            // Read the content of the file
            string textContent;
            using (var reader = new StreamReader(filePath, Encoding.UTF8))
            {
                textContent = reader.ReadToEnd();
            }

            // Initialize the sentiment analyzer and get scores
            var analyzer = new SentimentIntensityAnalyzer();
            var sentimentScores = analyzer.PolarityScores(textContent);

            // Populate the result view model with scores
            var sentimentResult = new SentimentResultViewModel
            {
                PositiveScore = sentimentScores.Positive,
                NegativeScore = sentimentScores.Negative,
                NeutralScore = sentimentScores.Neutral,
                CompoundScore = sentimentScores.Compound
            };

            return sentimentResult;
        }
    }
}
