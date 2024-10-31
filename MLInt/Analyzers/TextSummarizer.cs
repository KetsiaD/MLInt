public class TextRankSummarizer
{
    public static string Summarize(string text)
    {
        var sentences = TextHelper.TokenizeSentences(text);
        int totalSentences = sentences.Count;
        
        // Determine the number of summary sentences (e.g., 30% of total sentences)
        int sentenceCount = (int)(totalSentences * 0.4);
        sentenceCount = Math.Max(1, sentenceCount); // Ensure at least 1 sentence in summary
        
        // Step 1: Build TF-IDF vectors for each sentence
        var tfIdfScores = TextHelper.GetTfIdfScores(sentences);
        double[,] similarityMatrix = new double[totalSentences, totalSentences];

        // Step 2: Populate similarity matrix using cosine similarity
        for (int i = 0; i < totalSentences; i++)
        {
            for (int j = i + 1; j < totalSentences; j++)
            {
                similarityMatrix[i, j] = TextHelper.CosineSimilarity(
                    sentences[i].Split(' ').Select(word => tfIdfScores.ContainsKey(word) ? tfIdfScores[word] : 0).ToArray(),
                    sentences[j].Split(' ').Select(word => tfIdfScores.ContainsKey(word) ? tfIdfScores[word] : 0).ToArray()
                );
                similarityMatrix[j, i] = similarityMatrix[i, j]; // Symmetric matrix
            }
        }

        // Step 3: Implement TextRank (similar to PageRank)
        double[] ranks = Enumerable.Repeat(1.0, totalSentences).ToArray(); // Initial rank values
        double dampingFactor = 0.85, epsilon = 0.001; // Damping factor and convergence threshold
        bool converged;

        do
        {
            double[] newRanks = new double[totalSentences];
            converged = true;

            for (int i = 0; i < totalSentences; i++)
            {
                newRanks[i] = (1 - dampingFactor) / totalSentences;
                for (int j = 0; j < totalSentences; j++)
                {
                    if (i != j && similarityMatrix[j, i] > 0)
                    {
                        int linkCount = Enumerable.Range(0, totalSentences).Count(k => similarityMatrix[j, k] > 0);
                        newRanks[i] += dampingFactor * (similarityMatrix[j, i] / linkCount) * ranks[j];
                    }
                }
                if (Math.Abs(newRanks[i] - ranks[i]) > epsilon) converged = false;
            }
            ranks = newRanks;
        } while (!converged);

        // Step 4: Select top sentences by rank
        var topSentences = sentences.Zip(ranks, (sentence, rank) => new { sentence, rank })
                                    .OrderByDescending(x => x.rank)
                                    .Take(sentenceCount)
                                    .Select(x => x.sentence)
                                    .ToList();

        return string.Join(". ", topSentences) + ".";
    }
}
