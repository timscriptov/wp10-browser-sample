using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.Web.Http;
using System.Net;
using Windows.Storage;
using Windows.Data.Xml.Dom;

namespace Browser
{
    public sealed partial class MainPage : Page
    {
        private SettingsHelper settingsHelper;

        private string desktopUserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36 uacq";
        private string mobileUserAgent = "Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Mobile Safari/537.36";

        private string googleHomeUrl = "https://www.google.com";
        private string googleSearchUrl = "https://www.google.com/search?q=";

        private ContentDialog dialog;

        public MainPage()
        {
            this.InitializeComponent();
            settingsHelper = new SettingsHelper();
        }

        private void backBtn_Click(object sender, RoutedEventArgs e)
        {
            if (webView.CanGoBack)
            {
                webView.GoBack();
            }
        }

        private async void WebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Обновляем текстовое поле с актуальной ссылкой открытой страницы
                string source = webView.Source?.ToString();
                if (source != null)
                {
                    urlTextBox.Text = source;
                }
            });
        }

        private async void Home_Click(object sender, RoutedEventArgs e)
        {
            NavigateWithHeader(googleHomeUrl);
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (dialog != null)
            {
                dialog.Hide();
            }
            RefreshWeb();
        }

        private void RefreshWeb()
        {
            NavigateWithHeader(urlTextBox.Text.ToString());
        }

        private async void Menu_Click(object sender, RoutedEventArgs e)
        {
            bool isDarkModeEnabled = settingsHelper.GetSetting("isDarkModeEnabled");
            Button refresh = new Button
            {
                Content = "Refresh",
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            refresh.Click += Refresh_Click;
            CheckBox desktopModeCheckBox = new CheckBox
            {
                Content = "Desktop mode",
                IsChecked = isDarkModeEnabled // Загружаем сохраненное значение
            };

            StackPanel stackPanel = new StackPanel();
            stackPanel.Children.Add(refresh);
            stackPanel.Children.Add(desktopModeCheckBox);
            

            dialog = new ContentDialog
            {
                Title = "Desktop Mode Menu",
                Content = stackPanel,
                PrimaryButtonText = "OK"
            };

            ContentDialogResult result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Сохраняем значение флага при закрытии диалогового окна
                if(desktopModeCheckBox!=null)
                {
                    settingsHelper.AddOrUpdateSetting("isDarkModeEnabled", desktopModeCheckBox.IsChecked);
                    RefreshWeb();
                }
            }
        }

        private async void urlTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                string searchTerm = urlTextBox.Text;
                NavigateWithHeader(searchTerm);
            }
        }

        private async void NavigateWithHeader(string url)
        {
            string searchUrl = await ParseUrl(url);
            urlTextBox.Text = searchUrl;
            var response = await LoadUrlAsync(searchUrl);

            string responseData = await response.Content.ReadAsStringAsync();
            webView.NavigateToString(responseData);
        }

        private async Task<HttpResponseMessage> LoadUrlAsync(string url)
        {
            var httpClient = new HttpClient();
            var requestMsg = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
            requestMsg.Headers.Add("User-Agent", GetUserAgent());
            
            var response = await httpClient.SendRequestAsync(requestMsg);
            return response;
        }

        

        private async Task<string> ParseUrl(string url)
        {
            if (url == "")
            {
                return googleHomeUrl;
            }
            if (url == "about:/search")
            {
                return url.Replace("about:/search", googleSearchUrl);
            }
            string searchUrl = url;
            if (!searchUrl.StartsWith("http://") && !searchUrl.StartsWith("https://"))
            {
                string newUrl = "https://" + url;
                bool valid = await IsValidUrl(newUrl);
                if (!valid)
                {
                    searchUrl = googleSearchUrl + Uri.EscapeDataString(url);
                }
            }
            return searchUrl;
        }

        private async Task<bool> IsValidUrl(string url)
        {
            string searchUrl = url;
            try
            {
                WebRequest webRequest = WebRequest.Create(url);
                WebResponse webResponse = await webRequest.GetResponseAsync();
                return true;
            }
            catch (Exception ex)
            {
            }
            return false;
        }

        private string GetUserAgent()
        {
            string userAgent;
            bool isDarkModeEnabled = settingsHelper.GetSetting("isDarkModeEnabled");
            if (isDarkModeEnabled)
            {
                userAgent = desktopUserAgent;
            }
            else
            {
                userAgent = mobileUserAgent;
            }
            return userAgent;
        }
    }
}
