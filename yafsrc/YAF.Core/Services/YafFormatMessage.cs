/* Yet Another Forum.net
 * Copyright (C) 2003-2005 Bj�rnar Henden
 * Copyright (C) 2006-2010 Jaben Cargman
 * http://www.yetanotherforum.net/
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 */
namespace YAF.Core.Services
{
  #region Using

  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text.RegularExpressions;
  using System.Web;

  using YAF.Core;
  using YAF.Core.BBCode;
  using YAF.Core.BBCode.ReplaceRules;
  using YAF.Types.Interfaces; using YAF.Types.Constants;
  using YAF.Classes.Data;
  using YAF.Utils;
  using YAF.Utils.Helpers;
  using YAF.Utils.Helpers.StringUtils;
  using YAF.Types;
  using YAF.Types.Constants;
  using YAF.Types.Flags;
  using YAF.Types.Interfaces;

  #endregion

  /// <summary>
  /// YafFormatMessage provides functions related to formatting the post messages.
  /// </summary>
  public static class YafFormatMessage
  {
    /* Ederon : 6/16/2007 - conventions */
    #region Constants and Fields

    /// <summary>
    ///   format message regex
    /// </summary>
    private static RegexOptions _options = RegexOptions.IgnoreCase | RegexOptions.Multiline;

    #endregion

    #region Public Methods

    /// <summary>
    /// For backwards compatibility
    /// </summary>
    /// <param name="message">
    /// the message to add smiles to.
    /// </param>
    /// <returns>
    /// The add smiles.
    /// </returns>
    public static string AddSmiles([NotNull] string message)
    {
      var layers = new ProcessReplaceRules();
      AddSmiles(ref layers);

      // apply...
      layers.Process(ref message);
      return message;
    }

    /// <summary>
    /// Adds smiles replacement rules to the collection from the DB
    /// </summary>
    /// <param name="rules">
    /// The rules.
    /// </param>
    public static void AddSmiles([NotNull] ref ProcessReplaceRules rules)
    {
      CodeContracts.ArgumentNotNull(rules, "rules");

      var smiles = YafContext.Current.Get<IDBBroker>().GetSmilies();
      int codeOffset = 0;

      foreach (var smile in smiles)
      {
        string code = smile.Code;
        code = code.Replace("&", "&amp;");
        code = code.Replace(">", "&gt;");
        code = code.Replace("<", "&lt;");
        code = code.Replace("\"", "&quot;");

        // add new rules for smilies...
        var lowerRule = new SimpleReplaceRule(
          code.ToLower(),
          @"<img src=""{0}"" alt=""{1}"" />".FormatWith(
            YafBuildLink.Smiley(smile.Icon), HttpContext.Current.Server.HtmlEncode(smile.Emoticon)));

        var upperRule = new SimpleReplaceRule(
          code.ToUpper(),
          @"<img src=""{0}"" alt=""{1}"" />".FormatWith(
            YafBuildLink.Smiley(smile.Icon), HttpContext.Current.Server.HtmlEncode(smile.Emoticon)));

        // increase the rank as we go...
        lowerRule.RuleRank = lowerRule.RuleRank + 100 + codeOffset;
        upperRule.RuleRank = upperRule.RuleRank + 100 + codeOffset;

        rules.AddRule(lowerRule);
        rules.AddRule(upperRule);

        // add a bit more rank
        codeOffset++;
      }
    }

