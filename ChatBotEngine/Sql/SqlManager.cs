using System;
using System.Data.Common;
using System.Diagnostics;
using System.Data.SQLite;

namespace HD
{
  /// <summary>
  /// TODO - check exceptions (like a lock) and try to re-open
  /// </summary>
  public static class SqlManager
  {
    #region Data
    static readonly SQLiteConnection dbConnection;
    #endregion

    #region Init
    static SqlManager()
    {
      dbConnection = new SQLiteConnection("Data Source=../MyDatabase.sqlite;Version=3;");
      dbConnection.Open();
    }
    #endregion

    #region Write API
    public static void SetHasAutoFollowed(
      string userId,
      bool hasAutoFollowed = true)
    {
      SQLiteCommand command = new SQLiteCommand("update Users set HasAutoFollowed=@HasAutoFollowed where UserId=@UserId", dbConnection);
      command.Parameters.Add(new SQLiteParameter("@UserId", userId));
      command.Parameters.Add(new SQLiteParameter("@HasAutoFollowed", hasAutoFollowed));
      command.ExecuteNonQuery();
    }

    public static void SetStringValue(
      string key,
      string message)
    {
      SQLiteCommand command = new SQLiteCommand(
                      "update KeyStringValue set Value=@Value where Key=@Key", dbConnection);
      command.Parameters.Add(new SQLiteParameter("@Key", key));
      command.Parameters.Add(new SQLiteParameter("@Value", message));
      command.ExecuteNonQuery();
    }

    public static void SetLongValue(
      string key,
      long value)
    {
      SQLiteCommand command = new SQLiteCommand(
                      "update KeyintValue set Value=@Value where Key=@Key", dbConnection);
      command.Parameters.Add(new SQLiteParameter("@Key", key));
      command.Parameters.Add(new SQLiteParameter("@Value", value));
      command.ExecuteNonQuery();
    }

    public static void SetShoutoutMessage(
      string streamerId,
      string newShoutoutMessage)
    {
      SQLiteCommand command = new SQLiteCommand("update Shoutouts set message=@message where userid=@userid", dbConnection);
      command.Parameters.Add(new SQLiteParameter("@userid", streamerId));
      command.Parameters.Add(new SQLiteParameter("@message", newShoutoutMessage));
      if (command.ExecuteNonQuery() > 0)
      {
        return;
      }

      command = new SQLiteCommand("insert into Shoutouts(userid, message) values(@userid, @message)", dbConnection);
      command.Parameters.Add(new SQLiteParameter("@userid", streamerId));
      command.Parameters.Add(new SQLiteParameter("@message", newShoutoutMessage));
      command.ExecuteNonQuery();
    }

    internal static void StoreUser(
      string userId, string displayName, UserLevel userLevel)
    {
      SQLiteCommand command = new SQLiteCommand(
        "update users set UserName=@UserName, UserLevel=@UserLevel, DateLastSeenInTicks=@DateLastSeenInTicks where UserId=@UserId", dbConnection);
      command.Parameters.Add(new SQLiteParameter("@UserId", userId));
      command.Parameters.Add(new SQLiteParameter("@Username", displayName));
      command.Parameters.Add(new SQLiteParameter("@UserLevel", (int)userLevel));
      command.Parameters.Add(new SQLiteParameter("@DateLastSeenInTicks", DateTime.Now.Ticks));

      try
      {
        int result = command.ExecuteNonQuery();
        if (result > 0)
        {
          return;
        }
      }
      catch { }

      command.CommandText = "insert into users(UserId, Username, UserLevel, DateLastSeenInTicks) values(@UserId, @Username, @UserLevel, @DateLastSeenInTicks)";
      try
      {
        int result = command.ExecuteNonQuery();
        if (result > 0)
        {
          return;
        }
      }
      catch { }
    }

    internal static void DropAllSubs()
    {
      SQLiteCommand command = new SQLiteCommand(
       "delete from subs", dbConnection);
      command.ExecuteNonQuery();
    }

