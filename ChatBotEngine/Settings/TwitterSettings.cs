using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace HD
{
  /// <summary>
  /// This is optional
  /// </summary>
  [JsonObject(MemberSerialization.OptIn)]
  public class TwitterSettings
  {
    #region Data
    [JsonProperty (PropertyName = nameof(consumerKey))]
    string _consumerKey;

    [JsonProperty (PropertyName = nameof(consumerSecret))]
    string _consumerSecret;

    [JsonProperty (PropertyName = nameof(accessToken))]
    string _accessToken;

    [JsonProperty (PropertyName = nameof(accessTokenSecret))]
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
      }
    }
    #endregion
  }
}
