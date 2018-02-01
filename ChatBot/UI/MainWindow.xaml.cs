using System;
using System.Collections.Generic;
using System.Windows;
using System.ComponentModel;

namespace HD
{
  public partial class MainWindow : Window
  {
    public static MainWindow instance;

    #region Init
    public MainWindow()
    {
      instance = this;
      InitializeComponent();

      new KeyPressMonitor();
    }

    async void Window_Loaded(
      object sender,
      RoutedEventArgs e)
    {
      CheckBotSettings();
      await BotLogic.instance.Start();
      TwitchController.instance.onChannelInfoChange += OnTitleChange;
      TitleText.Text = (await TwitchController.instance.GetChannelInfo()).title;
    }
    #endregion

    #region Events
    void OnTitleChange(
      string title,
      string game)
    {
      Dispatcher.Invoke(() =>
      {
        TitleText.Text = title;
      });
    }

    private void CheckBotSettings()
    {
      Hide();
      if (BotSettings.IsConfigured == false)
      {
        MessageBox.Show("Your twitch settings are not configured properly. Please setup.",
          "ChatBot",
          MessageBoxButton.OK,
          MessageBoxImage.Exclamation);

        if (new SettingsWindow().ShowDialog() == false)
          Environment.Exit(0);
      }
      Show();
    }

    void Window_Closing(
      object sender,
      CancelEventArgs e)
    {
      BotLogic.instance.Stop();
    }
    #endregion

    int iHistory;
    readonly List<string> historyList = new List<string>();

    private void Message_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
      // TODO why is this here
    }

    private void Message_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
      if (e.Key == System.Windows.Input.Key.Enter)
      {
        string message = Message.Text;
        TwitchController.instance.InjectFakeMessage(new Message(
          TwitchController.instance.twitchChannel,
          message,
          true,
          0));
        historyList.Add(message);
        iHistory = historyList.Count;
        Message.Clear();
      }
      else
      {

        if (e.Key == System.Windows.Input.Key.Up)
        {
          iHistory--;
          if (iHistory < 0)
          {
            iHistory = 0;
          }
          if (iHistory >= historyList.Count)
          {
            Message.Clear();
          }
          else
          {
            Message.Text = historyList[iHistory];
          }
        }
        else if (e.Key == System.Windows.Input.Key.Down)
        {
          iHistory++;
          if (iHistory > historyList.Count)
          {
            iHistory = historyList.Count;
          }

          if (iHistory < 0
            || iHistory >= historyList.Count)
          {
            Message.Clear();
          }
          else
          {
            Message.Text = historyList[iHistory];
          }
        }

      }
    }

    async void Title_Loaded(
      object sender,
      RoutedEventArgs e)
    {
    }

    void Title_LostFocus(
      object sender,
      RoutedEventArgs e)
    {
      UpdateTitle();
    }

    void OpenSettings_OnClick(object sender, RoutedEventArgs e)
    {
      if (new SettingsWindow().ShowDialog() == false)
      {
        Environment.Exit(0);
      }
    }

    void Window_LostFocus(
      object sender,
      RoutedEventArgs e)
    {
      UpdateTitle();
    }

    void UpdateTitle()
    {
      TwitchController.instance.SetTitle(TitleText.Text);
    }
  }
}
