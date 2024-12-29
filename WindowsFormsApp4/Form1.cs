using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace WindowsFormsApp4
{
    public partial class Form1 : Form
    {
        private MLContext mlContext;
        private PredictionEngine<SentimentData, SentimentPrediction> predictionEngine;
        public Form1()
        {
            InitializeComponent();
            InitializeMLModel();

        }
        private void InitializeMLModel()
        {
           
            mlContext = new MLContext();

          
            var data = mlContext.Data.LoadFromTextFile<SentimentData>(
                path: @"sentiment_data.csv",
                hasHeader: true,
                separatorChar: ',',
                allowQuoting: true);

            var pipeline = mlContext.Transforms.Text.FeaturizeText("Features", "Text")
                          .Append(mlContext.Transforms.Conversion.MapValueToKey("Label", "Sentiment"))
                          .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
                          .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            var tts = mlContext.Data.TrainTestSplit(data, testFraction: 0.2);
            var model = pipeline.Fit(tts.TrainSet);
         
            
            predictionEngine = mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(model);
            
        }

        private void btnPredict_Click(object sender, EventArgs e)
        {
            var inputText = txtInput.Text;

            if (inputText.Length==0)
            {
                lblResult.Text = "لطفاً یک متن وارد کنید.";
                return;
            }

            var input = new SentimentData { Text = inputText };
            var prediction = predictionEngine.Predict(input);
            switch (prediction.PredictedLabel)
            {
                case "Positive":
                    lblResult.Text = "بنظر میرسه این متن حس خوبی داره ! ";
                    break;
                case "Negative":
                    lblResult.Text = "اوه اوه ... مث اینکه یکی اینجا ناراحته ";
                    break;
                case "Neutral":
                    lblResult.Text = "بی فایده س ! این متن هیچ حسی نداره  :(( ";
                    break ;
                default:
                    lblResult.Text = "اگه مطمئنی که این متن حاوی احساسه باید بیشتر روی دیتاستم کار کنی";
                    break;
            }
           
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }
        public class SentimentData
        {
            [LoadColumn(0)]
            public string Text { get; set; }

            [LoadColumn(1)]
            public string Sentiment { get; set; }
        }

        // Output class
        public class SentimentPrediction
        {
            [ColumnName("PredictedLabel")]
            public string PredictedLabel { get; set; }
        }
    }
}
