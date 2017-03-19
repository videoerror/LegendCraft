﻿// Copyright 2009-2012 Matvei Stefarov <me@matvei.org>

#region LICENCE
/*
Copyright 2017 video_error

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

	http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
 */
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Diagnostics;

using JetBrains.Annotations;

// Legacy autorank support for fCraft.
namespace fCraft.AutoRank {

	public static class fCraftAutoManager {

		internal static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(60);

		public static readonly List<fCraftCriterion> Criteria = new List<fCraftCriterion>();

		public const string TagName = "fCraftAutoRankConfig";

		/// <summary>
		/// Whether any criteria are defined.
		/// </summary>
		public static bool HasCriteria {
			get {
				return Criteria.Count > 0;
			}
		}

		/// <summary>
		/// Adds a new criterion to the list. Throws an ArgumentException on duplicates.
		/// </summary>
		public static void Add([NotNull] fCraftCriterion criterion) {
			if(criterion == null) {
				throw new ArgumentNullException("criterion");
			}

			if(Criteria.Contains(criterion)) {
				throw new ArgumentException("This criterion has already been added.");
			}

			Criteria.Add(criterion);
		}


		/// <summary> Checks whether a given player is due for a promotion or demotion. </summary>
		/// <param name="info"> PlayerInfo to check. </param>
		/// <returns> Null if no rank change is needed, or a rank to promote/demote too. </returns>
		[CanBeNull]
		public static Rank Check([NotNull] PlayerInfo info) {
			if(info == null) {
				throw new ArgumentNullException("info");
			}

			// ReSharper disable LoopCanBeConvertedToQuery.
			for(int i = 0; i < Criteria.Count; i++) {
				if(Criteria[i].FromRank == info.Rank &&
				   !info.IsBanned && Criteria[i].Condition.Eval(info)) {
					return Criteria[i].ToRank;
				}
			}

			// ReSharper restore LoopCanBeConvertedToQuery.
			return null;
		}

		// Rank everyone up.
		internal static void TaskCallback(SchedulerTask schedulerTask) {
			if(!ConfigKey.AutoRankEnabled.Enabled()) {
				return;
			}

			PlayerInfo[] onlinePlayers = Server.Players.Select(p => p.Info).ToArray();

			DoAutoRankAll(Player.Console, onlinePlayers, "AutoRank System", true);
		}

		internal static void DoAutoRankAll([NotNull] Player player, [NotNull] PlayerInfo[] list, string message, bool auto) {
			if(player == null) {
				throw new ArgumentNullException("player");
			}

			if(list == null) {
				throw new ArgumentNullException("list");
			}

			if(!HasCriteria) {
				if(!auto) {
					player.Message("AutoRankAll: No criteria found.");
				}

				return;
			}

			if(!auto) {
				player.Message("AutoRankAll: Evaluating {0} players...", list.Length);
			}

			Stopwatch sw = Stopwatch.StartNew();

			int promoted = 0, demoted = 0;

			for(int i = 0; i < list.Length; i++) {
				Rank newRank = Check(list[i]);

				if(newRank != null) {
					if(newRank > list[i].Rank) {
						promoted++;
					} else if(newRank < list[i].Rank) {
						demoted++;
					}

					try {
						list[i].ChangeRank(player, newRank, message, true, true, true);
					} catch(PlayerOpException ex) {
						if(auto) {
							Logger.Log(LogType.Error, "AutoRank: Could not change player's rank: {0}", ex.Message);
						} else {
							player.Message(ex.MessageColored);
						}
					}
				}
			}

			sw.Stop();

			String resultMsg = String.Format("AutoRankAll: Worked for {0}ms, {1} players promoted, {2} demoted.",
											 sw.ElapsedMilliseconds,
											 promoted,
											 demoted);

			if(auto) {
				if(promoted > 0 || demoted > 0) {
					Logger.Log(LogType.SystemActivity, resultMsg);
				}
			} else {
				player.Message(resultMsg);
			}
		}	


