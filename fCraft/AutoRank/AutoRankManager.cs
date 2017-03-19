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
using System.Text;
using System.Xml;
using System.Xml.Linq;

using fCraft;

namespace fCraft.AutoRank {

	public static class AutoRankManager {

		public static List<Condition> ConditionList = new List<Condition>();

		public static void Load() {
			try {
				if(!File.Exists(Paths.AutoRankFileName)) {
					// Autorank was never set up.
					return;
				}

				XDocument doc = XDocument.Load(Paths.AutoRankFileName);
				XElement docConfig = doc.Root;

				// Load each rank change.
				foreach(XElement mainElement in docConfig.Elements()) {
					string startingRank = mainElement.Name.ToString().Split('-')[0];
					string endingRank = mainElement.Name.ToString().Split('-')[1];

					foreach(XAttribute condition in mainElement.Attributes()) {
						string cond = condition.Name.ToString();
						string op = GetOperator(condition.Value);
						string value = GetValue(condition.Value);

						Condition generatedCondition = new Condition(startingRank, endingRank, cond, op, value);

						ConditionList.Add(generatedCondition);
					}

					// Load each condition in each rank change.
					foreach(XAttribute conditional in mainElement.Attributes()) {
						// Load the operator and the value.
					}
				}
			} catch(Exception ex) {
				if(ex is IndexOutOfRangeException) {
					//Chill, autorank is just empty and stuff.
				} else {
					Logger.LogToConsole(ex.Message + " " + ex.Data);
				}
			}
		}

		public static string GetOperator(string str) {
			string returnString = String.Empty;

			if(str.Contains("=/=")) {
				returnString = str.Substring(0, 3);

				return returnString;
			}

			if(str.Contains(">=") || str.Contains("<=")) {
				returnString = str.Substring(0, 2);
				return returnString;
			} else {
				returnString = str.Substring(0, 1);

				return returnString;
			}
		}

		public static string GetValue(string str) {
			string returnString = String.Empty;

			if(str.Contains("=/=")) {
				returnString = str.Substring(3);

				return returnString;
			}

			if(str.Contains(">=") || str.Contains("<=")) {
				returnString = str.Substring(2);

				return returnString;
			} else {
				returnString = str.Substring(1);

				return returnString;
			}
		}

		/// <summary>
		/// Check if a player is eligible for an autorank promotion/demotion.
		/// </summary>
		public static void Check(Player player) {
			// Loop through every condition in the conditions list.
			foreach(Condition condTest in ConditionList) {
				// Make sure the player's rank matches up with any of the starting ranks for each conditions.
				if(player.Info.Rank.Name == condTest.StartingRank) {
					bool rankUp = true;

					// Loop through each AND item in the condition, if one AND item is not met, player will not rank up.
					foreach(var item in condTest.Conditions) {
						// Type of condition.
						switch(item.Key) {
							case "Since_First_Login":
								if(!item.Value.Item1.Operator(player.Info.TimeSinceFirstLogin.Days, item.Value.Item2)) {
									rankUp = false;
								}

								break;
							
							case "Since_Last_Login":
								if(!item.Value.Item1.Operator(player.Info.TimeSinceLastLogin.Days, item.Value.Item2)) {
									rankUp = false;
								}

								break;
							
							case "Last_Seen":
								if(!item.Value.Item1.Operator(player.Info.TimeSinceLastLogin.Days, item.Value.Item2)) {
									rankUp = false;
								}

								break;
							
							case "Total_Time":
								if(!item.Value.Item1.Operator((long)player.Info.TotalTime.TotalHours, item.Value.Item2)) {
									rankUp = false;
								}

								break;
							
							case "Blocks_Built":
								if(!item.Value.Item1.Operator(player.Info.BlocksBuilt, item.Value.Item2)) {
									rankUp = false;
								}

								break;
							
							case "Blocks_Deleted":
								if(!item.Value.Item1.Operator(player.Info.BlocksDeleted, item.Value.Item2)) {
									rankUp = false;
								}

								break;
							
							case "Blocks_Changed":
								if(!item.Value.Item1.Operator(player.Info.BlocksBuilt + player.Info.BlocksDeleted + player.Info.BlocksDrawn, item.Value.Item2)) {
									rankUp = false;
								}

								break;
							
							case "Blocks_Drawn":
								if(!item.Value.Item1.Operator(player.Info.BlocksDrawn, item.Value.Item2)) {
									rankUp = false;
								}

								break;
							
							case "Visits":
								if(!item.Value.Item1.Operator(player.Info.TimesVisited, item.Value.Item2)) {
									rankUp = false;
								}

								break;
							
							case "Messages":
								if(!item.Value.Item1.Operator(player.Info.MessagesWritten, item.Value.Item2)) {
									rankUp = false;
								}

								break;
							
							case "Times_Kicked":
								if(!item.Value.Item1.Operator(player.Info.TimesKicked, item.Value.Item2)) {
									rankUp = false;
								}

								break;
							
							case "Since_Rank_Change":
								if(!item.Value.Item1.Operator(player.Info.TimeSinceRankChange.Days, item.Value.Item2)) {
									rankUp = false;
								}

								break;
							
							case "Since_Last_Kick":
								if(!item.Value.Item1.Operator(player.Info.TimeSinceLastKick.Days, item.Value.Item2)) {
									rankUp = false;
								}

								break;
							
							default:
								// Shouldn't happen.

								break;
						}

					}

					if(rankUp) {
						player.Info.ChangeRank(Player.Console, Rank.Parse(condTest.EndingRank), "AutoRank System", true, true, true);
					}
				}
			}
		}

		/// <summary>
		/// Parse a string into a bool like a boss.
		/// </summary>
		public static bool Operator(this string str, long x, long y) {
			switch(str) {
				case ">":
					return x > y;
					
				case "<":
					return x < y;
				
				case "=":
					return x == y;
				
				case ">=":
					return x >= y;
				
				case "<=":
					return x <= y;
				
				case "=/=":
					return x != y;
				
				default:
					throw new Exception("Error: Unable to parse AutoRank logic. Make sure that Autorank.xml is not damaged or corrupted!: " + str);
			}
		}
	}
}
