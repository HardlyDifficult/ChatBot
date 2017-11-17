using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HD.Features.Chat.Credits
{
  /// <summary>
  /// This was kind of working before... but needs to be rethought.
  /// </summary>
  class CreditsFeatures : IBotFeature
  {

    //static void RecordCredits(
    //  Message message)
    //{
    //  // TODO credits
    //  string usernameToCredit = null;
    //  Project? project = null;
    //  Category? category = null;
    //  string userId = null;
    //  string creditMessage = null;

    //  try
    //  {
    //    if(message.message.Contains("=") == false)
    //    { // Assume it's display credits instead
    //      return;
    //    }

    //    string[] tokens = message.message.GetBetween(" ", "=")?.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
    //    for(int i = 0; i < tokens.Length; i++)
    //    {
    //      if(project == null && Enum.TryParse<Project>(tokens[i], true, out Project selectedProject))
    //      {
    //        project = selectedProject;
    //      }
    //      else if(category == null && Enum.TryParse<Category>(tokens[i], true, out Category selectedCategory))
    //      {
    //        category = selectedCategory;
    //      }
    //      else if(usernameToCredit == null)
    //      {
    //        usernameToCredit = tokens[i].ToLower();
    //        if(usernameToCredit.StartsWith("@"))
    //        {
    //          usernameToCredit = usernameToCredit.Substring(1);
    //        }
    //      }
    //      else
    //      { // Got something we didn't expect, abort!
    //        usernameToCredit = null;
    //        break;
    //      }
    //    }

    //    userId = TwitchController.instance.GetUserId(usernameToCredit);
    //    creditMessage = message.message.GetAfter("=");
    //  }
    //  catch { }

    //  if(usernameToCredit == null || userId == null || project == null || category == null || creditMessage == null)
    //  {
    //    TwitchController.instance.SendWhisper(message.displayName,
    //      $"Fail. Expecting !credit @user pizza art = Very short summary. {Enum.GetNames(typeof(Project)).ToCsv()} / {Enum.GetNames(typeof(Category)).ToCsv()}");
    //    return;
    //  }

    //  SqlManager.AddCredits(userId, message.userId, project.Value.ToString(), category.Value.ToString(), creditMessage);

    //  (int projectContributions, int totalContributions) = SqlManager.GetCreditsCount(userId, project.Value.ToString());

    //  StringBuilder builder = new StringBuilder();
    //  builder.Append("Yay, ");
    //  builder.Append(usernameToCredit);
    //  builder.Append("!");

    //  if(totalContributions > 1)
    //  {
    //    builder.Append(" That's ");
    //    builder.Append(projectContributions);
    //    builder.Append(" towards ");
    //    builder.Append(project.Value);
    //    if(totalContributions > projectContributions)
    //    {
    //      builder.Append(" (");
    //      builder.Append(totalContributions);
    //      builder.Append(" total)");
    //    }
    //    builder.Append(".");
    //  }

    //  builder.Append(" Thanks for everything!");

    //  TwitchController.instance.SendMessage(builder.ToString());
    //}




    //static void DisplayCredits(
    //  Message message)
    //{
    //  if(message.message.Contains("="))
    //  { // Assume modifying credits
    //    return;
    //  }

    //  string username = message.message.GetAfter(" ");
    //  if(username != null && username.StartsWith("@"))
    //  {
    //    username = username.Substring(1);
    //  }

    //  string userId = TwitchController.instance.GetUserId(username);
    //  if(userId == null)
    //  {
    //    TwitchController.instance.SendWhisper(message.displayName, $"I don't know who {username} is");
    //    return;
    //  }

    //  List<(string project, string category, string contribution)> contributions = SqlManager.GetContributions(userId);

    //  if(contributions == null || contributions.Count == 0)
    //  {
    //    TwitchController.instance.SendWhisper(message.displayName, $"I don't have any credits recorded for {username}");
    //    return;
    //  }

    //  StringBuilder builder = new StringBuilder();
    //  string lastProject = null;
    //  int countSinceLastProject = 0;

    //  builder.Append(username);
    //  builder.Append(" contributions: ");

    //  for(int i = 0; i < contributions.Count; i++)
    //  {
    //    if(contributions[i].project != lastProject)
    //    {
    //      countSinceLastProject = 0;
    //      if(lastProject != null)
    //      {
    //        builder.Append(". ");
    //      }
    //      builder.Append(contributions[i].project);
    //      builder.Append(": ");
    //      lastProject = contributions[i].project;
    //    }

    //    if(countSinceLastProject > 0)
    //    {
    //      builder.Append(",");
    //    }
    //    builder.Append(" ");
    //    builder.Append(contributions[i].contribution);

    //    countSinceLastProject++;
    //  }

    //  if(message.userLevel < UserLevel.Mods || message.isWhisper)
    //  {
    //    TwitchController.instance.SendWhisper(message.displayName, builder.ToString());
    //  }
    //  else
    //  {
    //    TwitchController.instance.SendMessage(builder.ToString());
    //  }
    //}
    void IBotFeature.Init()
    {
      //dynamicCommandList.Add(new DynamicCommand("!credit", null, UserLevel.Mods, RecordCredits));

      //dynamicCommandList.Add(new DynamicCommand("!credit", null, UserLevel.Everyone, DisplayCredits));
    }
  }
}
