using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using System.Reflection;
using System.Data.SQLite;
using System.Data.SQLite.Linq;

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
    //public static void AddCredits(
    //  string creditUserId,
    //  string reporterUserId,
    //  string project,
    //  string category,
    //  string contribution)
    //{
    //  SQLiteCommand command = new SQLiteCommand(
    //    "insert into credits(CreditUserId, ReporterUserId, DateCreatedInTicks, Project, Category, Contribution) values(@CreditUserId, @ReporterUserId, @DateCreatedInTicks, @Project, @Category, @Contribution)", dbConnection);
    //  command.Parameters.Add(new SQLiteParameter("@CreditUserId", creditUserId));
    //  command.Parameters.Add(new SQLiteParameter("@ReporterUserId", reporterUserId));
    //  command.Parameters.Add(new SQLiteParameter("@DateCreatedInTicks", DateTime.Now.Ticks));
    //  command.Parameters.Add(new SQLiteParameter("@Project", project));
    //  command.Parameters.Add(new SQLiteParameter("@Category", category));
    //  command.Parameters.Add(new SQLiteParameter("@Contribution", contribution));
    //  command.ExecuteNonQuery();
    //}

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
      if (userId == TwitchController.twitchChannelId)
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
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="tableDefinition">
    /// e.g. `{commandField}` TEXT NOT NULL, `{responseField}` TEXT NOT NULL
    /// </param>
    /// <param name="fieldList">
    /// e.g. {commandField}, {responseField}, {userLevelField}
    /// </param>
    /// <returns></returns>
    public static string GetSqlAlterTable(
      string tableName, 
      string tableDefinition,
      string fieldList)
    {
      return $@"
PRAGMA foreign_keys=off;
 
BEGIN TRANSACTION;
 
ALTER TABLE {tableName} RENAME TO temp_{tableName};
 
CREATE TABLE {tableName}(
( 
  {tableDefinition}
);
 
INSERT INTO {tableName} ({fieldList})
  SELECT {fieldList}
  FROM temp_{tableName};
 
DROP TABLE temp_{tableName};
 
COMMIT;
 
PRAGMA foreign_keys=on;
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
    
    //public static (int projectContributions, int totalContributions) GetCreditsCount(
    //  string userId,
    //  string project)
    //{
    //  SQLiteCommand command = new SQLiteCommand(
    //    "select count(*) from Credits where CreditUserId=@CreditUserId", dbConnection);
    //  command.Parameters.Add(new SQLiteParameter("@CreditUserId", userId));
    //  command.Parameters.Add(new SQLiteParameter("@Project", project));

    //  int totalContributions = (int)(long)command.ExecuteScalar();

    //  command.CommandText += " and Project=@Project";
    //  int projectContributions = (int)(long)command.ExecuteScalar();

    //  return (projectContributions, totalContributions);
    //}
       

    //internal static List<(string project, string category, string contribution)> GetContributions(
    //  string userId)
    //{
    //  SQLiteCommand command = new SQLiteCommand(
    //   "select Project, Category, Contribution from Credits where CreditUserId=@CreditUserId order by Project, Category, Contribution", dbConnection);
    //  command.Parameters.Add(new SQLiteParameter("@CreditUserId", userId));

    //  List<(string project, string category, string contribution)> contributions = new List<(string project, string category, string contribution)>();
    //  using(DbDataReader reader = command.ExecuteReader())
    //  {
    //    while(reader.Read())
    //    {
    //      contributions.Add(((string)reader["Project"], (string)reader["Category"], (string)reader["Contribution"]));
    //    }
    //  }

    //  return contributions;
    //}

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
