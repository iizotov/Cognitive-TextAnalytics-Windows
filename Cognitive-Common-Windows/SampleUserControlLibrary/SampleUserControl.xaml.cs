// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Cognitive Services (formerly Project Oxford): https://www.microsoft.com/cognitive-services
// 
// Microsoft Cognitive Services (formerly Project Oxford) GitHub:
// https://github.com/Microsoft/Cognitive-Common-Windows
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace SampleUserControlLibrary
{
    public class Scenario
    {
        public string Title
        {
            set;
            get;
        }

        public Type PageClass
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class SampleScenarios : UserControl
    {
        public static DependencyProperty SampleTitleProperty =
            DependencyProperty.Register("SampleTitle", typeof(string), typeof(SampleScenarios));

        public static DependencyProperty SampleScenarioListProperty =
            DependencyProperty.Register("SampleScenarioList", typeof(Scenario[]), typeof(SampleScenarios));

        private SubscriptionKeyPage _subscriptionPage;

        public string SampleTitle
        {
            get { return (string)GetValue(SampleTitleProperty); }
            set { SetValue(SampleTitleProperty, value); }
        }

        public Scenario[] SampleScenarioList
        {
            get { return (Scenario[])GetValue(SampleScenarioListProperty); }
            set
            {
                SetValue(SampleScenarioListProperty, value);
                _scenarioListBox.ItemsSource = SampleScenarioList;
            }
        }

        /// <summary>
        /// Gets or sets the disclaimer
        /// </summary>
        public string Disclaimer
        {
            get { return _disclaimerTextBlock.Text; }
            set { _disclaimerTextBlock.Text = value; }
        }

        public string SubscriptionKey
        {
            get;
            set;
        }

        public SampleScenarios()
        {
            InitializeComponent();
            _subscriptionPage = new SubscriptionKeyPage(this);
            SubscriptionKey = _subscriptionPage.SubscriptionKey;

            SampleTitle = "Replace SampleNames with SampleScenarios.SampleTitle property";

            SampleScenarioList = new Scenario[]
            {
                new Scenario { Title = "Scenario 1: Replace items using SampleScenarios.ScenarioList" },
                new Scenario { Title = "Scenario 2: Replace items using SampleScenarios.ScenarioList" }
            };

            _scenarioListBox.ItemsSource = SampleScenarioList;

            _scenarioFrame.Navigate(_subscriptionPage);
        }

        public void Log(string logMessage)
        {
            if (String.IsNullOrEmpty(logMessage) || logMessage == "\n")
            {
                _logTextBox.Text += "\n";
                
            }
            else
            {
                string timeStr = DateTime.Now.ToString("HH:mm:ss.ffffff");
                string messaage = "[" + timeStr + "]: " + logMessage + "\n";
                _logTextBox.Text += messaage;
            }
            _logTextBox.ScrollToEnd();
        }
        

        public void LogTTS(string logMessage)
        {
            if (String.IsNullOrEmpty(logMessage) || logMessage == "\n")
            {
                _ttsTextBox.AppendText("\n");

            }
            else
            {
                _ttsTextBox.AppendText(logMessage + " ");
            }
            _ttsTextBox.ScrollToEnd();
            //http://stackoverflow.com/questions/8772308/method-to-search-through-a-richtextbox-and-highlight-all-instances-of-that-speci
        }

        public void LogLuis(string logMessage)
        {
            if (String.IsNullOrEmpty(logMessage) || logMessage == "\n")
            {
                _luisTextBox.AppendText("\n");

            }
            else
            {
                _luisTextBox.AppendText(logMessage + " ");
            }
            _luisTextBox.ScrollToEnd();
            //http://stackoverflow.com/questions/8772308/method-to-search-through-a-richtextbox-and-highlight-all-instances-of-that-speci
        }
        public void LogLuis2(string logMessage)
        {
            if (String.IsNullOrEmpty(logMessage) || logMessage == "\n")
            {
                _luis2TextBox.AppendText("\n");

            }
            else
            {
                _luis2TextBox.AppendText(logMessage + " ");
            }
            _luis2TextBox.ScrollToEnd();
            //http://stackoverflow.com/questions/8772308/method-to-search-through-a-richtextbox-and-highlight-all-instances-of-that-speci
        }

        private List<TextRange> FindWordFromPosition(TextPointer position, string word)
        {
            List <TextRange> outVar = new List<TextRange>();
            while (position != null)
            {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string textRun = position.GetTextInRun(LogicalDirection.Forward);

                    // Find the starting index of any substring that matches "word".
                    //int indexInRun = textRun.IndexOf(word);
                    MatchCollection matches = Regex.Matches(textRun, word);
                    foreach (Match match in matches)
                    {
                        if (match.Index >= 0)
                        {
                            TextPointer start = position.GetPositionAtOffset(match.Index);
                            TextPointer end = start.GetPositionAtOffset(word.Length);
                            outVar.Add(new TextRange(start, end));
                            //return new TextRange(start, end);
                        }
                    }
                }

                position = position.GetNextContextPosition(LogicalDirection.Forward);
            }

            // position will be null if "word" is not found.
            return outVar;
        }

        public void HighlightWords(List<string> words)
        {
            TextPointer text = _ttsTextBox.Document.ContentStart;
            while (text.GetPointerContext(LogicalDirection.Forward) != TextPointerContext.Text)
            {
                text = text.GetNextContextPosition(LogicalDirection.Forward);


                foreach (string word in words)
                {
                    //MatchCollection matches = Regex.Matches(new TextRange(text.DocumentStart, _ttsTextBox.Document.ContentEnd).Text, word);
                    //foreach (Match match in matches)
                    {


                        //var start = _ttsTextBox.Document.ContentStart;
                        //var startPos = text.GetPositionAtOffset(match.Index);
                        //var endPos = text.GetPositionAtOffset(match.Index + match.Length);
                        //var textRange = new TextRange(startPos, endPos);
                        foreach (TextRange textRange in FindWordFromPosition(text, word))
                        {
                            textRange.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.Yellow));
                            textRange.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                        }

                    }
                }

            }

            //Apply color to all matching text
            /*foreach (string word in words)
            {
                MatchCollection matches = Regex.Matches(new TextRange(_ttsTextBox.Document.ContentStart, _ttsTextBox.Document.ContentEnd).Text, word);
                foreach (Match match in matches)
                {
                    
                    
                    var start = _ttsTextBox.Document.ContentStart;
                    var startPos = start.GetPositionAtOffset(match.Index + 2);
                    var endPos = start.GetPositionAtOffset(match.Index + 2 + match.Length);
                    var textRange = new TextRange(startPos, endPos);
                    textRange.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.Yellow));
                    textRange.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                    
                }
            }*/
        }

        public void LogSentiment(string logMessage)
        {
            if (String.IsNullOrEmpty(logMessage) || logMessage == "\n")
            {
                _sentimentTextBox.Text += "\n";

            }
            else
            {
                _sentimentTextBox.Text = logMessage;
            }
            _sentimentTextBox.ScrollToEnd();
        }

        public void LogKeywords(string logMessage)
        {
            if (String.IsNullOrEmpty(logMessage) || logMessage == "\n")
            {
                _keyWordsTextBox.Text += "\n";

            }
            else
            {
                _keyWordsTextBox.Text = logMessage;
            }
            _keyWordsTextBox.ScrollToEnd();
        }

        

        public void ClearLog()
        {
            _logTextBox.Text = "";
            _keyWordsTextBox.Text = "";
            _sentimentTextBox.Text = "";
            _ttsTextBox.Document.Blocks.Clear();
            _luisTextBox.Text = "";
            _luis2TextBox.Text = "";
        }

        private void ScenarioChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox scenarioListBox = sender as ListBox;
            Scenario scenario = scenarioListBox.SelectedItem as Scenario;
            ClearLog();

            if (scenario != null)
            {
                Page page = Activator.CreateInstance(scenario.PageClass) as Page;
                page.DataContext = this.DataContext;
                _scenarioFrame.Navigate(page);
            }
        }

        private void SubscriptionManagementButton_Click(object sender, RoutedEventArgs e)
        {
            _scenarioFrame.Navigate(_subscriptionPage);
            // Reset the selection so that we can get SelectionChangedEvent later.
            _scenarioListBox.SelectedIndex = -1;
        }
    }

    public class ScenarioBindingConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Scenario s = value as Scenario;
            if (s != null)
            {
                return s.Title;
            }
            return null;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