    /// <summary>
    /// The method to detect a forbidden BBCode tag from delimited by 'delim' list 
    ///   'stringToMatch'
    /// </summary>
    /// <param name="stringToClear">
    /// Input string
    /// </param>
    /// <param name="stringToMatch">
    /// String with delimiter
    /// </param>
    /// <param name="delim">
    /// </param>
    /// <returns>
    /// Returns a string containing a forbidden BBCode or a null string
    /// </returns>
    [CanBeNull]
    public static string BBCodeForbiddenDetector([NotNull] string stringToClear, [NotNull] string stringToMatch, char delim)
    {
      // TODO: Convert to Regular Expression -- use the match function below -- BREAK DOWN INTO REUSABLE FUNCTIONS.
      bool checker = string.IsNullOrEmpty(stringToMatch);

      string tagForbidden = null;
      bool detectedTag = false;
      string[] codes = stringToMatch.Split(delim);
      char[] charray = stringToClear.ToCharArray();
      int openPosition = 0;
      int currentPosition = 0;

      // Loop through char array i will be current poision
      for (int i = 0; i < charray.Length; i++)
      {
        if (i >= currentPosition)
        {
          // bbcode token is detected
          if (charray[i] == '[')
          {
            openPosition = i;

            // we loop to find closing bracket, beginnin with i position
            for (int j = i; j < charray.Length - 1; j++)
            {
              // closing bracket found
              if (charray[j] != ']')
              {
                continue;
              }

              // we should reset the value in each cycle 
              // if an opening bracket was found
              detectedTag = false;
              string res = null;

              // now we loop through out allowed bbcode list
              foreach (string t in codes)
              {
                // closing bracket is in position 'j' opening is in pos 'i'
                // we should not take into account closing bracket
                // as we have tags like '[URL='
                for (int l = openPosition; l < j; l++)
                {
                  res = res + charray[l].ToString().ToUpper();
                }

                if (checker)
                {
                  return "ALL";
                }

                // detect if the tag from list was found
                detectedTag = res.Contains("[" + t.ToUpper().Trim()) || res.Contains("[/" + t.ToUpper().Trim());
                res = string.Empty;

                // if so we go out from k-loop after we should go out from j-loop too
                if (detectedTag)
                {
                  currentPosition = j + 1;
                  break;
                }
              }

              currentPosition = j + 1;

              // we didn't found the allowed tag in k-loop through allowed list,
              // so the tag is forbidden one and we should exit
              if (!detectedTag)
              {
                tagForbidden = stringToClear.Substring(i, j - i + 1).ToUpper();
                return tagForbidden;
              }

              if (detectedTag)
              {
                break;
              }

              // continue to loop
            }
          }
        }
      }

      return null;
    }

    /// <summary>
    /// The method used to get response string, if a forbidden tag is detected.
    /// </summary>
    /// <param name="checkString">
    /// The string to check.
    /// </param>
    /// <param name="acceptedTags">
    /// The list of accepted tags.
    /// </param>
    /// <param name="delim">
    /// The delimeter in a tags list.
    /// </param>
    /// <returns>
    /// A message string.
    /// </returns>
    public static string CheckHtmlTags([NotNull] string checkString, [NotNull] string acceptedTags, char delim)
    {
      string detectedHtmlTag = HtmlTagForbiddenDetector(checkString, acceptedTags, delim);
      if (!string.IsNullOrEmpty(detectedHtmlTag) && detectedHtmlTag != "ALL")
      {
        return YafContext.Current.Localization.GetTextFormatted(
          "HTMLTAG_WRONG", HttpUtility.HtmlEncode(detectedHtmlTag));
      }
      else if (detectedHtmlTag == "ALL")
      {
        return YafContext.Current.Localization.GetText("HTMLTAG_FORBIDDEN");
      }

      return string.Empty;
    }

    /// <summary>
    /// The format message.
    /// </summary>
    /// <param name="message">
    /// The message.
    /// </param>
    /// <param name="messageFlags">
    /// The message flags.
    /// </param>
    /// <returns>
    /// The formatted message.
    /// </returns>
    public static string FormatMessage([NotNull] string message, [NotNull] MessageFlags messageFlags)
    {
      return FormatMessage(message, messageFlags, false);
    }

    /// <summary>
    /// The format message.
    /// </summary>
    /// <param name="message">
    /// The message.
    /// </param>
    /// <param name="messageFlags">
    /// The message flags.
    /// </param>
    /// <param name="targetBlankOverride">
    /// The target blank override.
    /// </param>
    /// <returns>
    /// The formated message.
    /// </returns>
    public static string FormatMessage([NotNull] string message, [NotNull] MessageFlags messageFlags, bool targetBlankOverride)
    {
      return FormatMessage(message, messageFlags, targetBlankOverride, DateTime.UtcNow);
    }

