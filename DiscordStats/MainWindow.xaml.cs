using DiscordStats.Data;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Net.WebSocket;
using Newtonsoft.Json.Linq;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
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

namespace DiscordStats
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DiscordClient _client;
        private HttpClient _httpClient;

        public MainWindow()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            DataContext = new StatsConfig();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            string password = tokenTextBox.Password.Trim().Trim('"');
            if (!string.IsNullOrWhiteSpace(password))
            {
                _client = new DiscordClient(new DiscordConfiguration()
                {
                    Token = password,
                    TokenType = TokenType.User,
                    LogLevel = LogLevel.Debug
                });

                if (Environment.OSVersion.Version < new Version(6, 2))
                {
                    _client.SetWebSocketClient<WebSocket4NetCoreClient>();
                }

                _client.DebugLogger.LogMessageReceived += (s, ev) => Debug.WriteLine(ev);

                _httpClient.DefaultRequestHeaders.Add("authorization", password);
                _client.Ready += _client_Ready;
                try
                {
                    await _client.ConnectAsync();
                }
                catch
                {
                    TaskDialog dialog = new TaskDialog()
                    {
                        MainIcon = TaskDialogIcon.Error,
                        Content = "Real token plz thx"
                    };
                    dialog.Buttons.Add(new TaskDialogButton(ButtonType.Ok));
                    dialog.ShowDialog();
                    IsEnabled = true;
                }
            }
            else
            {
                TaskDialog dialog = new TaskDialog()
                {
                    MainIcon = TaskDialogIcon.Error,
                    Content = "Okay fuck off actually thanks",
                    WindowTitle = "Jesus Christ"
                };
                dialog.Buttons.Add(new TaskDialogButton(ButtonType.Ok));
                dialog.ShowDialog();
                IsEnabled = true;
            }
        }

        private async Task _client_Ready(DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                IsEnabled = true;
                loginGrid.Visibility = Visibility.Collapsed;
                mainGrid.Visibility = Visibility.Visible;
                guildsComboBox.ItemsSource = _client.Guilds.Values.OrderBy(g => g.Name);
            });

            foreach (DiscordGuild guild in _client.Guilds.Values)
            {
                Debug.WriteLine($"{guild.Name} - {guild.SplashUrl}");
            }
        }

        private void guildsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (guildsComboBox.SelectedItem != null)
            {
                DiscordGuild guild = (guildsComboBox.SelectedItem as DiscordGuild);
                DiscordMember currentMember = guild.CurrentMember;
                configureGrid.Visibility = Visibility.Visible;
                channelsBox.ItemsSource = guild.Channels
                    .Where(c => c.PermissionsFor(currentMember).HasFlag(Permissions.AccessChannels))
                    .Where(c => c.Type == ChannelType.Text)
                    .OrderBy(c => c.Position);
            }
            else
            {
                configureGrid.Visibility = Visibility.Collapsed;
            }
        }

        private async void run_Click(object sender, RoutedEventArgs e)
        {
            StatsConfig conf = DataContext as StatsConfig;
            guildName.Text = conf.Guild.Name;
            preRunContent.Visibility = Visibility.Collapsed;
            runContent.Visibility = Visibility.Visible;
            GuildStats stats = new GuildStats(conf.Guild);

            if (conf.UserMentionCounts == true || conf.UserMessageCounts == true)
            {
                await SetStatus("Retrieving members... (This may take a while)");
                var members = (await conf.Guild.GetAllMembersAsync()).OrderBy(m => m.Username).ToList();

                for (int i = 0; i < members.Count; i++)
                {
                    var m = members.ElementAt(i);
                    await SetStatus($"Searching for @{m.Username}#{m.Discriminator} ({i + 1}/{members.Count})", i, members.Count);

                    var mstats = new MemberStats(m);

                    if (conf.UserMessageCounts == true)
                    {
                        try
                        {
                            string sentResult = await _httpClient.GetStringAsync($"https://discordapp.com/api/v7/guilds/{conf.Guild.Id}/messages/search?author_id={m.Id}&include_nsfw=true");
                            mstats.SentMessages = GetCount(sentResult);
                            var array = JObject.Parse(sentResult)["messages"].FirstOrDefault();

                            if (array != null)
                            {
                                var message = array.ElementAtOrDefault(1) ?? array.ElementAtOrDefault(0);
                                if (message != null)
                                {
                                    mstats.LastMessage = message["timestamp"].ToObject<DateTimeOffset>().ToUniversalTime();
                                }
                            }

                            if (conf.BanMeDaddy != false)
                            {
                                await Task.Delay(100);
                            }
                        }
                        catch (Exception ex)
                        {
                            HandleError(ex);
                            continue;
                        }
                    }

                    if (conf.UserMentionCounts == true)
                    {
                        try
                        {
                            string sentResult = await _httpClient.GetStringAsync($"https://discordapp.com/api/v7/guilds/{conf.Guild.Id}/messages/search?mentions={m.Id}&include_nsfw=true");
                            mstats.Mentions = GetCount(sentResult);

                            if (conf.BanMeDaddy != false)
                            {
                                await Task.Delay(100);
                            }
                        }
                        catch (Exception ex)
                        {
                            HandleError(ex);
                            continue;
                        }
                    }

                    stats.MemberStats.Add(mstats);
                }
            }

            if (conf.ChannelMessageCounts == true)
            {
                DiscordMember currentMember = conf.Guild.CurrentMember;
                configureGrid.Visibility = Visibility.Visible;
                var channels = conf.Guild.Channels
                    .Where(c => c.PermissionsFor(currentMember).HasFlag(Permissions.AccessChannels))
                    .Where(c => c.Type == ChannelType.Text)
                    .OrderBy(c => c.Position).ToList();

                for (int i = 0; i < channels.Count; i++)
                {
                    DiscordChannel c = channels.ElementAt(i);
                    await SetStatus($"Getting message count for #{c.Name} ({i + 1}/{channels.Count})", i, channels.Count);

                    ChannelStats channelStat = new ChannelStats(c);

                    string result = await _httpClient.GetStringAsync($"https://discordapp.com/api/v7/guilds/{conf.Guild.Id}/messages/search?channel_id={c.Id}&include_nsfw=true");
                    channelStat.Messages = GetCount(result);
                    stats.ChannelStats.Add(channelStat);

                    if (conf.BanMeDaddy != false)
                    {
                        await Task.Delay(100);
                    }
                }
            }

            await Dispatcher.InvokeAsync(() =>
            {
                runContent.Visibility = Visibility.Collapsed;
                resultsContent.Visibility = Visibility.Visible;
                resultsContent.Navigate(new ResultsPage(stats));
            });
        }

        private static void HandleError(Exception ex)
        {
            ShowTaskDialog($"Oops, something fucked up while working grabbing data. An {ex.GetType().Name} occured ({ex.Message})", TaskDialogIcon.Error);
        }

        private static int GetCount(string sentResult)
        {
            JObject tempObject = JObject.Parse(sentResult);
            return tempObject["total_results"].ToObject<int>();
        }

        private async Task SetStatus(string text, int? value = null, int? max = null)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                statusText.Text = text;
                progressBar.Maximum = max ?? progressBar.Maximum;
                if (value == null)
                {
                    progressBar.IsIndeterminate = true;
                }
                else
                {
                    progressBar.IsIndeterminate = false;
                    progressBar.Value = value.Value;
                }
            });
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ShowTaskDialog("Hey this probably isn't a good idea and will likely get you banned from Discord! Don't be a twat.", TaskDialogIcon.Warning);
        }

        public static void ShowTaskDialog(string content, TaskDialogIcon icon)
        {
            TaskDialog dialog = new TaskDialog()
            {
                Content = content,
                MainIcon = icon,
                WindowTitle = "Discord Statistics"
            };
            dialog.Buttons.Add(new TaskDialogButton(ButtonType.Ok));
            dialog.ShowDialog();
        }
    }

}
