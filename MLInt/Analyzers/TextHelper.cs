using System;
using System.Collections.Generic;
using System.Linq;

public static class TextHelper
{
    // Tokenizing a text into sentences
    public static List<string> TokenizeSentencesOrParagraphs(string text)
{
    try
    {
        //checking if paragraphs are more than 5
        var paragraphs = text.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

        if (paragraphs.Length > 5)
        {
            return paragraphs.Select(paragraph => paragraph.Trim()).ToList();
        }

        return text.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
                   .Select(sentence => sentence.Trim())
                   .ToList();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error in TokenizeSentencesOrParagraphs: {ex.Message}");
        return null;
    }
}


    // Calculates cosine similarity between two sentences represented as vectors
    public static double CosineSimilarity(double[] vectorA, double[] vectorB)
{
    try
    {
        // Determing maximum length
        int maxLength = Math.Max(vectorA.Length, vectorB.Length);

        double dotProduct = 0, magnitudeA = 0, magnitudeB = 0;

        for (int i = 0; i < maxLength; i++)
        {
            double valueA = i < vectorA.Length ? vectorA[i] : 0; // Using 0 if index is out of bounds
            double valueB = i < vectorB.Length ? vectorB[i] : 0; 

            dotProduct += valueA * valueB; // dot product
            magnitudeA += Math.Pow(valueA, 2); // magnitude of vectorA
            magnitudeB += Math.Pow(valueB, 2); // magnitude of vectorB
        }

        return (magnitudeA == 0 || magnitudeB == 0) ? 0 : dotProduct / (Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error in cosineSimilarity: {ex.Message}");
        return 0;
    }
}


    // using tf-idf 
    public static Dictionary<string, double> GetTfIdfScores(List<string> sentences)
    {
        try{
            var termFrequency = new Dictionary<string, int>();
            foreach (var sentence in sentences)
            {
                var words = sentence.Split(' ');
                foreach (var word in words)
                {
                    if (!termFrequency.ContainsKey(word))
                        termFrequency[word] = 0;
                    termFrequency[word]++;
                }
            }
            return termFrequency.ToDictionary(kvp => kvp.Key, kvp => Math.Log(1 + kvp.Value));
        }catch(Exception ex){
            Console.WriteLine($"Error in GetTFIdFscores: {ex.Message}");
            return null;
        }
    }
}