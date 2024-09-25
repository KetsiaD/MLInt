from transformers import BertTokenizer, BertForSequenceClassification
import torch
import matplotlib.pyplot as plt

def bert_analysis(inputfilepath: str, outputfilepath: str, visualoutputfile: str):
    # Load pre-trained BERT tokenizer and model
    tokenizer = BertTokenizer.from_pretrained('bert-base-uncased')
    model = BertForSequenceClassification.from_pretrained('bert-base-uncased')

    # Set the model in evaluation mode
    model.eval()
    
    # Read text from your file
    with open(inputfilepath, 'r') as file:
        text = file.read()

    # Tokenize the input text
    inputs = tokenizer(text, return_tensors='pt', truncation=True, padding=True, max_length=512)
    
    # Forward pass through the model
    with torch.no_grad():
        outputs = model(**inputs)

    # Get the predicted class (logits)
    logits = outputs.logits
    predicted_class = torch.argmax(logits, dim=1).item()

    # Map the predicted class to a human-readable label
    labels = ['Negative', 'Positive']  # This is an example; adjust according to your task
    sentiment_label = labels[predicted_class]
    
    # Write the result to an output file
    with open(outputfilepath, 'w') as output_file:
        output_file.write(f'Predicted sentiment: {sentiment_label}\n')
    
    print(f'Predicted sentiment: {sentiment_label}')

    # Set the sentiment score for visualization
    sentiment_value = 1 if sentiment_label == 'Positive' else -1

    # Create a visualization of the sentiment
    plt.figure(figsize=(10, 2))
    plt.axhline(0, color='black', linewidth=0.5)  # Horizontal line at 0
    plt.plot(sentiment_value, 0, 'ro')  # Plot sentiment value as a point
    plt.text(sentiment_value, 0.1, f"{sentiment_label}", ha='center')

    # Customize the number line
    plt.xlim(-1, 1)
    plt.xticks([-1, 0, 1], ['Negative', 'Neutral', 'Positive'])
    plt.yticks([])
    plt.title('Sentiment Analysis Result')
    plt.grid(True, axis='x', linestyle='--', alpha=0.5)

    # Save the plot to the specified output file
    plt.savefig(visualoutputfile)
    plt.show()

# Example usage:
# bert_analysis(
#     inputfilepath="/Users/ketsiadusenge/Desktop/Capstone/MLInt/MLInt/wwwroot/uploads/testpositive.txt",
#     outputfilepath="/Users/ketsiadusenge/Desktop/Capstone/MLInt/MLInt/wwwroot/uploads/sentiment_output.txt",
#     visualoutputfile="/Users/ketsiadusenge/Desktop/Capstone/MLInt/MLInt/wwwroot/uploads/sentiment_visual.png"
# )
