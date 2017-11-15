using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace HD
{
  public class TwitterSettings
  {
    #region Data
    [JsonProperty]
    string _consumerKey;

    [JsonProperty]
    string _consumerSecret;

    [JsonProperty]
    string _accessToken;

    [JsonProperty]
    string _accessTokenSecret;
    #endregion

    #region Properties
    public string consumerKey
    {
      get
      {
        return _consumerKey;
      }
      set
      {
        _consumerKey = value;
        BotSettings.Save();
      }
    }

    public string consumerSecret
    {
      get
      {
        return _consumerSecret;
      }
      set
      {
        _consumerSecret = value;
        BotSettings.Save();
      }
    }

    public string accessToken
    {
      get
      {
        return _accessToken;
      }
      set
      {
        _accessToken = value;
        BotSettings.Save();
      }
    }

    public string accessTokenSecret
    {
      get
      {
        return _accessTokenSecret;
      }
      set
      {
        _accessTokenSecret = value;
        BotSettings.Save();
      }
    }
    #endregion
  }
}
