using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HD
{
  /// <summary>
  /// This saves and loads the Settings.json file.
  /// </summary>
  public class BotSettings
  {
    #region Data
    const string settingsFilename = "../Settings.json";

    static BotSettings instance;

    [JsonProperty(PropertyName = nameof(twitch))]
    readonly TwitchSettings _twitch = new TwitchSettings();

    [JsonProperty(PropertyName = nameof(twitter))]
    readonly TwitterSettings _twitter = new TwitterSettings();
    #endregion

    #region Properties
    public static TwitchSettings twitch
    {
      get
      {
        if (instance == null)
        {
          Load();
        }
        return instance._twitch;
      }
    }

    public static TwitterSettings twitter
    {
      get
      {
        if (instance == null)
        {
          Load();
        }
        return instance._twitter;
      }
    }
    #endregion

    #region Public Write
    public static void Save()
    {
      string json = JsonConvert.SerializeObject(instance, Formatting.Indented);
      File.WriteAllText(settingsFilename, json);
    }
    #endregion

    #region Private Read
    static void Load()
    {
      Debug.Assert(instance == null);

      try
      {
        string json = File.ReadAllText(settingsFilename);
        instance = JsonConvert.DeserializeObject<BotSettings>(json);
      }
      catch { }

      if (instance == null)
      {
        instance = new BotSettings();
        Save();
      }
    }
    #endregion
  }
}
