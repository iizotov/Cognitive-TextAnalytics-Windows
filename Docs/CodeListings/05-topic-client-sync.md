```cs
using System;
using System.Collections.Generic;
using Microsoft.ProjectOxford.Text.Topic;
using System.Threading;

namespace TopicClientSync
{
    class Program
    {
        static void Main(string[] args)
        {
            var apiKey = "YOUR-TEXT-ANALYTICS-API-SUBSCRIPTION-KEY";

            var randomText = new RandomText();

            var request = new TopicRequest();

            for (int i = 1; i <= 200; i++)
            {
                request.Documents.Add(new TopicDocument() { Id = i.ToString(), Text = randomText.Next() });
            }

            var client = new TopicClient(apiKey);
            var opeationUrl = client.StartTopicProcessing(request);

            TopicResponse response = null;
            var doneProcessing = false;

            while (!doneProcessing)
            {
                response = client.GetTopicResponse(opeationUrl);

                switch(response.Status)
                {
                    case TopicOperationStatus.Cancelled:
                        Console.WriteLine("Status: Operation Cancelled");
                        doneProcessing = true;
                        break;
                    case TopicOperationStatus.Failed:
                        Console.WriteLine("Status: Operation Failed");
                        doneProcessing = true;
                        break;
                    case TopicOperationStatus.NotStarted:
                        Console.WriteLine("Status: Operation Not Started");
                        Thread.Sleep(60000);
                        break;
                    case TopicOperationStatus.Running:
                        Console.WriteLine("Status: Operation Running");
                        Thread.Sleep(60000);
                        break;
                    case TopicOperationStatus.Succeeded:
                        Console.WriteLine("Status: Operation Succeeded");
                        doneProcessing = true;
                        break;
                }
            }

            foreach (var topic in response.OperationProcessingResult.Topics)
            {
                Console.WriteLine("{0} | {1}", topic.KeyPhrase, topic.Score);
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }
    }

    internal class RandomText
    {
        private Random _random;
        private List<string> _text;

        public RandomText()
        {
            _random = new Random();

            _text = new List<string>();

            _text.Add("I had a wonderful experience! The rooms were wonderful and the staff were helpful.");
            _text.Add("I had a terrible time at the hotel. The staff were rude and the food was awful.");
        }

        public string Next()
        {
            var index = _random.Next(0, 2);
            return _text[index];
        }
    }
}
```
