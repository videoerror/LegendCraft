﻿// Copyright(C) <2012>  <Jon Baker, Glenn Mariën and Lao Tszy>

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

// Copyright(C) <2012> Jon Baker(http://au70.net)

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

namespace fCraft.Commands.CommandHandlers {

	public class VoteHandler {

		public static string Usage;

		public static int VotedYes;
		public static int VotedNo;

		public static List<Player> Voted;

		public static string VoteStarter;

		public static bool VoteIsOn = false;

		public static string VoteKickReason;
		public static string TargetName;
		public static string Question;

		public static void NewVote() {
			Usage = "&A/Vote Yes | No | Ask | Abort";

			VotedYes = 0;
			VotedNo = 0;

			Voted = new List<Player>();

			VoteIsOn = true;
		}

		public static void VoteParams(Player player, Command cmd) {
			string option = cmd.Next();

			if(option == null) {
				MessageVoteStatus(player);

				return;
			}

			switch(option) {
				default:
					MessageVoteStatus(player);

					break;
				
				case "abort":
				
				case "stop":
					if(!VoteIsOn) {
						player.Message("No vote is currently running");

						return;
					}
					
					if(!player.Can(Permission.MakeVotes)) {
						player.Message("You do not have Permission to abort votes");

						return;
					}

					VoteIsOn = false;

					foreach(Player V in Voted) {
						if(V.Info.HasVoted) {
							V.Info.HasVoted = false;
						}

						V.Message("Your vote was cancelled");
					}

					Voted.Clear();

					TargetName = null;

					Server.Players.Message("{0} &Saborted the vote.", player.ClassyName);

					break;
				
				case "yes":
					if(!VoteIsOn) {
						player.Message("No vote is currently running");

						return;
					}

					if(player.Info.HasVoted) {
						player.Message("&CYou have already voted");

						return;
					}

					Voted.Add(player);

					VotedYes++;

					player.Info.HasVoted = true;

					player.Message("&8You have voted for 'Yes'");

					break;
				
				case "no":
					if(!VoteIsOn) {
						player.Message("No vote is currently running");

						return;
					}

					if(player.Info.HasVoted) {
						player.Message("&CYou have already voted");

						return;
					}

					VotedNo++;

					Voted.Add(player);

					player.Info.HasVoted = true;

					player.Message("&8You have voted for 'No'");

					break;
				
				case "ask":
					string AskQuestion = cmd.NextAll();

					Question = AskQuestion;

					if(!player.Can(Permission.MakeVotes)) {
						player.Message("You do not have permissions to ask a question");

						return;
					}

					if(VoteIsOn) {
						player.Message("A vote has already started. Each vote lasts 1 minute.");

						return;
					}

					if(Question.Length < 5) {
						player.Message("Invalid question");

						return;
					}

					NewVote();

					VoteStarter = player.ClassyName;

					Server.Players.Message("{0}&S Asked: {1}", player.ClassyName, Question);

					Server.Players.Message("&9Vote now! &S/Vote &AYes &Sor /Vote &CNo");

					VoteIsOn = true;

					Scheduler.NewTask(VoteCheck).RunOnce(TimeSpan.FromMinutes(1));

					break;
			}
		}

		private static void MessageVoteStatus(Player player) {
			if(!VoteIsOn) {
				player.Message("No vote is currently running.");
			} else if(VoteKickReason == null) {
				player.Message("Current question: {0}&C asked: {1}", VoteStarter, Question);
			} else {
				player.Message("&CCurrent VoteKick for {0}&C, reason: {1}", TargetName, VoteKickReason);
			}

			player.Message(Usage);
		}
		
		private static void VoteCheck(SchedulerTask task) {
			if(!VoteIsOn) {
				return;
			}

			Server.Players.Message("{0}&S Asked: {1} \n&SResults are in! Yes: &A{2} &SNo: &C{3}",
								   VoteStarter, Question, VotedYes, VotedNo);

			VoteIsOn = false;

			foreach(Player V in Voted) {
				V.Info.HasVoted = false;
			}
		}
	}
}
