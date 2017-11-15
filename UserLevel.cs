﻿using System;
using System.Threading.Tasks;
using TwitchLib;
using TwitchLib.Models.Client;

namespace HD
{
  public enum UserLevel
  {
    Everyone = 0,
    Follower = 1,
    Subscribers = 2,
    Mods = 3,
    Owner = 4
  }

  public static class UserLevelHelpers
  {
    public static UserLevel Get(
      ChatMessage chatMessage)
    {
      if (chatMessage.IsBroadcaster)
      {
        return UserLevel.Owner;
      }
      else if (chatMessage.IsModerator)
      {
        return UserLevel.Mods;
      }
      else if (chatMessage.IsSubscriber)
      {
        return UserLevel.Subscribers;
      }
      else if (IsFollowing(chatMessage.UserId).Result)
      {
        return UserLevel.Follower;
      }
      else
      {
        return UserLevel.Everyone;
      }
    }

    public static UserLevel Get(
      string userId)
    {
      if (userId == TwitchController.twitchChannelId)
      {
        return UserLevel.Owner;
      }
      else if (IsMod(userId))
      {
        return UserLevel.Mods;
      }
      else if (IsSubscribed(userId))
      {
        return UserLevel.Subscribers;
      }
      else if (IsFollowing(userId).Result)
      {
        return UserLevel.Follower;
      }
      else
      {
        return UserLevel.Everyone;
      }
    }

    internal static async Task<bool> IsFollowing(
       string userId)
    {
      try
      {
        return (await TwitchController.twitchApi.Users.v5.CheckUserFollowsByChannelAsync(
          userId.ToString(),
          TwitchController.twitchChannelId.ToString())) != null;
      }
      catch
      {
        return false;
      }
    }

    internal static bool IsSubscribed(
       string userId)
    {
      try
      {
        return TwitchController.twitchApi.Users.v5.CheckUserSubscriptionByChannelAsync(userId.ToString(),
          TwitchController.twitchChannelId.ToString()).Result != null;
      }
      catch
      {
        return false;
      }
    }

    internal static bool IsMod(
      string userId)
    {
      return SqlManager.GetUserLevel(userId) >= UserLevel.Mods;
    }
  }

  public static class UserLevelExtensions
  {
    public static bool Includes(
      this UserLevel userLevel,
      string userId)
    {
      switch (userLevel)
      {
        case UserLevel.Everyone:
          return true;
        case UserLevel.Follower:
          if (UserLevel.Subscribers.Includes(userId))
          {
            return true;
          }

          return UserLevelHelpers.IsFollowing(userId).Result;
        case UserLevel.Subscribers:
          if (UserLevel.Mods.Includes(userId))
          {
            return true;
          }

          return UserLevelHelpers.IsSubscribed(userId);
        case UserLevel.Mods:
          if (UserLevel.Owner.Includes(userId))
          {
            return true;
          }

          return UserLevelHelpers.IsMod(userId);
        default:
        case UserLevel.Owner:
          return userId == TwitchController.twitchChannelId;
      }
    }

    public static bool Includes(
      this UserLevel userLevel,
      ChatMessage chatMessage)
    {
      switch (userLevel)
      {
        case UserLevel.Everyone:
          return true;
        case UserLevel.Follower:
          if (UserLevel.Subscribers.Includes(chatMessage))
          {
            return true;
          }

          return UserLevelHelpers.IsFollowing(chatMessage.UserId).Result;
        case UserLevel.Subscribers:
          if (UserLevel.Mods.Includes(chatMessage))
          {
            return true;
          }

          return chatMessage.IsSubscriber;
        case UserLevel.Mods:
          if (UserLevel.Owner.Includes(chatMessage))
          {
            return true;
          }

          return chatMessage.IsModerator;
        default:
        case UserLevel.Owner:
          return chatMessage.IsBroadcaster;
      }
    }
  }
}
