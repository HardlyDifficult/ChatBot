using System;
using System.Collections.Generic;
using System.Windows;
using System.ComponentModel;

namespace HD
{
  public partial class MainWindow : Window
  {
    #region Init
    public MainWindow()
    {
      InitializeComponent();

      new KeyPressMonitor();
    }
    #endregion

    #region Events
    void Window_Loaded(
      object sender,
      RoutedEventArgs e)
    {
      CheckBotSettings();
      TwitchController.Start();
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
      TwitchController.Stop();
    }
    #endregion

    int iHistory;
    readonly List<string> historyList = new List<string>();

    private void Message_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {

    }

    private void Message_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
      if (e.Key == System.Windows.Input.Key.Enter)
      {
        string message = Message.Text;
        BotLogic.OnMessage(new Message(
          TwitchController.twitchChannelId,
          BotSettings.twitch.channelUsername,
          UserLevel.Owner,
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

    void Title_Loaded(
      object sender,
      RoutedEventArgs e)
    {
      BotLogic.streamTitle = TitleText.Text = BotLogic.streamTitle; // I'm not crazy, promise ;)
    }

    void Title_LostFocus(
      object sender,
      RoutedEventArgs e)
    {
      BotLogic.streamTitle = TitleText.Text;
    }

    private void OpenSettings_OnClick(object sender, RoutedEventArgs e)
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
      BotLogic.streamTitle = TitleText.Text;
    }
  }
}
