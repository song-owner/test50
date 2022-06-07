using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using problemSolver.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace problemSolver
{
    public partial class ResultPage : ContentPage
    {
        double sliderValue = 0.5;
        List<Problems> problemList = new List<Problems>();
        FirebaseHelper firebaseHelper = new FirebaseHelper();
        WebView webView = new WebView();
        static int listRow = 0;
        Problems lastProblems = new Problems();
        string lastItemTappedUrl = "";
        public ResultPage(string finedWord)
        {
            InitializeComponent();
            Device.StartTimer(TimeSpan.FromSeconds(0.2), () =>
            {
                seartchWord.Text = finedWord;
                return false;
            });
            lastProblems.SearchWord = finedWord;
            webView.Source = "https://www.google.com/search?q=" + finedWord;
            webView.VerticalOptions = LayoutOptions.FillAndExpand;
            browser.Children.Add(webView);
            webView.Navigated += new EventHandler<Xamarin.Forms.WebNavigatedEventArgs>(OnNavigated);
            string uuid = "";
            if (DependencyService.Get<IDeviceService>() != null)
            {
                uuid = DependencyService.Get<IDeviceService>().GetUuid();
            }
            lastProblems.PhoneId = uuid;
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            refreshList();
        }
        async void refreshList()
        {
            var allProblems = await firebaseHelper.GetAllProblems();
            if (allProblems == null) return;
            var sameWord = new List<string>();
            var sameWordLast = new List<string>();
            if (seartchWord.Text != "" && seartchWord.Text != null && allProblems.Count > 0)
            {
                //問題リストに複数ワード
                var problems = new List<testClass>();
                int number = 0;
                foreach (var tmp in allProblems)
                {
                    if (tmp.SearchWord == null) continue;
                    problems.Add(new testClass() { abcd = tmp.SearchWord.Split()[0], num = number });
                    number++;
                }
                //検索文字列に複数ワード
                int splitNo = 1;
                foreach (string seartchs in seartchWord.Text.Split())
                {
                    sameWord = seartchs.SortByDistance(problems).ToList();
                    sameWord = sameWord.Distinct().ToList();
                    int num = 0;
                    foreach (string result in sameWord)
                    {
                        if (sameWordLast.Count <= num) sameWordLast.Add(result);
                        else sameWordLast.Insert(num, result);
                        num += splitNo;                        
                    }
                    splitNo++;
                    //sameWord.AddRange(seartchs.SortByDistance(problems).ToList());
                    //sameWord = sameWord.Distinct().ToList();
                }
                //sameWord = seartchWord.Text.SortByDistance(problems).ToList();
                sameWordLast = sameWordLast.Distinct().ToList();
                int count = 0;
                foreach (var tmp in sameWordLast)
                {
                    var problems2 = allProblems.
                        Where(a => a.SearchWord != null && a.SearchWord.IndexOf(tmp) >= 0).ToList();
                    if (count == 0) problemList = problems2; else problemList.InsertRange(problemList.Count, problems2);
                    if (count == 7) break;
                    count++;
                }
            }
            else
            {
                problemList = allProblems;
            }
            foreach (var tmp in problemList)
            {
                if (tmp.PhoneId == lastProblems.PhoneId)
                {
                    tmp.Del = true;
                    tmp.DelWidth1 = new GridLength(0.2, GridUnitType.Star);
                    tmp.DelWidth2 = new GridLength(0.2, GridUnitType.Star);
                }
                else
                {
                    tmp.Del = false;
                    tmp.DelWidth1 = new GridLength(0.4, GridUnitType.Star);
                    tmp.DelWidth2 = new GridLength(0, GridUnitType.Star);
                }
            }
            //problemList.Sort((a, b) => int.Parse(b.Nice) - int.Parse(a.Nice));
            problemListView.ItemsSource = problemList;
        }
        private async void itemTapped(object sender, ItemTappedEventArgs e)
        {
            listRow = e.ItemIndex;
            if (listRow < 0)
            {
                return;
            }
            var now = problemList[listRow];
            if (now.Url == null || now.Url.IndexOf("http") == -1)
            {
                return;
            }
            seartchWord.Text = now.SearchWord;
            if (lastProblems.SearchWord != now.SearchWord/* || lastItemTappedUrl != lastProblems.Url*/)
                await solvedResult();
            webView.Source = now.Url;
            lastProblems.SearchWord = now.SearchWord;
            lastItemTappedUrl = now.Url;
        }
        private async void deleteTaped(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            string key = button.CommandParameter.ToString();
            await firebaseHelper.DeleteProblem(key);
            refreshList();
        }
        private async void startSearch(object sender, EventArgs e)
        {
            var url = "https://www.google.com/search?q=" + seartchWord.Text;
            if (seartchWord.Text.IndexOf("http") >= 0) url = seartchWord.Text;
            if (seartchWord.Text != lastProblems.SearchWord) await solvedResult();
            webView.Source = url;
            refreshList();
        }
        private async void saveUrl(object sender, EventArgs e)
        {
            await solvedResult();
        }
        async Task<bool> solvedResult()
        {
            var result = await DisplayAlert("Did the search word \"" + lastProblems.SearchWord + "\" resolve on the last searched page?", "", "Yes", "No");
            if (result == true)
            {
                var sameUrl = await firebaseHelper.GetProblem(lastProblems.Url);
                if (sameUrl == null)
                {
                    lastProblems.Nice = "1";
                    await firebaseHelper.AddProblem(
                        lastProblems.PhoneId, lastProblems.SearchWord, lastProblems.HtmlTitle, lastProblems.Url, lastProblems.Nice);
                }
                else
                {
                    var nice = 0;
                    if (sameUrl.Nice != null) nice = Int32.Parse(sameUrl.Nice);
                    nice++;
                    await firebaseHelper.UpdatePerson(sameUrl.Key, lastProblems.PhoneId, lastProblems.SearchWord, lastProblems.HtmlTitle, lastProblems.Url, nice.ToString());
                }
            }
            refreshList();
            lastProblems.SearchWord = seartchWord.Text;
            return true;
        }
        private async void OnNavigated(object sender, WebNavigatedEventArgs e)
        {
            var url = e.Url;
            //testUrl.Text = url;
            var title = await AsyncWork2(url);
            lastProblems.HtmlTitle = title;
            lastProblems.Url = url;
        }
        private void resultPageChange(object sender, PropertyChangedEventArgs e)
        {
            if (resultPage.Width < 0) return;
            slider.Value = sliderValue;
            AppPge.Width = resultPage.Width * sliderValue;
            GooglePage.Width = GridLength.Star;
        }
        static async Task<string> AsyncWork2(string urlstring)
        {
            var client = new HttpClient();
            var parser = new HtmlParser();
            var doc = default(IHtmlDocument);
            try
            {
                var stream = await client.GetStreamAsync(new Uri(urlstring));
                doc = await parser.ParseDocumentAsync(stream);
                var title = doc.GetElementsByTagName("title")[0].InnerHtml;
                return title;
            }
            catch { }
            return "";
        }
        private async void closeClick(object sender, EventArgs e)
        {
            if (seartchWord.Text != lastProblems.SearchWord) await solvedResult();
            DependencyService.Get<IDeviceService>().Exit();
        }

        private void backClick(object sender, EventArgs e)
        {
            if (webView.CanGoBack) webView.GoBack();
        }
    }
}
