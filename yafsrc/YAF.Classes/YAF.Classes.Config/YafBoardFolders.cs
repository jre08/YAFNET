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

namespace YAF.Classes
{
    using YAF.Classes.Pattern;

    /// <summary>
  /// The YAF board folders.
  /// </summary>
  public class YafBoardFolders
  {
    /// <summary>
    /// Gets Current.
    /// </summary>
    public static YafBoardFolders Current => PageSingleton<YafBoardFolders>.Instance;

    /// <summary>
    /// Gets BoardFolder.
    /// </summary>
    public string BoardFolder =>
        Config.MultiBoardFolders
            ? string.Format(Config.BoardRoot + "{0}/", YafControlSettings.Current.BoardID)
            : Config.BoardRoot;

    /// <summary>
    /// Gets Uploads.
    /// </summary>
    public string Uploads => string.Concat(this.BoardFolder, "Uploads");

    /// <summary>
    /// Gets Images.
    /// </summary>
    public string Images => string.Concat(this.BoardFolder, "Images");

    /// <summary>
    /// Gets Avatars.
    /// </summary>
    public string Avatars => string.Concat(this.BoardFolder, "Images/Avatars");

    /// <summary>
    /// Gets Categories.
    /// </summary>
    public string Categories => string.Concat(this.BoardFolder, "Images/Categories");

    /// <summary>
    /// Gets Categories.
    /// </summary>
    public string Forums => string.Concat(this.BoardFolder, "Images/Forums");

    /// <summary>
    /// Gets Medals.
    /// </summary>
    public string Medals => string.Concat(this.BoardFolder, "Images/Medals");

    /// <summary>
    /// Gets Ranks.
    /// </summary>
    public string Ranks => string.Concat(this.BoardFolder, "Images/Ranks");
  }
}