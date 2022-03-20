using FunY.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace FunY
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainPage_Loaded;

            ApplicationView.GetForCurrentView().TryResizeView(new Size(500, 360));

            NavigationCacheMode = NavigationCacheMode.Required;

            var appViewTitleBar = ApplicationView.GetForCurrentView().TitleBar;

            appViewTitleBar.ButtonBackgroundColor = Colors.Transparent;
            appViewTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            UpdateTitleBarLayout(coreTitleBar);

            Window.Current.SetTitleBar(AppTitleBar);

            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;
            coreTitleBar.IsVisibleChanged += CoreTitleBar_IsVisibleChanged;
        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            UpdateTitleBarLayout(sender);
        }

        private void CoreTitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args)
        {
            AppTitleBar.Visibility = sender.IsVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateTitleBarLayout(CoreApplicationViewTitleBar coreTitleBar)
        {
            // Update title bar control size as needed to account for system size changes.
            AppTitleBar.Height = coreTitleBar.Height;

            // Ensure the custom title bar does not overlap window caption controls
            Thickness currMargin = AppTitleBar.Margin;
            AppTitleBar.Margin = new Thickness(currMargin.Left, currMargin.Top, coreTitleBar.SystemOverlayRightInset, currMargin.Bottom);
        }

        private async void FetchRandomButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using var client = new HttpClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get
                };
                bool isLocal = false;

                switch (App.SViewModel.Source)
                {
                    case 0:
                        isLocal = true;
                        break;
                    case 1:
                        request.RequestUri = new Uri("https://free-quotes-api.herokuapp.com");
                        break;
                    case 2:
                        request.RequestUri = new Uri("https://v2.jokeapi.dev/joke/Any?blacklistFlags=nsfw,religious,political,racist,sexist,explicit");
                        break;
                }

                if (!isLocal)
                {
                    using var response = await client.SendAsync(request);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        if (App.SViewModel.Source == 1)
                        {
                            Fact fact = JsonConvert.DeserializeObject<Fact>(await response.Content.ReadAsStringAsync());

                            CurrentFactText.Text = string.IsNullOrWhiteSpace(fact.Author) ? fact.FactDesc : $"{fact.FactDesc}\n\nAuthor: {fact.Author}";
                        }
                        else if (App.SViewModel.Source == 2)
                        {
                            JokeAPIFact joke = JsonConvert.DeserializeObject<JokeAPIFact>(await response.Content.ReadAsStringAsync());

                            CurrentFactText.Text = string.IsNullOrWhiteSpace(joke.Joke) ? $"{joke.JokeStart} {joke.JokeAns}" : joke.Joke;
                        }
                    }
                }
            } catch
            {

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }
    }
}