    /// <summary>
    /// The format message.
    /// </summary>
    /// <param name="message">
    /// The message.
    /// </param>
    /// <param name="messageFlags">
    /// The message flags.
    /// </param>
    /// <param name="targetBlankOverride">
    /// The target blank override.
    /// </param>
    /// <param name="messageLastEdited">
    /// The message last edited.
    /// </param>
    /// <returns>
    /// The formatted message.
    /// </returns>
    public static string FormatMessage([NotNull] string message, [NotNull] MessageFlags messageFlags, bool targetBlankOverride, DateTime messageLastEdited)
    {
      bool useNoFollow = YafContext.Current.BoardSettings.UseNoFollowLinks;

      // check to see if no follow should be disabled since the message is properly aged
      if (useNoFollow && YafContext.Current.BoardSettings.DisableNoFollowLinksAfterDay > 0)
      {
        TimeSpan messageAge = messageLastEdited - DateTime.UtcNow;
        if (messageAge.Days > YafContext.Current.BoardSettings.DisableNoFollowLinksAfterDay)
        {
          // disable no follow
          useNoFollow = false;
        }
      }

      // do html damage control
      message = RepairHtml(message, messageFlags.IsHtml);

      // get the rules engine from the creator...
      ProcessReplaceRules ruleEngine =
        ReplaceRulesCreator.GetInstance(new[] { messageFlags.IsBBCode, targetBlankOverride, useNoFollow });

      // see if the rules are already populated...
      if (!ruleEngine.HasRules)
      {
        // populate

        // get rules for YafBBCode and Smilies
        YafBBCode.CreateBBCodeRules(ref ruleEngine, messageFlags.IsBBCode, targetBlankOverride, useNoFollow);

        // add email rule
        // vzrus: it's freezing  when post body contains full email adress.
        // the fix provided by community 
        var email =
          new VariableRegexReplaceRule(
            @"(?<before>^|[ ]|\>|\[[A-Za-z0-9]\])(?<inner>(([A-Za-z0-9]+_+)|([A-Za-z0-9]+\-+)|([A-Za-z0-9]+\.+)|([A-Za-z0-9]+\++))*[A-Za-z0-9]+@((\w+\-+)|(\w+\.))*\w{1,63}\.[a-zA-Z]{2,6})", 
            "${before}<a href=\"mailto:${inner}\">${inner}</a>", 
            _options, 
            new[] { "before" });

        email.RuleRank = 10;

        ruleEngine.AddRule(email);

        // URLs Rules
        string target = (YafContext.Current.BoardSettings.BlankLinks || targetBlankOverride)
                          ? "target=\"_blank\""
                          : string.Empty;
        string nofollow = useNoFollow ? "rel=\"nofollow\"" : string.Empty;

        var url =
          new VariableRegexReplaceRule(
            @"(?<before>^|[ ]|\>|\[[A-Za-z0-9]\])(?<!href="")(?<!src="")(?<inner>(http://|https://|ftp://)(?:[\w-]+\.)+[\w-]+(?:/[\w-./?+%#&=;:,]*)?)", 
            "${before}<a {0} {1} href=\"${inner}\" title=\"${inner}\">${innertrunc}</a>".Replace("{0}", target).Replace(
              "{1}", nofollow), 
            _options, 
            new[] { "before" }, 
            new[] { string.Empty }, 
            50);

        url.RuleRank = 10;
        ruleEngine.AddRule(url);

        // ?<! - match if prefixes href="" and src="" are not present
        // <inner> = named capture group
        // (http://|https://|ftp://) - numbered capture group - select from 3 alternatives
        // Match expression but don't capture it, one or more repetions, in the end is dot(\.)? here we match "www." - (?:[\w-]+\.)+
        // Match expression but don't capture it, zero or one repetions (?:/[\w-./?%&=+;,:#~$]*[^.<])?
        // (?<inner>(http://|https://|ftp://)(?:[\w-]+\.)+[\w-]+(?:/[\w-./?%&=+;,:#~$]*[^.<])?)
        url =
          new VariableRegexReplaceRule(
            @"(?<before>^|[ ]|\>|\[[A-Za-z0-9]\])(?<!href="")(?<!src="")(?<inner>(http://|https://|ftp://)(?:[\w-]+\.)+[\w-]+(?:/[\w-./?%&=+;,:#~$]*[^.<|^.\[])?)", 
            "${before}<a {0} {1} href=\"${inner}\" title=\"${inner}\">${innertrunc}</a>".Replace("{0}", target).Replace(
              "{1}", nofollow), 
            _options, 
            new[] { "before" }, 
            new[] { string.Empty }, 
            50);
        url.RuleRank = 10;
        ruleEngine.AddRule(url);

        url =
          new VariableRegexReplaceRule(
            @"(?<before>^|[ ]|\>|\[[A-Za-z0-9]\])(?<!http://)(?<inner>www\.(?:[\w-]+\.)+[\w-]+(?:/[\w-./?%+#&=;,]*)?)", 
            "${before}<a {0} {1} href=\"http://${inner}\" title=\"http://${inner}\">${innertrunc}</a>".Replace(
              "{0}", target).Replace("{1}", nofollow), 
            _options, 
            new[] { "before" }, 
            new[] { string.Empty }, 
            50);
        url.RuleRank = 10;
        ruleEngine.AddRule(url);
      }

      // process...
      ruleEngine.Process(ref message);

      message = YafContext.Current.Get<IBadWordReplace>().Replace(message);

      return message;
    }

