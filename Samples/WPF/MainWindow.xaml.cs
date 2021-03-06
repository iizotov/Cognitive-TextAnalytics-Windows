﻿using Microsoft.ProjectOxford.Text.Controls;
using Microsoft.ProjectOxford.Text.Helpers;
using Microsoft.ProjectOxford.Text.Topic;
using SampleUserControlLibrary;
using System.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.IO;
using System.Web.Script.Serialization;
using Microsoft.ProjectOxford.Text.Core;
using Microsoft.ProjectOxford.Text.Language;
using Microsoft.ProjectOxford.Text.Sentiment;
using Microsoft.ProjectOxford.Text.KeyPhrase;
using Newtonsoft.Json;
using System.Data.Entity.Design.PluralizationServices;
using Microsoft.Cognitive.LUIS;

namespace Microsoft.ProjectOxford.Text
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
            ServicePointManager.DefaultConnectionLimit = 1000;

            this.ViewModel = new MainViewModel()
            {
                LanguageIdentificationDescription = "Identify the language text is written in.",
                SentimentAnalysisDescription = "Identity the sentiment in text.",
                KeyPhraseDetectionDescription = "Detect the key phrases in text.",
                TopicDetectionDescription = "Detect topics in text"
            };

            this.DataContext = this.ViewModel;

            this._scenariosControl.SampleScenarioList = new Scenario[]
            {
                new Scenario()
                {
                    PageClass = typeof(WorkPage),
                    Title = "Text Magic"
                },
                new Scenario()
                {
                    PageClass = typeof(LanguageIdentificationPage),
                    Title = "Language Identification"
                },
                new Scenario()
                {
                    PageClass = typeof(SentimentAnalysisPage),
                    Title = "Sentiment Analysis"
                },
                new Scenario()
                {
                    PageClass = typeof(KeyPhraseDetectionPage),
                    Title = "Key Phrase Detection"
                },
                new Scenario()
                {
                    PageClass = typeof(TopicDetectionPage),
                    Title = "Topic Detection"
                }
            };
        }

        #endregion Constructors
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var windowClipboardManager = new ClipboardManager(this);
            windowClipboardManager.ClipboardChanged += ClipboardChanged;
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            test();
        }

        private string ttsMessage = "";
        private string lastMessage = "";
        private List<string> lastNMessages = new List<string>();
        private const int SENTENCE_LIMIT = 5;

        private System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();


        private string sentimentUrl = "https://api.powerbi.com/beta/72f988bf-86f1-41af-91ab-2d7cd011db47/datasets/a6540be2-a8b2-4b68-a0d9-69117fdb68a5/rows?key=osaQDlM2hUdzzCBJ3JrvghsjJGarSm%2BTTUJz6qcfEsWjjyNh3VXVH0ik7RbVYZ3BDgbkVigV7Xm6uBOCQ1N0lg%3D%3D";

        private string powerBIAppId = "820450be-2296-4123-afdb-9ff5a55daf6e";
        private string luisSubscriptionKey = "2a9c536b2356498ba88f21bfc76da65a";
        private string luisAppId = "57c5fb6d-ffde-4eeb-8411-a6e3e3e1b345";
        private LuisResult prevResult { get; set; }

        private string sentimentDatasetName = "_TextAPISentiment";
        private string keywordDatasetName = "_TextAPIKeywords";

        private string keywordUrl = "https://api.powerbi.com/beta/72f988bf-86f1-41af-91ab-2d7cd011db47/datasets/6603e099-0265-41ef-a6d2-2b0616b587c8/rows?key=htKJW0r0VcrE7hfSPVlRB%2BsTeNkJykSEc0RB%2FzAZfbaijac5Y0oGKit0MgCTNEaSuFzhpurmJiPG8o7YswaugQ%3D%3D";

        private string keyPhrasesString = "";
        private Dictionary<string, int> keyPhrasesDict = new Dictionary<string, int>();
        private List<string> keyPhrasesList = new List<string>();

        private PluralizationService ps = PluralizationService.CreateService(new CultureInfo("en-us"));
        private static string testText = 
            @"October arrived, spreading a damp chill over the grounds and into the castle. Madam Pomfrey, the nurse, was kept busy by a sudden spate of colds among the staff and students. Her Pepperup potion worked instantly, though it left the drinker smoking at the ears for several hours afterward. Ginny Weasley, who had been looking pale, was bullied into taking some by Percy. The steam pouring from under her vivid hair gave the impression that her whole head was on fire. Raindrops the size of bullets thundered on the castle windows for days on end; the lake rose, the flower beds turned into muddy streams, and Hagrid's pumpkins swelled to the size of garden sheds. Oliver Wood's enthusiasm for regular training sessions, however, was not dampened, which was why Harry was to be found, late one stormy Saturday afternoon a few days before Halloween, returning to Gryffindor Tower, drenched to the skin and splattered with mud. Even aside from the rain and wind it hadn't been a happy practice session. Fred and George, who had been spying on the Slytherin team, had seen for themselves the speed of those new Nimbus Two Thousand and Ones. They reported that the Slytherin team was no more than seven greenish blurs, shooting through the air like missiles.";
        private static string[] testTextArray = testText.Split('.');

        private int testIterator = 0;



        private void ClipboardChanged(object sender, EventArgs e)
        {
            // Handle your clipboard update here, debug logging example:
            if (Clipboard.ContainsText() && Clipboard.GetText() != lastMessage)
            {
                try
                {
                    this.ttsMessage += Clipboard.GetText();
                    this.lastMessage = Clipboard.GetText();
                    this.lastNMessages.Add(this.lastMessage);
                    ((MainWindow)Application.Current.MainWindow)._scenariosControl.LogTTS(Clipboard.GetText());

                    //LUIS - in the top left corner, popping up the window / push message
                    //Add highlights as the text is spoken
                    //Load test

                    Detect_Key_Phrases(this.lastMessage);
                    Predict(this.lastMessage);

                    //add accumulators - sent after there's been N sentences
                    while(lastNMessages.Count > SENTENCE_LIMIT && SENTENCE_LIMIT > 0)
                    {
                        lastNMessages.RemoveAt(0);
                    }
                    Get_Sentiment(this.lastMessage, String.Join(" ", this.lastNMessages));


                }
                catch (Exception ex)
                {
                    Log(ex.Message);
                }
            }
        }


        #region Properties

        /// <summary>
        /// Gets view model instance for MainWindow
        /// </summary>
        public MainViewModel ViewModel
        {
            get; private set;
        }

        #endregion Properties

        #region Methods
        private async void Detect_Key_Phrases(string text)
        {
            try
            {
                var document = new KeyPhraseDocument() { Id = Guid.NewGuid().ToString(), Text = text, Language = "en" };

                var request = new KeyPhraseRequest();
                request.Documents.Add(document);

                MainWindow mainWindow = Window.GetWindow(this) as MainWindow;
                var client = new KeyPhraseClient(mainWindow._scenariosControl.SubscriptionKey);

                var response = await client.GetKeyPhrasesAsync(request);
                Log("Response: Success. Key phrases identified.");

                Dictionary<string, int> deltaPhrasesDict = new Dictionary<string, int>();

                var keyPhrases = new StringBuilder();

                //add only new phrases
                keyPhrasesList.AddRange(response.Documents[0].KeyPhrases.Where(x => !keyPhrasesList.Any(y => y == x)));

                //((MainWindow)Application.Current.MainWindow)._scenariosControl.HighlightWords(keyPhrasesList);
                ((MainWindow)Application.Current.MainWindow)._scenariosControl.HighlightWords(response.Documents[0].KeyPhrases);


                foreach (var keyPhrase in response.Documents[0].KeyPhrases.ConvertAll(d => ps.Singularize(d.ToLower())))
                {
                    if (keyPhrase.Trim() != "")
                    {
                        keyPhrases.Append(string.Format("{0}; ", keyPhrase));
                        if (keyPhrasesDict.ContainsKey(keyPhrase))
                        {
                            keyPhrasesDict[keyPhrase] = keyPhrasesDict[keyPhrase] + 1;
                            deltaPhrasesDict.Add(keyPhrase, keyPhrasesDict[keyPhrase]);
                        }
                        else
                        {
                            keyPhrasesDict.Add(keyPhrase, 1);
                            deltaPhrasesDict.Add(keyPhrase, 1);
                        }
                    }
                }
                keyPhrasesString = string.Join("; ", keyPhrasesDict.Keys);

                ((MainWindow)Application.Current.MainWindow)._scenariosControl.LogKeywords(keyPhrasesString);


                if (sendToPBICheckbox.IsChecked == true)
                {
                    string json = JsonConvert.SerializeObject(deltaPhrasesDict.Select(e => new { keyword = e.Key, frequency = e.Value, dt = (DateTime.UtcNow.ToString("s") + "Z") }));
                    //post(keywordUrl, json);
                    Thread myNewThread = new Thread(() => post(keywordUrl, json));
                    myNewThread.Start();
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }

        private async void Get_Sentiment(string text, string lastNTest)
        {

            try
            {
                var document = new SentimentDocument() { Id = Guid.NewGuid().ToString(), Text = text, Language = "en" };
                var lastNDocument = new SentimentDocument() { Id = Guid.NewGuid().ToString(), Text = lastNTest, Language = "en" };

                var request = new SentimentRequest();
                request.Documents.Add(document);
                request.Documents.Add(lastNDocument);

                MainWindow mainWindow = Window.GetWindow(this) as MainWindow;
                var client = new SentimentClient(mainWindow._scenariosControl.SubscriptionKey);

                var response = await client.GetSentimentAsync(request);
                Log("Response: Success. Sentiment analyzed.");

                var score = response.Documents[0].Score * 100;
                var lastNScore = response.Documents[1].Score * 100;
                ((MainWindow)Application.Current.MainWindow)._scenariosControl.LogSentiment(string.Format("Last sentence: {0}%\nLast {1} sentences: {2}%", score, SENTENCE_LIMIT, lastNScore));

                if (sendToPBICheckbox.IsChecked == true)
                {
                    //post(sentimentUrl, "[" + json + "]");
                    string json = JsonConvert.SerializeObject(new { dt = (DateTime.UtcNow.ToString("s") + "Z"), sentence = text, sentiment_Nlast = score, average_sentiment = lastNScore });
                    Thread myNewThread = new Thread(() => post(sentimentUrl, "[" + json + "]"));
                    myNewThread.Start();
                }

            }
            catch (Exception ex)
            {
                MainWindow.Log(ex.Message);
            }
        }

        private static void post(string url, string jsonContent)
        {
            var webRequest = WebRequest.Create(url);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/json";

            var bytes = Encoding.ASCII.GetBytes(jsonContent);

            var requestStream = webRequest.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);

            try
            {
                var response = webRequest.GetResponse();
            }
            catch (WebException ex)
            {
                Log(ex.Message);
            }
        }

        public dataset[] GetDatasets()
        {
            //This is sample code to illustrate a Power BI operation.   
            //In a production application, refactor code into specific methods and use appropriate exception handling.  

            //The client id that Azure AD creates when you register your client app.  
            //  To learn how to register a client app, see https://msdn.microsoft.com/en-US/library/dn877542(Azure.100).aspx           
            string clientID = powerBIAppId;

            //RedirectUri you used when you register your app.  
            //For a client app, a redirect uri gives Azure AD more details on the application that it will authenticate.  
            // You can use this redirect uri for your client app  
            string redirectUri = "https://login.live.com/oauth20_desktop.srf";

            //Resource Uri for Power BI API  
            string resourceUri = "https://analysis.windows.net/powerbi/api";

            //OAuth2 authority Uri  
            string authorityUri = "https://login.windows.net/common/oauth2/authorize";

            string powerBIApiUrl = "https://api.powerbi.com/v1.0/myorg/datasets";

            //Get access token:   
            // To call a Power BI REST operation, create an instance of AuthenticationContext and call AcquireToken  
            // AuthenticationContext is part of the Active Directory Authentication Library NuGet package  
            // To install the Active Directory Authentication Library NuGet package in Visual Studio,   
            //  run "Install-Package Microsoft.IdentityModel.Clients.ActiveDirectory" from the nuget Package Manager Console.  

            // AcquireToken will acquire an Azure access token  
            // Call AcquireToken to get an Azure token from Azure Active Directory token issuance endpoint  
            AuthenticationContext authContext = new AuthenticationContext(authorityUri);
            string token = authContext.AcquireToken(resourceUri, clientID, new Uri(redirectUri), PromptBehavior.Auto).AccessToken;

            //GET web request to list all datasets.  
            //To get a datasets in a group, use the Groups uri: https://api.powerbi.com/v1.0/myorg/groups/{group_id}/datasets  
            HttpWebRequest request = System.Net.WebRequest.Create(powerBIApiUrl) as System.Net.HttpWebRequest;
            request.KeepAlive = true;
            request.Method = "GET";
            request.ContentLength = 0;
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", String.Format("Bearer {0}", token));

            //Get HttpWebResponse from GET request  
            using (HttpWebResponse httpResponse = request.GetResponse() as System.Net.HttpWebResponse)
            {
                //Get StreamReader that holds the response stream  
                using (StreamReader reader = new System.IO.StreamReader(httpResponse.GetResponseStream()))
                {
                    string responseContent = reader.ReadToEnd();

                    JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                    Datasets datasets = (Datasets)jsonSerializer.Deserialize(responseContent, typeof(Datasets));
                    Log("Authenticate + GetDatasets()");
                    return datasets.value;
                }
            }
        }

        public string DeleteRows(string datasetName)
        {
            //This is sample code to illustrate a Power BI operation.   
            //In a production application, refactor code into specific methods and use appropriate exception handling.  

            //The client id that Azure AD creates when you register your client app.  
            //  To learn how to register a client app, see https://msdn.microsoft.com/en-US/library/dn877542(Azure.100).aspx    
            string clientID = powerBIAppId;

            //Assuming you have a dataset named SalesMarketing  
            // To get a dataset id, see Get Datasets operation.             
            dataset[] datasets = GetDatasets();
            string datasetId = (from d in datasets where d.Name == datasetName select d).FirstOrDefault().Id;

            // Assumes the Dataset named SalesMarketing has a Table named Product  
            string tableName = "RealTimeData";

            //RedirectUri you used when you register your app.  
            //For a client app, a redirect uri gives Azure AD more details on the application that it will authenticate.  
            // You can use this redirect uri for your client app  
            string redirectUri = "https://login.live.com/oauth20_desktop.srf";

            //Resource Uri for Power BI API  
            string resourceUri = "https://analysis.windows.net/powerbi/api";

            //OAuth2 authority Uri  
            string authorityUri = "https://login.windows.net/common/oauth2/authorize";

            string powerBIApiUrl = String.Format("https://api.powerbi.com/v1.0/myorg/datasets/{0}/tables/{1}/Rows", datasetId, tableName);

            //Get access token:   
            // To call a Power BI REST operation, create an instance of AuthenticationContext and call AcquireToken  
            // AuthenticationContext is part of the Active Directory Authentication Library NuGet package  
            // To install the Active Directory Authentication Library NuGet package in Visual Studio,   
            //  run "Install-Package Microsoft.IdentityModel.Clients.ActiveDirectory" from the nuget Package Manager Console.  

            // AcquireToken will acquire an Azure access token  
            // Call AcquireToken to get an Azure token from Azure Active Directory token issuance endpoint  
            AuthenticationContext authContext = new AuthenticationContext(authorityUri);
            string token = authContext.AcquireToken(resourceUri, clientID, new Uri(redirectUri), PromptBehavior.Auto).AccessToken;

            //DELETE web request to delete rows.  
            //To delete rows in a group, use the Groups uri: https://api.powerbi.com/v1.0/myorg/groups/{group_id}/datasets/{dataset_id}/tables/{table_name}/rows  
            HttpWebRequest request = System.Net.WebRequest.Create(powerBIApiUrl) as System.Net.HttpWebRequest;
            request.KeepAlive = true;
            request.Method = "DELETE";
            request.ContentLength = 0;
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", String.Format("Bearer {0}", token));

            //Get HttpWebResponse from GET request  
            using (HttpWebResponse httpResponse = request.GetResponse() as System.Net.HttpWebResponse)
            {
                //Get StreamReader that holds the response stream  
                using (StreamReader reader = new System.IO.StreamReader(httpResponse.GetResponseStream()))
                {
                    string responseContent = reader.ReadToEnd();
                    Log("Cleaned Dataset " + datasetName);
                    return responseContent;
                    
                }
            }
        }

        private void btnReply_Click(object sender, RoutedEventArgs e)
        {
            if (prevResult == null || (prevResult.DialogResponse != null
                && prevResult.DialogResponse.Status == "Finished"))
            {
                Log("There is nothing to reply to.");
                return;
            }
            try
            {
                //Reply();
            }
            catch (Exception exception)
            {
                Log(exception.Message);
            }
        }

        public async void Predict(string text)
        {
            string appId = luisAppId;
            string subscriptionKey = luisSubscriptionKey;
            bool preview = true;
            try
            {
                LuisClient client = new LuisClient(appId, subscriptionKey, preview);
                LuisResult res = await client.Predict(text);
                processRes(res);
                Log("Predicted successfully.");
            }
            catch (System.Exception exception)
            {
                Log(exception.Message);
            }
        }

        public async void Reply(string text)
        {
            string appId = luisAppId;
            string subscriptionKey = luisSubscriptionKey;
            bool preview = true;
            try
            {
                LuisClient client = new LuisClient(appId, luisSubscriptionKey, preview);
                LuisResult res = await client.Reply(prevResult, text);
                processRes(res);
                Log("Replied successfully.");
            }
            catch (System.Exception exception)
            {
                Log(exception.Message);
            }
        }

        private void processRes(LuisResult res)
        {
            string entityValue = "";

            if (res.TopScoringIntent.Score < 0.7 || res.TopScoringIntent.Name == "None")
            {
                if (loopTestCheckbox.IsChecked == false)
                {
                    return;
                }
            }
            prevResult = res;
            
            //((MainWindow)Application.Current.MainWindow)._scenariosControl.LogLuis(res.OriginalQuery);
            ((MainWindow)Application.Current.MainWindow)._scenariosControl.LogLuis("\"" + res.OriginalQuery + "\": LUIS Intent: " + res.TopScoringIntent.Name + ", confidence: " + (res.TopScoringIntent.Score * 100).ToString().Substring(0,4) + "%");
            List<string> entitiesNames = new List<string>();
            var entities = res.GetAllEntities();
            foreach (Entity entity in entities)
            {
                entitiesNames.Add(entity.Name);
                ((MainWindow)Application.Current.MainWindow)._scenariosControl.LogLuis(", " + entity.Name + ": " + entity.Value);
                entityValue = entity.Value;
            }
            ((MainWindow)Application.Current.MainWindow)._scenariosControl.LogLuis("\n");
            switch (res.TopScoringIntent.Name)
            {
                case "New Quote":
                    if(entityValue.ToLower().Contains("car"))
                    {
                        OpenUri("https://www.suncorp.com.au/insurance/car.html");
                    }
                    else if (entityValue.ToLower().Contains("house"))
                    {
                        OpenUri("https://www.suncorp.com.au/insurance/home/home-and-contents.html");
                    }
                    else if (entityValue.ToLower().Contains("insurance"))
                    {
                        OpenUri("https://www.suncorp.com.au/insurance/");
                    }
                    else
                    {
                        OpenUri("https://www.suncorp.com.au/insurance/");
                    }
                    break;
                case "New Complaint":
                    OpenUri("https://www.suncorp.com.au/insurance/contact-us.html#write");
                    break;
                case "New Incident":
                    OpenUri("https://www.suncorp.com.au/insurance/claims.html#car-insurance");
                    break;
                case "Update Contact Details":
                    OpenUri("https://insurance.suncorp.com.au/usermgmt/public/suncorp/login.jsp?contextType=external&username=string&contextValue=%2Foam&password=sercure_string&challenge_url=https%3A%2F%2Finsurance.suncorp.com.au%2Fusermgmt%2Fpublic%2Fsuncorp%2Flogin.jsp&request_id=-414593964383372649&authn_try_count=0&locale=en_US&resource_url=http%253A%252F%252Finsurance.suncorp.com.au%252Fpss");
                    break;
                default:
                    break;
            }
        }
        public static bool OpenUri(string uri)
        {
            System.Diagnostics.Process.Start(uri);
            return true;
        }

        public class Datasets
        {
            public dataset[] value { get; set; }
        }

        public class dataset
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        public class Tables
        {
            public table[] value { get; set; }
        }

        public class table
        {
            public string Name { get; set; }
        }

        public class Groups
        {
            public group[] value { get; set; }
        }

        public class group
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        private async void Detect_Topics(string text)
        {
            try
            {
                var source = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList();
                var randomText = new RandomText(source);

                var request = new TopicRequest();

                for (int i = 0; i < 100; i++)
                {
                    request.Documents.Add(new TopicDocument() { Id = i.ToString(), Text = randomText.Next() });
                }

                MainWindow mainWindow = Window.GetWindow(this) as MainWindow;
                var client = new TopicClient(mainWindow._scenariosControl.SubscriptionKey);
                var opeationUrl = await client.StartTopicProcessingAsync(request);

                TopicResponse response = null;
                var doneProcessing = false;

                while (!doneProcessing)
                {
                    response = await client.GetTopicResponseAsync(opeationUrl);

                    switch (response.Status)
                    {
                        case TopicOperationStatus.Cancelled:
                            Log("Operation Status: Cancelled");
                            doneProcessing = true;
                            break;
                        case TopicOperationStatus.Failed:
                            Log("Operation Status: Failed");
                            doneProcessing = true;
                            break;
                        case TopicOperationStatus.NotStarted:
                            Log("Operation Status: Not Started");
                            Thread.Sleep(100);
                            break;
                        case TopicOperationStatus.Running:
                            Log("Operation Status: Running");
                            Thread.Sleep(100);
                            break;
                        case TopicOperationStatus.Succeeded:
                            Log("Operation Status: Succeeded");
                            doneProcessing = true;
                            break;
                    }
                }

                foreach (var topic in response.OperationProcessingResult.Topics)
                {
                    var score = topic.Score * 100;
                    Log(string.Format("{0} ({1}%)", topic.KeyPhrase, score));
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }

        /// <summary>
        /// Log message in main window log pane
        /// </summary>
        /// <param name="format">format string</param>
        /// <param name="args">format arguments</param>
        public static void Log(string format, params object[] args)
        {
            ((MainWindow)Application.Current.MainWindow)._scenariosControl.Log(string.Format(format, args));
        }

        #endregion Methods

        #region Nested Types

        public class MainViewModel : INotifyPropertyChanged
        {
            #region Events

            /// <summary>
            /// Implements INotifyPropertyChanged interface
            /// </summary>
            /// 
            public event PropertyChangedEventHandler PropertyChanged;

            #endregion Events

            #region Properties

           

            public string LanguageIdentificationDescription
            {
                get;
                set;
            }

            public string SentimentAnalysisDescription
            {
                get;
                set;
            }

            public string KeyPhraseDetectionDescription
            {
                get;
                set;
            }

            public string TopicDetectionDescription
            {
                get;
                set;
            }

            #endregion Properties

            #region Methods

            /// <summary>
            /// Helper function for INotifyPropertyChanged interface 
            /// </summary>
            /// <typeparam name="T">Property type</typeparam>
            /// <param name="caller">Property name</param>
            private void OnPropertyChanged<T>([CallerMemberName]string caller = null)
            {
                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(caller));
                }
            }

            #endregion Methods
        }

        #endregion Nested Types

        private void clearHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteRows(keywordDatasetName);
            DeleteRows(sentimentDatasetName);

            ttsMessage = "";
            lastMessage = "";
            lastNMessages.Clear();
            keyPhrasesString = "";
            keyPhrasesDict = new Dictionary<string, int>();
            ((MainWindow)Application.Current.MainWindow)._scenariosControl.ClearLog();
        }

        private void testButton_Click(object sender, RoutedEventArgs e)
        {
            if (loopTestCheckbox.IsChecked == true)
            {
                if (!dispatcherTimer.IsEnabled)
                {
                    dispatcherTimer.Start();
                    testButton.Content = "Stop Timer";

                }
                else
                {
                    dispatcherTimer.Stop();
                    testButton.Content = "Test";

                }
            }
            else
            {
                test();
            }
            //dispatcherTimer.Start();
        }

        private void test()
        {
            
                if (testIterator < testTextArray.Count())
                {
                    Clipboard.SetText(testTextArray[testIterator].Trim() + ". ");
                    testIterator++;
                }
                else
                {
                    testIterator = 0;
                }


        }


    }
}
