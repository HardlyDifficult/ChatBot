using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace HD
{
  [JsonObject(MemberSerialization.OptIn)]
  public class TwitchSettings
  {
    #region Data
    [JsonProperty]
    string _channelUsername;

    [JsonProperty]
    string _clientId;

    [JsonProperty]
    string _channelOauth;

    [JsonProperty]
    string _botUsername;

    [JsonProperty]
    string _botOauth;
    #endregion

    #region Properties
    public string channelUsername
    {
      get
      {
        return _channelUsername;
      }
      set
      {
        _channelUsername = value;
        BotSettings.Save();
      }
    }

    public string clientId
    {
      get
      {
        return _clientId;
      }
      set
      {
        _clientId = value;
        BotSettings.Save();
      }
    }

    /// <summary>
    /// TODO document scope required
    /// </summary>
    public string channelOauth
    {
      get
      {
        return _channelOauth;
      }
      set
      {
        _channelOauth = value;
        BotSettings.Save();
      }
    }

    public string botUsername
    {
      get
      {
        return _botUsername;
      }
      set
      {
        _botUsername = value;
        BotSettings.Save();
      }
    }

    /// <summary>
    /// TODO document scope required
    /// </summary>
    public string botOauth
    {
      get
      {
        return _botOauth;
      }
      set
      {
        _botOauth = value;
        BotSettings.Save();
      }
    }
    #endregion
  }
}