    internal static void RecordSub(
      string userId,
      int tier1To3)
    {
      if (userId == TwitchController.instance.twitchChannelId)
      {
        return;
      }

      if (tier1To3 == 3)
      {
        tier1To3 = 6;
      }

      SQLiteCommand command = null;
      try
      {
        command = new SQLiteCommand(
        "insert into subs(UserId, Points) values(@UserId, @Points)", dbConnection);
        command.Parameters.Add(new SQLiteParameter("@UserId", userId));
        command.Parameters.Add(new SQLiteParameter("@Points", tier1To3));
        command.ExecuteNonQuery();
      }
      catch
      {
        command.CommandText = "update subs set Points=@Points where UserId=@UserId";
        int count = command.ExecuteNonQuery();
        Debug.Assert(count == 1);
      }
    }

    public static int GetTotalSubCount()
    {
      SQLiteCommand command = new SQLiteCommand(
       "select sum(Points) from subs", dbConnection);
      return Convert.ToInt32(command.ExecuteScalar()) + 1;
    }

    public static bool ExecuteNonQuery(
      string sql,
      params (string name, object value)[] parameters)
    {
      SQLiteCommand command = CreateCommand(sql, parameters);
      return command.ExecuteNonQuery() > 0;
    }
    #endregion

    #region Read API
    /// <param name="tableDefinition">
    /// e.g. `{commandField}` TEXT NOT NULL, `{responseField}` TEXT NOT NULL
    /// </param>
    /// <param name="fieldList">
    /// e.g. {commandField}, {responseField}, {userLevelField}
    /// </param>
    public static string GetSqlAlterTable(
      string tableName, 
      string tableDefinition,
      string fieldList)
    {
      return $@"
ALTER TABLE {tableName} RENAME TO temp_{tableName};
 
CREATE TABLE {tableName}
( 
  {tableDefinition}
);
 
INSERT INTO {tableName} ({fieldList})
  SELECT {fieldList}
  FROM temp_{tableName};
 
DROP TABLE temp_{tableName};
            ";
    }

    public static DbDataReader GetReader(
      string sql, 
      params (string key, object value)[] parameters)
    {
      SQLiteCommand command = CreateCommand(sql, parameters);

      return command.ExecuteReader();
    }

    public static bool TableExists(
      string tableName)
    {
      SQLiteCommand command = new SQLiteCommand(
        "SELECT name FROM sqlite_master WHERE type='table' AND name=@name;", dbConnection);
      command.Parameters.Add(new SQLiteParameter("@name", tableName));
      bool exists = command.ExecuteScalar() != null;
      return exists;
    }

    public static object GetScalar(
      string sql,
      params (string name, object value)[] parameters)
    {
      SQLiteCommand command = CreateCommand(sql, parameters);
      return command.ExecuteScalar();
    }

    internal static UserLevel GetUserLevel(
      string userId)
    {
      SQLiteCommand command = new SQLiteCommand(
        "select UserLevel from users where UserId=@UserId", dbConnection);
      command.Parameters.Add(new SQLiteParameter("@UserId", userId));

      return (UserLevel)(long)(command.ExecuteScalar() ?? 0);
    }

    internal static bool HasAutoFollowedBefore(
      string userId)
    {
      SQLiteCommand command = new SQLiteCommand(
        "select HasAutoFollowed from Users where UserId=@UserId", dbConnection);
      command.Parameters.Add(new SQLiteParameter("@UserId", userId));
      object result = command.ExecuteScalar();
      try
      {
        return ((long)result) > 0;
      }
      catch { }
      return false;
    }

    public static string GetShoutoutMessage(
      string userIdToShoutout)
    {
      SQLiteCommand command = new SQLiteCommand("select message from shoutouts where UserId=@UserId", dbConnection);
      command.Parameters.Add(new SQLiteParameter("@UserId", userIdToShoutout));
      return (string)command.ExecuteScalar();
    }
    #endregion

    #region Private Read API
    static SQLiteCommand CreateCommand(
      string sql, 
      (string name, object value)[] parameters)
    {
      SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
      for (int i = 0; i < parameters.Length; i++)
      {
        (string name, object value) = parameters[i];
        command.Parameters.Add(new SQLiteParameter(name, value));
      }

      return command;
    }
    #endregion
  }
}
