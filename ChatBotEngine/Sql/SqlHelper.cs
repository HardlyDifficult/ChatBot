using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace HD
{
  public class SqlHelper
  {
    // TODO wire up again!
    internal static UserLevel GetUserLevel(
      string userId)
    {
      SQLiteCommand command = new SQLiteCommand(
        "select UserLevel from users where UserId=@UserId", SqlManager.dbConnection);
      command.Parameters.Add(new SQLiteParameter("@UserId", userId));

      object result = command.ExecuteScalar();

      if (result == null || result is DBNull)
      {
        return 0;
      }

      return (UserLevel)(long)result;
    }

    public static void StoreUser(
     string userId, string displayName, UserLevel userLevel)
    {
      SQLiteCommand command = new SQLiteCommand(
        "update users set UserName=@UserName, UserLevel=@UserLevel, DateLastSeenInTicks=@DateLastSeenInTicks where UserId=@UserId", SqlManager.dbConnection);
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

  }
}