		public static bool Init() {
			Criteria.Clear();

			if(File.Exists("ref/fCraftAutorank.xml")) {
				try {
					XDocument doc = XDocument.Load("ref/fCraftAutorank.xml");

					if(doc.Root == null) {
						return false;
					}

					foreach(XElement el in doc.Root.Elements("Criterion")) {
						try {
							Add(new fCraftCriterion(el));
						} catch(Exception ex) {
							Logger.Log(LogType.Error,
									   "AutoRank.Init: Could not parse an AutoRank criterion: {0}", ex);
						}
					}

					if(Criteria.Count == 0) {
						Logger.Log(LogType.Warning, "AutoRank.Init: No criteria loaded.");
					}

					return true;
				} catch(Exception ex) {
					Logger.Log(LogType.Error,
							   "AutoRank.Init: Could not parse the AutoRank file: {0}", ex);

					return false;
				}
			} else {
				Logger.Log(LogType.Warning, "AutoRank.Init: fCraftAutorank.xml not found. No criteria loaded.");

				return false;
			}
		}
	}

	#region ENUMS

	/// <summary>
	/// Operators used to compare PlayerInfo fields.
	/// </summary>
	public enum ComparisonOp {

		/// <summary>
		/// Equals to.
		/// </summary>
		Eq,
		/// <summary>
		/// Not Equal to.
		/// </summary>
		Neq,
		/// <summary>
		/// Greater than.
		/// </summary>
		Gt,
		/// <summary>
		/// Greater than or equal.
		/// </summary>
		Gte,
		/// <summary>
		/// Less than.
		/// </summary>
		Lt,
		/// <summary>
		/// Less than or equal.
		/// </summary>
		Lte
	}

	/// <summary>
	/// Enumeration of quantifiable PlayerInfo fields(or field combinations) that may be used with AutoRank conditions.
	/// </summary>
	public enum ConditionField {

		/// <summary>
		/// Time since first login(first time the player connected), in seconds.
		/// For players who have been entered into PlayerDB but have never logged in, this is a huge value.
		/// </summary>
		TimeSinceFirstLogin,
		/// <summary>
		/// Time since most recent login, in seconds.
		/// For players who have been entered into PlayerDB but have never logged in, this is a huge value.
		/// </summary>
		TimeSinceLastLogin,
		/// <summary>
		/// Time since player was last seen(0 if the player is online, otherwise time since last logout, in seconds).
		/// For players who have been entered into PlayerDB but have never logged in, this is a huge value.
		/// </summary>
		LastSeen,
		/// <summary>
		/// Total time spent on the server(including current session) in seconds.
		/// For players who have been entered into PlayerDB but have never logged in, this is 0.
		/// </summary>
		TotalTime,
		/// <summary>
		/// Number of blocks that were built manually(by clicking).
		/// Does not include drawn or pasted blocks.
		/// </summary>
		BlocksBuilt,
		/// <summary>
		/// Number of blocks deleted manually(by clicking).
		/// Does not include drawn or cut blocks.
		/// </summary>
		BlocksDeleted,
		/// <summary>
		/// Number of blocks changed(built + deleted) manually(by clicking).
		/// Does not include drawn or cut/paste blocks.
		/// </summary>
		BlocksChanged,
		/// <summary>
		/// Number of blocks affected by drawing commands, replacement, and cut/paste.
		/// </summary>
		BlocksDrawn,
		/// <summary>
		/// Number of separate visits/sessions on this server.
		/// </summary>
		TimesVisited,
		/// <summary>
		/// Number of messages written in chat.
		/// Includes normal chat, PMs, rank chat, /Staff, /Say, and /Me messages.
		/// </summary>
		MessagesWritten,
		/// <summary>
		/// Number of times kicked by other players or by console.
		/// Does not include any kind of automated kicks(AFK kicks, anti-grief or anti-spam, server shutdown, etc).
		/// </summary>
		TimesKicked,
		/// <summary>
		/// Time since last promotion or demotion, in seconds.
		/// For new players(who still have the default rank) this is a huge value.
		/// </summary>
		TimeSinceRankChange,
		/// <summary>
		/// Time since the player has been kicked by other players or by console.
		/// Does not reset from any kind of automated kicks(AFK kicks, anti-grief or anti-spam, server shutdown, etc).
		/// </summary>
		TimeSinceLastKick
	}
	#endregion
}
