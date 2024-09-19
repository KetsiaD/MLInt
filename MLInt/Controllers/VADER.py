import sys
import csv
from nltk.sentiment.vader import SentimentIntensityAnalyzer

def analyze_and_generate_csv(input_file_path: str, output_csv_path: str):
    """
    Perform sentiment analysis on the content of the given input file and write the results to a CSV file.
    """
    # Open and read the input file
    with open(input_file_path, 'r') as file_:
        file_lines_list = file_.read().splitlines()

    # Initialize VADER sentiment analyzer
    vader = SentimentIntensityAnalyzer()

    # Lists to hold the sentiment values
    neg_list = []
    neu_list = []
    pos_list = []
    compound_list = []

    # Analyze sentiment for each line and collect the values
    for line in file_lines_list:
        vader_an = vader.polarity_scores(line)
        neg_list.append(vader_an['neg'])
        neu_list.append(vader_an['neu'])
        pos_list.append(vader_an['pos'])
        compound_list.append(vader_an['compound'])

    # Calculate the average of each sentiment score
    average_neg = sum(neg_list) / len(neg_list) if neg_list else 0
    average_neu = sum(neu_list) / len(neu_list) if neu_list else 0
    average_pos = sum(pos_list) / len(pos_list) if pos_list else 0
    average_compound = sum(compound_list) / len(compound_list) if compound_list else 0

    # Writing the average sentiment results to the CSV file
    with open(output_csv_path, mode='w', newline='') as csv_file:
        csv_writer = csv.writer(csv_file)
        # Write header
        csv_writer.writerow(['sentiment', 'value'])
        # Write average values for each sentiment
        csv_writer.writerow(['neg', average_neg])
        csv_writer.writerow(['neu', average_neu])
        csv_writer.writerow(['pos', average_pos])
        csv_writer.writerow(['compound', average_compound])

    print(f"CSV file created at: {output_csv_path}")

#analyze_and_generate_csv('/Users/ketsiadusenge/Desktop/Capstone/MLInt/MLInt/wwwroot/uploads/Phenomenal.txt', '/Users/ketsiadusenge/Desktop/Capstone/MLInt/MLInt/wwwroot/uploads/Phenomenal.csv')