    /// <summary>
    /// The format syndication message.
    /// </summary>
    /// <param name="message">
    /// The message.
    /// </param>
    /// <param name="messageFlags">
    /// The message flags.
    /// </param>
    /// <param name="altItem">
    /// The alt Item.
    /// </param>
    /// <param name="charsToFetch">
    /// The chars To Fetch.
    /// </param>
    /// <returns>
    /// The formatted message.
    /// </returns>
    [NotNull]
    public static string FormatSyndicationMessage([NotNull] string message, [NotNull] MessageFlags messageFlags, bool altItem, int charsToFetch)
    {
      message =
        @"<table class=""{0}"" width=""100%""><tr><td>{1}</td></tr></table>".FormatWith(
          altItem ? "content postContainer" : "content postContainer_Alt", FormatMessage(message, messageFlags, false));
      message = message.Replace("<div class=\"innerquote\">", "<blockquote>").Replace("[quote]", "</blockquote>");
      return message;

      // <span class=\"quotetitle\">tester1 �������:</span><div class=\"innerquote\">gfhgfhdf</div></div><br />vcxvxcvzcxv</td></tr></tab
    }

    /// <summary>
    /// The get cleaned topic message. Caches cleaned topic message by TopicID.
    /// </summary>
    /// <param name="topicMessage">
    /// The message to clean.
    /// </param>
    /// <param name="topicId">
    /// 
    ///   The topic id.
    /// </param>
    /// <returns>
    /// The get cleaned topic message.
    /// </returns>
    public static MessageCleaned GetCleanedTopicMessage([NotNull] object topicMessage, [NotNull] object topicId)
    {
      CodeContracts.ArgumentNotNull(topicMessage, "topicMessage");
      CodeContracts.ArgumentNotNull(topicId, "topicId");

      // get the common words for the language -- should be all lower case.
      List<string> commonWords = YafContext.Current.Localization.GetText("COMMON", "COMMON_WORDS").StringToList(',');

      string cacheKey = Constants.Cache.FirstPostCleaned.FormatWith(YafContext.Current.PageBoardID, topicId);
      var message = new MessageCleaned();

      if (!topicMessage.IsNullOrEmptyDBField())
      {
        message = YafContext.Current.Cache.GetItem(
          cacheKey, 
          YafContext.Current.BoardSettings.FirstPostCacheTimeout, 
          () =>
            {
              string returnMsg = topicMessage.ToString();
              var keywordList = new List<string>();

              if (returnMsg.IsSet())
              {
                // process message... clean html, strip html, remove bbcode, etc...
                returnMsg =
                  StringExtensions.RemoveMultipleWhitespace(
                    BBCodeHelper.StripBBCode(HtmlHelper.StripHtml(HtmlHelper.CleanHtmlString(returnMsg))));

                if (returnMsg.IsNotSet())
                {
                  returnMsg = string.Empty;
                }
                else
                {
                  // get string without punctuation
                  string keywordCleaned =
                    new string(returnMsg.Where(c => !char.IsPunctuation(c) || char.IsWhiteSpace(c)).ToArray()).Trim().
                      ToLower();

                  if (keywordCleaned.Length > 5)
                  {
                    // create keywords...
                    keywordList = keywordCleaned.StringToList(' ', commonWords);

                    // clean up the list a bit...
                    keywordList =
                      keywordList.GetNewNoEmptyStrings().GetNewNoSmallStrings(5).Where(x => !Char.IsNumber(x[0])).
                        Distinct().ToList();

                    // sort...
                    keywordList.Sort();

                    // get maximum of 50 keywords...
                    if (keywordList.Count > 50)
                    {
                      keywordList = keywordList.GetRange(0, 50);
                    }
                  }
                }
              }

              return new MessageCleaned(StringExtensions.Truncate(returnMsg, 255), keywordList);
            });
      }

      return message;
    }

