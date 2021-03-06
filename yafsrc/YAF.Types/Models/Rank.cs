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
namespace YAF.Types.Models
{
    #region Using

    using System;

    using ServiceStack.DataAnnotations;

    using YAF.Types.Interfaces;
    using YAF.Types.Interfaces.Data;

    #endregion

    /// <summary>
    ///     A class which represents the YAF_Rank table in the YAF Database.
    /// </summary>
    [Serializable]

    [UniqueConstraint(nameof(BoardID), nameof(Name))]
    public partial class Rank : IEntity, IHaveID, IHaveBoardID
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Rank" /> class.
        /// </summary>
        public Rank()
        {
            this.OnCreated();
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the Rank ID.
        /// </summary>
        [AutoIncrement]
        [Alias("RankID")]
        public int ID { get; set; }

        [References(typeof(Board))]
        [Required]
        public int BoardID { get; set; }
        [Required]
        public string Name { get; set; }
        public int? MinPosts { get; set; }
        public string RankImage { get; set; }
        [Required]
        [Default(0)]
        public int Flags { get; set; }

        [Default(0)]
        public int? PMLimit { get; set; }
        public string Style { get; set; }
        [Required]
        [Default(0)]
        public short SortOrder { get; set; }
        public string Description { get; set; }
        [Required]
        [Default(0)]
        public int UsrSigChars { get; set; }
        public string UsrSigBBCodes { get; set; }
        public string UsrSigHTMLTags { get; set; }
        [Required]
        [Default(0)]
        public int UsrAlbums { get; set; }
        [Required]
        [Default(0)]
        public int UsrAlbumImages { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///     The on created.
        /// </summary>
        partial void OnCreated();

        #endregion
    }
}