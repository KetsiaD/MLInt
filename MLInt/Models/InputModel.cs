using Microsoft.ML.Data;

public class InputModel
{
    [LoadColumn(0)] // textID column
    public string? TextID { get; set; }

    [LoadColumn(1)] // text column
    public string? Text { get; set; }

    [LoadColumn(2)] // selected_text column
    public string? SelectedText { get; set; }

    [LoadColumn(3)] // sentiment column
    public string? Sentiment { get; set; }

    [LoadColumn(4)] // Time of Tweet column
    public string? TimeOfTweet { get; set; }

    [LoadColumn(5)] // Age of User column
    public string? AgeOfUser { get; set; }

    [LoadColumn(6)] // Country column
    public string? Country { get; set; }

    [LoadColumn(7)] // Population - 2020 column
    public float Population2020 { get; set; }

    [LoadColumn(8)] // Land Area (Km²) column
    public float LandArea { get; set; }

    [LoadColumn(9)] // Density (P/Km²) column
    public float Density { get; set; }
}