    /// <summary>
    /// The method to detect a forbidden HTML code from delimited by 'delim' list
    /// </summary>
    /// <param name="stringToClear">
    /// </param>
    /// <param name="stringToMatch">
    /// </param>
    /// <param name="delim">
    /// </param>
    /// <returns>
    /// Returns a forbidden HTML tag or a null string
    /// </returns>
    [CanBeNull]
    public static string HtmlTagForbiddenDetector([NotNull] string stringToClear, [NotNull] string stringToMatch, char delim)
    {
      // TODO: Convert to Regular Expression -- THIS IS WHAT REGEX IF FOR!
      bool checker = string.IsNullOrEmpty(stringToMatch);

      string tagForbidden = null;
      bool detectedTag = false;
      string[] codes = stringToMatch.Split(delim);
      char[] charray = stringToClear.ToCharArray();
      int openPosition = 0;
      int currentPosition = 0;

      // Loop through char array i will be current poision
      for (int i = 0; i < charray.Length; i++)
      {
        if (i >= currentPosition)
        {
          // bbcode token is detected
          if (charray[i] == '<')
          {
            openPosition = i;

            // we loop to find closing bracket, beginnin with i position
            for (int j = i; j < charray.Length - 1; j++)
            {
              // closing bracket found
              if (charray[j] == '>')
              {
                // we should reset the value in each cycle 
                // if an opening bracket was found
                detectedTag = false;
                string res = null;

                // now we loop through out allowed bbcode list
                foreach (string t in codes)
                {
                  // closing bracket is in position 'j' opening is in pos 'i'
                  // we should not take into account closing bracket
                  // as we have tags like '[URL='
                  for (int l = openPosition; l < j; l++)
                  {
                    res = res + charray[l].ToString().ToUpper();
                  }

                  if (checker)
                  {
                    return "ALL";
                  }

                  // detect if the tag from list was found
                  detectedTag = res.Contains("<" + t.ToUpper().Trim()) || res.Contains("</" + t.ToUpper().Trim());
                  res = string.Empty;

                  // if so we go out from k-loop after we should go out from j-loop too
                  if (detectedTag)
                  {
                    currentPosition = j + 1;
                    break;
                  }
                }

                currentPosition = j + 1;

                // we didn't found the allowed tag in k-loop through allowed list,
                // so the tag is forbidden one and we should exit
                if (!detectedTag)
                {
                  tagForbidden = stringToClear.Substring(i, j - i + 1).ToUpper();
                  return tagForbidden;
                }

                if (detectedTag)
                {
                  break;
                }
              }

              // continue to loop
            }
          }
        }
      }

      return null;
    }

