using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace Autoweb.Mobile
{
    public partial class MainPage : ContentPage
    {
        private HubConnection _hubConnection;
        private Entry entCarNumber;
        private Label lblLiveStatus;

        public MainPage()
        {
            BackgroundColor = Color.FromArgb("#F5F6FA");

            // 1. Dynamic UI Controls Creation (Safe Initialization)
            var titleLabel = new Label
            {
                Text = "AutoWeg Live Tracker",
                FontSize = 26,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#2C3E50"),
                HorizontalOptions = LayoutOptions.Center
            };

            var subtitleLabel = new Label
            {
                Text = "Apni car ki live servicing track karein",
                FontSize = 14,
                TextColor = Color.FromArgb("#7F8C8D"),
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, -10, 0, 10)
            };

            entCarNumber = new Entry
            {
                Placeholder = "e.g. MP10-1234",
                FontSize = 16,
                TextColor = Color.FromArgb("#2C3E50"),
                BackgroundColor = Colors.White,
                HeightRequest = 50
            };

            lblLiveStatus = new Label
            {
                Text = "Server se connect ho raha hai...",
                FontSize = 22,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#2980B9"),
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
            };

            var statusFrame = new Frame
            {
                BackgroundColor = Colors.White,
                BorderColor = Color.FromArgb("#E2E8F0"),
                Padding = new Thickness(25),
                CornerRadius = 15,
                HasShadow = true,
                Content = new VerticalStackLayout
                {
                    Spacing = 10,
                    Children = {
                        new Label { Text = "CURRENT STATUS", FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#95A5A6"), HorizontalOptions = LayoutOptions.Center },
                        lblLiveStatus
                    }
                }
            };

            Content = new ScrollView
            {
                Content = new VerticalStackLayout
                {
                    Padding = new Thickness(30),
                    Spacing = 25,
                    VerticalOptions = LayoutOptions.Center,
                    Children = { titleLabel, subtitleLabel, entCarNumber, statusFrame }
                }
            };

            // 2. Crash-Proof SignalR Setup (Aapka HTTPS port 7208 set kiya hai)
            string hubUrl = "http://10.0.2";
            //string hubUrl = "https://ngrok-free.app";

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options => {
                    // Android certificate errors bypass handler
#if ANDROID
                    var handler = new System.Net.Http.HttpClientHandler();
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                    options.HttpMessageHandlerFactory = h => handler;
#endif
                })
                .WithAutomaticReconnect()
                .Build();

            // 3. Safe Real-time Event Listener with Crash Protection Check
            _hubConnection.On<string, string>("ReceiveStatusUpdate", (carNumber, newStatus) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        // Strict null reference validation check
                        if (entCarNumber != null && entCarNumber.Text != null && lblLiveStatus != null)
                        {
                            if (entCarNumber.Text.Trim().Equals(carNumber, StringComparison.OrdinalIgnoreCase))
                            {
                                lblLiveStatus.Text = newStatus;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"UI Update Exception: {ex.Message}");
                    }
                });
            });

            StartSignalRConnection();
        }

        private async void StartSignalRConnection()
        {
            try
            {
                if (_hubConnection != null)
                {
                    await _hubConnection.StartAsync();

                    if (lblLiveStatus != null)
                        lblLiveStatus.Text = "Connected! Waiting for updates...";
                }
            }
            catch (Exception ex)
            {
                if (lblLiveStatus != null)
                    lblLiveStatus.Text = "Connection Failed. Trying again...";

                System.Diagnostics.Debug.WriteLine($"SignalR Connection Error: {ex.Message}");
            }
        }
    }
}
