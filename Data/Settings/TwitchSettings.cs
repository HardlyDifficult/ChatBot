using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace HD
{
  [JsonObject(MemberSerialization.OptIn)]
  public class TwitchSettings
  {
    #region Data
    [JsonProperty(PropertyName = nameof(channelUsername))] 
    string _channelUsername;

    [JsonProperty(PropertyName = nameof(clientId))]
    string _clientId;

    [JsonProperty(PropertyName = nameof(channelOauth))]
    string _channelOauth;

    [JsonProperty(PropertyName = nameof(botUsername))]
    string _botUsername;

    [JsonProperty(PropertyName = nameof(botOauth))]
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
      }
    }
    #endregion
  }
}