    /// <summary>
    /// Removes nested YafBBCode quotes from the given message body.
    /// </summary>
    /// <param name="body">
    /// Message body test to remove nested quotes from
    /// </param>
    /// <returns>
    /// A version of <paramref name="body"/> that contains no nested quotes.
    /// </returns>
    [NotNull]
    public static string RemoveNestedQuotes([NotNull] string body)
    {
      RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline;
      var quote = new Regex(@"\[quote(\=[^\]]*)?\](.*?)\[/quote\]", options);

      // remove quotes from old messages
      return quote.Replace(body, string.Empty).TrimStart();
    }

    /// <summary>
    /// The repair html.
    /// </summary>
    /// <param name="html">
    /// The html.
    /// </param>
    /// <param name="allowHtml">
    /// The allow html.
    /// </param>
    /// <returns>
    /// The repaired html.
    /// </returns>
    public static string RepairHtml([NotNull] string html, bool allowHtml)
    {
      // vzrus: NNTP temporary tweaks to wipe out server hangs. Put it here as it can be in every place.
      // These are '\n\r' things related to multiline regexps.
      MatchCollection mc1 = Regex.Matches(html, "[^\r]\n[^\r]", RegexOptions.IgnoreCase);
      for (int i = mc1.Count - 1; i >= 0; i--)
      {
        html = html.Insert(mc1[i].Index + 1, " \r");
      }

      MatchCollection mc2 = Regex.Matches(html, "[^\r]\n\r\n[^\r]", RegexOptions.IgnoreCase);
      for (int i = mc2.Count - 1; i >= 0; i--)
      {
        html = html.Insert(mc2[i].Index + 1, " \r");
      }

      if (!allowHtml)
      {
        html = YafBBCode.EncodeHTML(html);
      }
      else
      {
        // get allowable html tags       
        html = RemoveHtmlByList(html, YafContext.Current.BoardSettings.AcceptedHTML.Split(','));
      }

      return html;
    }

    /// <summary>
    /// The repair html.
    /// </summary>
    /// <param name="html">
    /// The html.
    /// </param>
    /// <param name="allowHtml">
    /// The allow html.
    /// </param>
    /// <returns>
    /// The repaired html.
    /// </returns>
    public static string RepairHtmlFeeds([NotNull] string html, bool allowHtml)
    {
      // vzrus: NNTP temporary tweaks to wipe out server hangs. Put it here as it can be in every place.
      // These are '\n\r' things related to multiline regexps.
      MatchCollection mc1 = Regex.Matches(html, "[^\r]\n[^\r]", RegexOptions.IgnoreCase);
      for (int i = mc1.Count - 1; i >= 0; i--)
      {
        html = html.Insert(mc1[i].Index + 1, " \r");
      }

      MatchCollection mc2 = Regex.Matches(html, "[^\r]\n\r\n[^\r]", RegexOptions.IgnoreCase);
      for (int i = mc2.Count - 1; i >= 0; i--)
      {
        html = html.Insert(mc2[i].Index + 1, " \r");
      }

      if (!allowHtml)
      {
        html = YafBBCode.EncodeHTML(html);
      }
      else
      {
        // get allowable html tags       
        html = RemoveHtmlByList(html, new[] { "a" });
      }

      return html;
    }

    /// <summary>
    /// Surrounds a word list with prefix/postfix. Case insensitive.
    /// </summary>
    /// <param name="message">
    /// </param>
    /// <param name="wordList">
    /// </param>
    /// <param name="prefix">
    /// </param>
    /// <param name="postfix">
    /// </param>
    /// <returns>
    /// The surround word list.
    /// </returns>
    public static string SurroundWordList(
      [NotNull] string message, [NotNull] IList<string> wordList, [NotNull] string prefix, [NotNull] string postfix)
    {
      CodeContracts.ArgumentNotNull(message, "message");
      CodeContracts.ArgumentNotNull(wordList, "wordList");
      CodeContracts.ArgumentNotNull(prefix, "prefix");
      CodeContracts.ArgumentNotNull(postfix, "postfix");

      const RegexOptions regexOptions = RegexOptions.IgnoreCase;

      foreach (string word in wordList.Where(w => w.Length > 3))
      {
        MatchAndPerformAction(
          "({0})".FormatWith(word.ToLower().ToRegExString()), 
          message, 
          (inner, index, length) =>
            {
              message = message.Insert(index + length, postfix);
              message = message.Insert(index, prefix);
            });

        // var matches = regEx.Matches(message).Cast<Match>().ToList().OrderByDescending(x => x.Index);

        // foreach (Match m in matches)
        // {
        // message = message.Insert(m.Index + m.Length, postfix);
        // message = message.Insert(m.Index, prefix);
        // }
      }

      return message;
    }

