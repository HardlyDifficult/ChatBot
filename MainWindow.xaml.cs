using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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
      TwitchController.Start();
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
      Title.Text = BotLogic.streamTitle;
    }

    void Title_LostFocus(
      object sender,
      RoutedEventArgs e)
    {
      BotLogic.streamTitle = Title.Text;
    }
  }
}
