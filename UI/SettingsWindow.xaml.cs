using System.ComponentModel;
using System.Windows;

namespace HD
{
  /// <summary>
  /// Interaction logic for SettingsWindow.xaml
  /// </summary>
  public partial class SettingsWindow : Window
  {
    private bool _settingsSaved = true;
    public SettingsWindow()
    {
      InitializeComponent();
      LoadSettings();
      if (CheckSettings() == false)
        _settingsSaved = false;
    }

    private void LoadSettings()
    {
      LoadTwitch();
      LoadTwitter();
    }

    private void LoadTwitter()
    {
      ConsumerKey.Text = BotSettings.twitter.consumerKey;
      ConsumerSecret.Text = BotSettings.twitter.consumerSecret;
      AccessToken.Text = BotSettings.twitter.accessToken;
      AccessTokenSecret.Text = BotSettings.twitter.accessTokenSecret;
    }

    private void LoadTwitch()
    {
      BotOAuth.Text = BotSettings.twitch.botOauth;
      BotUsername.Text = BotSettings.twitch.botUsername;
      ChannelOAuth.Text = BotSettings.twitch.channelOauth;
      ChannelUsername.Text = BotSettings.twitch.channelUsername;
      ClientId.Text = BotSettings.twitch.clientId;
    }

    private void SaveButton_OnClick(object sender, RoutedEventArgs e)
    {
      SaveSettings();
    }

    private void SaveSettings()
    {
      // Save twitter anyways, since there is no validation yet.
      SaveTwitter();
      if (CheckSettings() == false)
      {
        _settingsSaved = false;
        MessageBox.Show("Please insert all values for twitch\r\n\r\n" +
                        "Otherwise the bot can't start.",
                        "ChatBot",
                        MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
        return;
      }

      SaveTwitch();
      _settingsSaved = true;

      DialogResult = _settingsSaved;
      Close();
    }

    private void SaveTwitch()
    {
      BotSettings.twitch.botOauth = BotOAuth.Text;
      BotSettings.twitch.botUsername = BotUsername.Text;
      BotSettings.twitch.channelOauth = ChannelOAuth.Text;
      BotSettings.twitch.channelUsername = ChannelUsername.Text;
      BotSettings.twitch.clientId = ClientId.Text;
      BotSettings.Save();
    }

    private void SaveTwitter()
    {
      BotSettings.twitter.consumerKey = ConsumerKey.Text;
      BotSettings.twitter.consumerSecret = ConsumerSecret.Text;
      BotSettings.twitter.accessToken = AccessToken.Text;
      BotSettings.twitter.accessTokenSecret = AccessTokenSecret.Text;
      BotSettings.Save();
    }

    private bool CheckSettings()
    { // we can later add validation for twitter settings
      return CheckTwitchSettings();
    }

    private bool CheckTwitchSettings()
    {
      return !string.IsNullOrEmpty(BotOAuth.Text) &&
             !string.IsNullOrEmpty(BotUsername.Text) &&
             !string.IsNullOrEmpty(ChannelOAuth.Text) &&
             !string.IsNullOrEmpty(ChannelUsername.Text) &&
             !string.IsNullOrEmpty(ClientId.Text);
    }

    private void SettingsWindow_OnClosing(object sender, CancelEventArgs e)
    {
      if (_settingsSaved)
      {
        DialogResult = _settingsSaved;
        return;
      }

      if (MessageBox.Show("The bot is not configured properly. " +
                          "Close application?", "ChatBot",
                          MessageBoxButton.YesNo,
                          MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
      {
        DialogResult = false;
      }
      else
      {
        e.Cancel = true;
      }
    }
  }
}