    #endregion

    #region Methods

    /// <summary>
    /// The make match list.
    /// </summary>
    /// <param name="matchRegEx">
    /// The match reg ex.
    /// </param>
    /// <param name="text">
    /// The text.
    /// </param>
    /// <returns>
    /// </returns>
    [NotNull]
    private static IList<string> MakeMatchList([NotNull] string matchRegEx, [NotNull] string text)
    {
      CodeContracts.ArgumentNotNull(matchRegEx, "matchRegEx");
      CodeContracts.ArgumentNotNull(text, "text");

      var matchList = new List<string>();

      MatchAndPerformAction(matchRegEx, text, (match, index, length) => matchList.Add(match));

      return matchList;
    }

    /// <summary>
    /// The match and perform action.
    /// </summary>
    /// <param name="matchRegEx">
    /// The match reg ex.
    /// </param>
    /// <param name="text">
    /// The text.
    /// </param>
    /// <param name="MatchAction">
    /// The match action.
    /// </param>
    private static void MatchAndPerformAction(
      [NotNull] string matchRegEx, [NotNull] string text, [NotNull] Action<string, int, int> MatchAction)
    {
      CodeContracts.ArgumentNotNull(matchRegEx, "matchRegEx");
      CodeContracts.ArgumentNotNull(text, "text");
      CodeContracts.ArgumentNotNull(MatchAction, "MatchAction");

      const RegexOptions options = RegexOptions.IgnoreCase;

      var matches = Regex.Matches(text, matchRegEx, options).Cast<Match>().OrderByDescending(x => x.Index);

      foreach (var match in matches)
      {
        string inner = text.Substring(match.Index + 1, match.Length - 1).Trim().ToLower();
        MatchAction(inner, match.Index, match.Length);
      }
    }

    /// <summary>
    /// The remove html by list.
    /// </summary>
    /// <param name="text">
    /// The text.
    /// </param>
    /// <param name="matchList">
    /// The match list.
    /// </param>
    /// <returns>
    /// The remove html by list.
    /// </returns>
    private static string RemoveHtmlByList([NotNull] string text, [NotNull] IEnumerable<string> matchList)
    {
      CodeContracts.ArgumentNotNull(text, "text");
      CodeContracts.ArgumentNotNull(matchList, "matchList");

      MatchAndPerformAction(
        "<.*?>", 
        text, 
        (tag, index, len) =>
          {
            if (!HtmlHelper.IsValidTag(tag, matchList))
            {
              text = text.Remove(index, len);
            }
          });

      return text;
    }

    #endregion

    /// <summary>
    /// The message cleaned class (internal)
    /// </summary>
    [Serializable]
    public class MessageCleaned
    {
      #region Constructors and Destructors

      /// <summary>
      ///   Initializes a new instance of the <see cref = "MessageCleaned" /> class.
      /// </summary>
      public MessageCleaned()
      {
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="MessageCleaned"/> class.
      /// </summary>
      /// <param name="messageTruncated">
      /// The message truncated.
      /// </param>
      /// <param name="messageKeywords">
      /// The message keywords.
      /// </param>
      public MessageCleaned([NotNull] string messageTruncated, [NotNull] List<string> messageKeywords)
      {
        this.MessageTruncated = messageTruncated;
        this.MessageKeywords = messageKeywords;
      }

      #endregion

      #region Properties

      /// <summary>
      ///   Gets or sets MessageKeywords.
      /// </summary>
      public List<string> MessageKeywords { get; set; }

      /// <summary>
      ///   Gets or sets MessageTruncated.
      /// </summary>
      public string MessageTruncated { get; set; }

      #endregion
    }
  }
}