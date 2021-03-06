/* Yet Another Forum.NET
 * Copyright (C) 2003-2005 Bjørnar Henden
 * Copyright (C) 2006-2013 Jaben Cargman
 * Copyright (C) 2014-2019 Ingo Herbote
 * http://www.yetanotherforum.net/
 * 
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at

 * http://www.apache.org/licenses/LICENSE-2.0

 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */
namespace YAF.Core.Model
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Web.Hosting;
    using ServiceStack.OrmLite;

    using YAF.Classes;
    using YAF.Core.Extensions;
    using YAF.Types;
    using YAF.Types.Extensions;
    using YAF.Types.Interfaces;
    using YAF.Types.Interfaces.Data;
    using YAF.Types.Models;
    using YAF.Types.Objects;
    using YAF.Utils.Helpers;

    /// <summary>
    ///     The Message repository extensions.
    /// </summary>
    public static class MessageRepositoryExtensions
    {
        #region Public Methods and Operators

        /// <summary>
        /// Gets all the post by a user.
        /// </summary>
        /// <param name="boardID">
        /// The board id.
        /// </param>
        /// <param name="userID">
        /// The user id.
        /// </param>
        /// <param name="pageUserID">
        /// The page user id.
        /// </param>
        /// <param name="topCount">
        /// Top count to return. Null is all.
        /// </param>
        /// <returns>
        /// </returns>
        public static DataTable AllUserAsDataTable(
            this IRepository<Message> repository,
            [NotNull] object boardId,
            [NotNull] object userID,
            [NotNull] object pageUserID,
            [NotNull] object topCount)
        {
            return repository.DbFunction.GetData.post_alluser(
                BoardID: boardId,
                UserID: userID,
                PageUserID: pageUserID,
                topCount: topCount);
        }


        /// <summary>
        /// The post_list.
        /// </summary>
        /// <param name="topicId">
        /// The topic id.
        /// </param>
        /// <param name="currentUserID"> </param>
        /// <param name="authorUserID">
        /// The author User ID.
        /// </param>
        /// <param name="updateViewCount">
        /// The update view count.
        /// </param>
        /// <param name="showDeleted">
        /// The show deleted.
        /// </param>
        /// <param name="styledNicks">
        /// The styled nicks.
        /// </param>
        /// <param name="sincePostedDate">
        /// The posted date.
        /// </param>
        /// <param name="toPostedDate">
        /// The to Posted Date.
        /// </param>
        /// <param name="sinceEditedDate">
        /// The edited date.
        /// </param>
        /// <param name="toEditedDate">
        /// The to Edited Date.
        /// </param>
        /// <param name="pageIndex">
        /// The page index.
        /// </param>
        /// <param name="pageSize">
        /// The Page size.
        /// </param>
        /// <param name="sortPosted">
        /// The sort by posted date.
        ///   0 - no sort, 1 - ASC, 2 - DESC
        /// </param>
        /// <param name="sortEdited">
        /// The sort by edited date.
        ///   0 - no sort, 1 - ASC, 2 - DESC.
        /// </param>
        /// <param name="sortPosition">
        /// The sort Position.
        /// </param>
        /// <param name="showThanks">
        /// The show thanks. Returnes thanked posts. Not implemented.
        /// </param>
        /// <param name="messagePosition">
        /// The message Position.
        /// </param>
        /// <returns>
        /// </returns>
        public static DataTable PostListAsDataTable(
            this IRepository<Message> repository,
            [NotNull] object topicId,
            object currentUserID,
            [NotNull] object authorUserID,
            [NotNull] object updateViewCount,
            bool showDeleted,
            bool styledNicks,
            bool showReputation,
            DateTime sincePostedDate,
            DateTime toPostedDate,
            DateTime sinceEditedDate,
            DateTime toEditedDate,
            int pageIndex,
            int pageSize,
            int sortPosted,
            int sortEdited,
            int sortPosition,
            bool showThanks,
            int messagePosition)
        {
            return repository.DbFunction.GetData.post_list(
                TopicID: topicId,
                PageUserID: currentUserID,
                AuthorUserID: authorUserID,
                UpdateViewCount: updateViewCount,
                ShowDeleted: showDeleted,
                StyledNicks: styledNicks,
                ShowReputation: showReputation,
                SincePostedDate: sincePostedDate,
                ToPostedDate: toPostedDate,
                SinceEditedDate: sinceEditedDate,
                ToEditedDate: toEditedDate,
                PageIndex: pageIndex,
                PageSize: pageSize,
                SortPosted: sortPosted,
                SortEdited: sortEdited,
                SortPosition: sortPosition,
                ShowThanks: showThanks,
                MessagePosition: messagePosition,
                UTCTIMESTAMP: DateTime.UtcNow);
        }


        /// <summary>
        /// Gets all the post by a user.
        /// </summary>
        /// <param name="boardID">The board id.</param>
        /// <param name="userID">The user id.</param>
        /// <returns> Returns all the post by a user.</returns>
        public static IOrderedEnumerable<Message> GetAllUserMessages(this IRepository<Message> repository, int userId)
        {
            return repository.Get(m => m.UserID == userId).OrderByDescending(m => m.Posted);
        }

        /// <summary>
        /// Gets all messages by board as Typed Search Message List.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="boardId">The board identifier.</param>
        /// <returns>Returns all Messages as Typed Search Message List</returns>
        public static IEnumerable<SearchMessage> GetAllMessagesByBoard(this IRepository<Message> repository, int? boardId)
        {
            CodeContracts.VerifyNotNull(repository, "repository");

            return repository.DbFunction.GetAsDataTable(cdb => cdb.message_list_search(BoardID: boardId ?? repository.BoardID))
                .SelectTypedList(t => new SearchMessage(t));
        }

        /// <summary>
        /// The message_list.
        /// </summary>
        /// <param name="messageID">
        /// The message id.
        /// </param>
        [NotNull]
        public static IEnumerable<TypedMessageList> MessageList(this IRepository<Message> repository, int messageId)
        {
            CodeContracts.VerifyNotNull(repository, "repository");

            return repository.DbFunction.GetAsDataTable(cdb => cdb.message_list(MessageID: messageId))
                .SelectTypedList(t => new TypedMessageList(t));
        }


        /// <summary>
        /// A list of messages.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="messageId">The message identifier.</param>
        /// <returns>Returns Typed Message List</returns>
        public static IList<Message> ListTyped(this IRepository<Message> repository, int messageId)
        {
            CodeContracts.VerifyNotNull(repository, "repository");

            return repository.SqlList(
                "message_list",
                new
                    {
                       MessageID = messageId
                });
        }

        /// <summary>
        /// Saves the specified message identifier.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="topicId">The topic identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="guestUserName">Name of the guest user.</param>
        /// <param name="ip">The ip.</param>
        /// <param name="posted">The posted.</param>
        /// <param name="replyTo">The reply to.</param>
        /// <param name="blogPostId">The blog post identifier.</param>
        /// <param name="flags">The flags.</param>
        /// <returns>Returns the new message identifier</returns>
        public static int Save(
            this IRepository<Medal> repository,
            int topicId,
            int userId,
            string message,
            string guestUserName,
            string ip,
            DateTime posted,
            int replyTo,
            int blogPostId,
            int flags)
        {
            CodeContracts.VerifyNotNull(repository, "repository");

            var messageId = (int)repository.DbFunction.Scalar.message_save(
                TopicID: topicId,
                UserID: userId,
                Message: message,
                UserName: guestUserName,
                IP: ip,
                Posted: posted,
                ReplyTo: replyTo,
                BlogPostID: null,
                Flags: flags,
                UTCTIMESTAMP: DateTime.UtcNow);

            repository.FireNew(messageId);

            return messageId;
        }

        /// <summary>
        /// Approves the message.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="messageId">The message identifier.</param>
        public static void ApproveMessage(this IRepository<Message> repository, int messageId)
        {
            CodeContracts.VerifyNotNull(repository, "repository");

            repository.DbFunction.Query.message_approve(MessageID: messageId);
        }

        /// <summary>
        /// Updates the flags.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="flags">The flags.</param>
        public static void UpdateFlags(this IRepository<Message> repository, int messageId, int flags)
        {
            CodeContracts.VerifyNotNull(repository, "repository");

            repository.UpdateOnly(() => new Message { Flags = flags }, where: u => u.ID == messageId);
        }

        public static DataTable SimpleListAsDataTable(this IRepository<Message> repository, [CanBeNull] int startId = 0, [CanBeNull] int limit = 500)
        {
            CodeContracts.VerifyNotNull(repository, "repository");

            return repository.DbFunction.GetData.message_simplelist(StartID: startId, Limit: limit);
        }

        /// <summary>
        /// The message_delete.
        /// </summary>
        /// <param name="messageID">
        /// The message id.
        /// </param>
        /// <param name="isModeratorChanged">
        /// The is moderator changed.
        /// </param>
        /// <param name="deleteReason">
        /// The delete reason.
        /// </param>
        /// <param name="isDeleteAction">
        /// The is delete action.
        /// </param>
        /// <param name="DeleteLinked">
        /// The delete linked.
        /// </param>
        public static void Delete(this IRepository<Message> repository, [NotNull] int messageID, bool isModeratorChanged, [NotNull] string deleteReason, int isDeleteAction, bool DeleteLinked)
        {
            repository.Delete(messageID, isModeratorChanged, deleteReason, isDeleteAction, DeleteLinked, false);
        }

        /// <summary>
        /// The message_delete.
        /// </summary>
        /// <param name="messageID">
        /// The message id.
        /// </param>
        /// <param name="isModeratorChanged">
        /// The is moderator changed.
        /// </param>
        /// <param name="deleteReason">
        /// The delete reason.
        /// </param>
        /// <param name="isDeleteAction">
        /// The is delete action.
        /// </param>
        /// <param name="DeleteLinked">
        /// The delete linked.
        /// </param>
        /// <param name="eraseMessage">
        /// The erase message.
        /// </param>
        public static void Delete(this IRepository<Message> repository,
            [NotNull] int messageID,
            bool isModeratorChanged,
            [NotNull] string deleteReason,
            int isDeleteAction,
            bool DeleteLinked,
            bool eraseMessage)
        {
           repository.DeleteRecursively(
                messageID,
                isModeratorChanged,
                deleteReason,
                isDeleteAction,
                DeleteLinked,
                false,
                eraseMessage);
        }

        /// <summary>
        /// message movind function
        /// </summary>
        /// <param name="messageID">
        /// The message id.
        /// </param>
        /// <param name="moveToTopic">
        /// The move to topic.
        /// </param>
        /// <param name="moveAll">
        /// The move all.
        /// </param>
        public static void Move(
            this IRepository<Message> repository, [NotNull] int messageID, [NotNull] int moveToTopic, bool moveAll)
        {
            repository.DbFunction.Scalar.message_move(
                MessageID: messageID,
                MoveToTopic: moveToTopic);

            if (!moveAll)
            {
                return;
            }

            // moveAll=true anyway
            // it's in charge of moving answers of moved post
            var replies = repository.Get(m => m.ReplyTo == messageID).Select(x => x.ID);

            foreach (var replyId in replies)
            {
                repository.MoveRecursively(replyId, moveToTopic);
            }
        }

        /// <summary>
        /// gets list of replies to message
        /// </summary>
        /// <param name="messageID">
        /// The message id.
        /// </param>
        /// <returns>
        /// </returns>
        [NotNull]
        public static DataTable RepliesListAsDataTable(this IRepository<Message> repository, [NotNull] int messageId)
        {
            return repository.DbFunction.GetData.message_reply_list(MessageID: messageId);
        }

        /// <summary>
        /// The message_list.
        /// </summary>
        /// <param name="messageID">
        /// The message id.
        /// </param>
        /// <returns>
        /// </returns>
        public static DataTable ListAsDataTable(this IRepository<Message> repository, [NotNull] int messageId)
        {
            return repository.DbFunction.GetData.message_list(MessageID: messageId);
        }



        /// <summary>
        /// Finds the Unread Message
        /// </summary>
        /// <param name="topicID">The topic id.</param>
        /// <param name="messageId">The message Id.</param>
        /// <param name="lastRead">The last read.</param>
        /// <param name="showDeleted">The show Deleted.</param>
        /// <param name="authorUserID">The author User ID.</param>
        /// <returns></returns>
        public static DataTable FindUnreadAsDataTable(
            this IRepository<Message> repository,
            [NotNull] int topicId,
            [NotNull] int messageId,
            [NotNull] DateTime lastRead,
            [NotNull] bool showDeleted,
            [NotNull] int authorUserId)
        {
            // Make sure there are no more DateTime.MinValues coming from db.
            if (lastRead == DateTime.MinValue)
            {
                lastRead = DateTimeHelper.SqlDbMinTime();
            }

            return repository.DbFunction.GetData.message_findunread(
                TopicID: topicId,
                MessageID: messageId,
                MinDateTime: DateTimeHelper.SqlDbMinTime().AddYears(-1),
                LastRead: lastRead,
                ShowDeleted: showDeleted,
                AuthorUserID: authorUserId);
        }

        /// <summary>
        /// Retrieve all reported messages with the correct forumID argument.
        /// </summary>
        /// <param name="forumID">
        /// The forum id.
        /// </param>
        /// <returns>
        /// </returns>
        public static DataTable ListReportedAsDataTable(this IRepository<Message> repository, [NotNull] int forumId)
        {
            return repository.DbFunction.GetData.message_listreported(ForumID: forumId);
        }

        /// <summary>
        /// Here we get reporters list for a reported message
        /// </summary>
        /// <param name="messageID">
        /// The message ID.
        /// </param>
        /// <returns>
        /// Returns reporters DataTable for a reported message.
        /// </returns>
        public static DataTable ListReportersAsDataTable(
            this IRepository<Message> repository, int messageId)
        {
            return repository.DbFunction.GetData.message_listreporters(MessageID: messageId, UserID: 0);
        }

        /// <summary>
        /// The message_listreporters.
        /// </summary>
        /// <param name="messageID">
        /// The message id.
        /// </param>
        /// <param name="userID">
        /// The user id.
        /// </param>
        /// <returns>
        /// </returns>
        public static DataTable ListReportersAsDataTable(
            this IRepository<Message> repository, int messageId, [NotNull] int userId)
        {
            return repository.DbFunction.GetData.message_listreporters(MessageID: messageId, UserID: userId);
        }

        // <summary> Save reported message back to the database. </summary>
        /// <summary>
        /// The message_report.
        /// </summary>
        /// <param name="messageID">
        /// The message id.
        /// </param>
        /// <param name="userID">
        /// The user id.
        /// </param>
        /// <param name="reportedDateTime">
        /// The reported date time.
        /// </param>
        /// <param name="reportText">
        /// The report text.
        /// </param>
        public static void Report(
            this IRepository<Message> repository,
            [NotNull] int messageId,
            [NotNull] int userId,
            [NotNull] DateTime reportedDateTime,
            [NotNull] string reportText)
        {
            repository.DbFunction.Scalar.message_report(
                MessageID: messageId,
                ReporterID: userId,
                ReportedDate: reportedDateTime,
                ReportText: reportText,
                UTCTIMESTAMP: DateTime.UtcNow);
        }

        /// <summary>
        /// Copy current Message text over reported Message text. </summary>
        /// <summary>
        /// <param name="messageID">
        /// The message id.
        /// </param>
        public static void ReportCopyOver(
            this IRepository<Message> repository, [NotNull] int messageId)
        {
            repository.DbFunction.Scalar.message_reportcopyover(MessageID: messageId);
        }

        /// <summary>
        /// Copy current Message text over reported Message text. </summary>
        /// <summary>
        /// <param name="messageFlag">
        /// The message flag.
        /// </param>
        /// <param name="messageID">
        /// The message id.
        /// </param>
        /// <param name="userID">
        /// The user id.
        /// </param>
        public static void ReportResolve(
            this IRepository<Message> repository, [NotNull] int messageFlag, [NotNull] int messageId, [NotNull] int userId)
        {
            repository.DbFunction.Scalar.message_reportresolve(
                MessageFlag: messageFlag,
                MessageID: messageId,
                UserID: userId,
                UTCTIMESTAMP: DateTime.UtcNow);
        }

        /// <summary>
        /// The message_save.
        /// </summary>
        /// <param name="topicID">
        /// The topic id.
        /// </param>
        /// <param name="userID">
        /// The user id.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="guestUserName">
        /// The guest user name.
        /// </param>
        /// <param name="ip">
        /// The ip.
        /// </param>
        /// <param name="posted">
        /// The posted.
        /// </param>
        /// <param name="replyTo">
        /// The reply to.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <param name="messageID">
        /// The message id.
        /// </param>
        /// <returns>
        /// The message_save.
        /// </returns>
        public static bool Save(
            this IRepository<Message> repository,
            [NotNull] long topicId,
            [NotNull] int userId,
            [NotNull] string message,
            [NotNull] string guestUserName,
            [NotNull] string ip,
            [NotNull] DateTime posted,
            [NotNull] int replyTo,
            [NotNull] int flags,
            ref long messageID)
        {

            IDbDataParameter parameterMessage = null;

            repository.SqlList(
                "message_save",
                cmd =>
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.AddParam("TopicID", topicId);
                        cmd.AddParam("UserID", userId);
                        cmd.AddParam("Message", message);
                        cmd.AddParam("UserName", guestUserName);
                        cmd.AddParam("IP", ip);
                        cmd.AddParam("Posted", posted);
                        cmd.AddParam("ReplyTo", replyTo);
                        cmd.AddParam("BlogPostID", null);
                        cmd.AddParam("Flags", flags);
                        cmd.AddParam("UTCTIMESTAMP", DateTime.UtcNow);

                        parameterMessage = cmd.AddParam("MessageID", direction: ParameterDirection.Output);
                    });

            messageID = parameterMessage.Value.ToType<long>();

            return true;
        }

        /// <summary>
        /// Returns message data based on user access rights
        /// </summary>
        /// <param name="MessageID">
        /// The Message Id.
        /// </param>
        /// <param name="pageUserId">
        /// The page User Id.
        /// </param>
        /// <returns>
        /// </returns>
        public static DataTable SecAsDataTable(
            this IRepository<Message> repository, int messageId, [NotNull] int pageUserId)
        {
            return repository.DbFunction.GetData.message_secdata(
                PageUserID: pageUserId,
                MessageID: messageId);
        }

        /// <summary>
        /// The message_unapproved.
        /// </summary>
        /// <param name="forumID">
        /// The forum id.
        /// </param>
        /// <returns>
        /// </returns>
        public static DataTable UnapprovedAsDataTable(
            this IRepository<Message> repository, [NotNull] int forumId)
        {
            return repository.DbFunction.GetData.message_unapproved(
                ForumID: forumId);
        }

        /// <summary>
        /// The message_update.
        /// </summary>
        /// <param name="messageID">
        /// The message id.
        /// </param>
        /// <param name="priority">
        /// The priority.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="description">
        /// The description.
        /// </param>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <param name="subject">
        /// The subject.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <param name="reasonOfEdit">
        /// The reason of edit.
        /// </param>
        /// <param name="isModeratorChanged">
        /// The is moderator changed.
        /// </param>
        /// <param name="overrideApproval">
        /// The override approval.
        /// </param>
        /// <param name="originalMessage">
        /// The original Message.
        /// </param>
        /// <param name="editedBy">
        /// UserId of who edited the message.
        /// </param>
        public static void Update(
            this IRepository<Message> repository,
            [NotNull] int messageId,
            [NotNull] int priority,
            [NotNull] string message,
            [NotNull] string description,
            [CanBeNull] string status,
            [CanBeNull] string styles,
            [NotNull] string subject,
            [NotNull] int flags,
            [NotNull] string reasonOfEdit,
            [NotNull] bool isModeratorChanged,
            [NotNull] bool? overrideApproval,
            [NotNull] string originalMessage,
            [NotNull] int editedBy)
        {
            repository.DbFunction.Scalar.message_update(
                MessageID: messageId,
                Priority: priority,
                Message: message,
                Description: description,
                Status: status,
                Styles: styles,
                Subject: subject,
                Flags: flags,
                Reason: reasonOfEdit,
                EditedBy: editedBy,
                IsModeratorChanged: isModeratorChanged,
                OverrideApproval: overrideApproval,
                OriginalMessage: originalMessage,
                CurrentUtcTimestamp: DateTime.UtcNow);
        }

        /// <summary>
        /// The messagehistory_list.
        /// </summary>
        /// <param name="messageId">
        /// The Message ID.
        /// </param>
        /// <param name="daysToClean">
        /// Days to clean.
        /// </param>
        /// <returns>
        /// List of all message changes.
        /// </returns>
        public static DataTable HistoryListAsDataTable(
            this IRepository<Message> repository, int messageId, int daysToClean)
        {
            return repository.DbFunction.GetData.messagehistory_list(
                MessageID: messageId,
                DaysToClean: daysToClean,
                UTCTIMESTAMP: DateTime.UtcNow);
        }

        #endregion

        /// <summary>
        /// The message_delete recursively.
        /// </summary>
        /// <param name="messageID">
        /// The message id.
        /// </param>
        /// <param name="isModeratorChanged">
        /// The is moderator changed.
        /// </param>
        /// <param name="deleteReason">
        /// The delete reason.
        /// </param>
        /// <param name="isDeleteAction">
        /// The is delete action.
        /// </param>
        /// <param name="deleteLinked">
        /// The delete linked.
        /// </param>
        /// <param name="isLinked">
        /// The is linked.
        /// </param>
        /// <param name="eraseMessages">
        /// The erase messages.
        /// </param>
        private static void DeleteRecursively(
            this IRepository<Message> repository,
            [NotNull] int messageID,
            bool isModeratorChanged,
            [NotNull] string deleteReason,
            int isDeleteAction,
            bool deleteLinked,
            bool isLinked,
            bool eraseMessages)
        {
            var useFileTable = YafContext.Current.Get<YafBoardSettings>().UseFileTable;

            if (deleteLinked)
            {
                // Delete replies
                var replies = repository.Get(m => m.ReplyTo == messageID).Select(x => x.ID);

                foreach (var replyId in replies)
                {
                    repository.DeleteRecursively(
                        replyId,
                        isModeratorChanged,
                        deleteReason,
                        isDeleteAction,
                        true,
                        true,
                        eraseMessages);
                }
            }

            // If the files are actually saved in the Hard Drive
            if (!useFileTable)
            {
                var attachments = repository.DbFunction.GetData.attachment_list(MessageID: messageID);

                var uploadDir =
                    HostingEnvironment.MapPath(
                        string.Concat(BaseUrlBuilder.ServerFileRoot, YafBoardFolders.Current.Uploads));

                foreach (DataRow row in attachments.Rows)
                {
                    try
                    {
                        var fileName = string.Format("{0}/{1}.{2}.yafupload", uploadDir, messageID, row["FileName"]);

                        if (File.Exists(fileName))
                        {
                            File.Delete(fileName);
                        }
                    }
                    catch
                    {
                        // error deleting that file...
                    }
                }
            }

            // Ederon : erase message for good
            if (eraseMessages)
            {
                repository.DbFunction.Scalar.message_delete(
                    MessageID: messageID,
                    EraseMessage: eraseMessages);
            }
            else
            {
                // Delete Message
                // undelete function added
                repository.DbFunction.Scalar.message_deleteundelete(
                    MessageID: messageID,
                    isModeratorChanged: isModeratorChanged,
                    DeleteReason: deleteReason,
                    isDeleteAction: isDeleteAction);
            }
        }


        /// <summary>
        /// moves answers of moved post
        /// </summary>
        /// <param name="messageID">
        /// The message id.
        /// </param>
        /// <param name="moveToTopic">
        /// The move to topic.
        /// </param>
        private static void MoveRecursively(
            this IRepository<Message> repository, [NotNull] int messageID, [NotNull] int moveToTopic)
        {
            var replies = repository.Get(m => m.ReplyTo == messageID).Select(x => x.ID);

            foreach (var replyId in replies)
            {
                repository.MoveRecursively(replyId, moveToTopic);
            }

            repository.DbFunction.Scalar.message_move(
                MessageID: messageID,
                MoveToTopic: moveToTopic);
        }
    }
}