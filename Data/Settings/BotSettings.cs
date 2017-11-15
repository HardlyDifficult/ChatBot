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
  public class BotSettings
  {
    const string settingsFilename = "../Settings.json";

    static BotSettings instance;

    [JsonProperty]
    readonly TwitchSettings _twitch = new TwitchSettings();

    [JsonProperty]
    readonly TwitterSettings _twitter = new TwitterSettings();

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

    public static void Save()
    {
      string json = JsonConvert.SerializeObject(instance);
      File.WriteAllText(settingsFilename, json);
    }
  }
}
