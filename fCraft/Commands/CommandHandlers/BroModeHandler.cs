// Copyright(C) <2012>  <Jon Baker, Glenn Mariën and Lao Tszy>

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

// Copyright(C) <2012> Glenn Mariën(http://project-vanilla.com)

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

	internal class BroMode {

		private static BroMode Instance;

		private static List<String> BroNames;

		private static Dictionary<int, Player> RegisteredBroNames;

		private static int NamesRegistered = 0;

		public static bool Active = false;

		private BroMode() {
			// Empty, singleton
			// single
			// single like glennmr?
		}

		public static BroMode GetInstance() {
			if(Instance == null) {
				Instance = new BroMode();

				BroNames = new List<string>() {
					"Brozo the Clown",
					"Rag and Brone",
					"Breau Brummel",
					"Brole Porter",
					"Flannery Bro'Connor",
					"Angelina Brolie",
					"Marco Brolo",
					"Placido Bromingo",
					"Brony Seikaly",
					"Vincent Van Brogh",
					"Brodhistiva",
					"Sandy Broufax",
					"Brosef Stalin",
					"Brojohsephat",
					"Lebrona Helmsley",
					"Tom Brolicchio",
					"Brohan Santana",
					"Brobi-Wan Kenobi",
					"Haley Broel Osment",
					"Brometheus",
					"Fidel Castbro",
					"Broul Castbro",
					"Leonid Brozhnev",
					"Brotello Putin Brodimir Brodimirovich <tm>",
					"Brangela Merkel",
					"Brovio Brobrusconi",
					"Brol Pot",
					"Elvis Costellbro",
					"Amy Broehler",
					"Stephen Brolbert",
					"Nabroleon Bronaparte",
					"Broliver Cromwell",
					"Evander Brolyfield",
					"Mario Brotali",
					"Brophia Loren",
					"David Brohansen",
					"Terrell Browens",
					"Tony Bromo",
					"Braubert",
					"Pete Brose",
					"Brony Soprano",
					"Jonathan Safran Broer",
					"Alex Brovechkin",
					"Bro Jackson",
					"Bropher Grace",
					"Renzo Pianbro",
					"Santiagbro Calatrava",
					"Broam Chomsky",
					"Evelyn Brah",
					"Bronus Wagner",
					"Brad Brohaus",
					"Giorgibro Armani",
					"Al Brolson",
					"Greg Brostertag",
					"Emilibro Estevez",
					"Paul Bro Bryant",
					"Pablo Picassbro",
					"Broto Baggins",
					"Diegbro Velazqeuz",
					"Larry",
					"Bromar Sharif",
					"Willem Dabroe",
					"Brolden Caulfield",
					"Broni Mitchell",
					"Truman Cabrote",
					"John Broltrane",
					"Broman Brolanski",
					"Gary Broldman",
					"Teddy Broosevelt",
					"Marilyn Monbroe",
					"Charles Brokowski",
					"Rimbraud",
					"Brogi Berra",
					"Czeslaw Mibroscz",
					"Paul Brauguin",
					"Tim Tebro",
					"Edgar Allen Bro",
					"Christopher Brolumbus",
					"Norah Brones",
					"Brofessor X",
					"Brofiteroles",
					"Rice o Broni",
					"Pete Brozelle",
					"The Sultan of Bronei",
					"C-3PBro",
					"Brodhisattva",
					"Brohsaphat",
					"Gandalf",
					"Bro Chi Minh",
					"Dirk Diggler",
					"Brodo Baggins",
					"Bromer Simpson",
					"Grady Sizemore",
					"Helmut Brohl",
					"Foghorn Leghorn",
					"Brobespierre",
					"Nicolas Sarbrozy",
					"Sherlock Brolmes",
					"John Brolmes",
					"Coolibro",
					"Broco Crisp",
					"Broald Dahl",
					"Bronan the Brahbarian",
					"Bro Derek",
					"Mr. Brojangles",
					"Bro Diddley",
					"Yo-Yo Brah",
					"BrO. J. Simpson",
					"Mephistophbroles",
					"Wolfgang Amadeus Brozart",
					"G.I. Bro",
					"Brosama bin Laden",
					"Magnetbro"
				};

				RegisteredBroNames = new Dictionary<int, Player>();

				Player.Disconnected += new EventHandler<Events.PlayerDisconnectedEventArgs>(PlayerDisconnected);

				Player.Connected += new EventHandler<Events.PlayerConnectedEventArgs>(PlayerConnected);
			}

			return Instance;
		}

		private static void PlayerConnected(object sender, Events.PlayerConnectedEventArgs e) {
			if(Active) {
				BroMode.GetInstance().RegisterPlayer(e.Player);
			}
		}

		private static void PlayerDisconnected(object sender, Events.PlayerDisconnectedEventArgs e) {
			if(Active) {
				BroMode.GetInstance().UnregisterPlayer(e.Player);
			}
		}

		public void RegisterPlayer(Player player) {
			if(!player.Info.IsWarned) {
				if(!player.Info.IsMuted) {
					if(!player.Info.IsFrozen) {
						try {
							if(NamesRegistered < BroNames.Count) {
								Random randomizer = new Random();

								int index = randomizer.Next(0, BroNames.Count);

								int attempts = 0;

								Player output = null;

								bool found = false;

								if(player.Info.DisplayedName == null) {
									// Fix for rank problems throughout.
									player.Info.changedName = false;
								} else {
									player.Info.oldname = player.Info.DisplayedName;
								}

								// If name is changed, true.
								player.Info.changedName = true;

								while(!found) {
									RegisteredBroNames.TryGetValue(index, out output);

									if(output == null) {
										found = true;

										break;
									}

									attempts++;
									index = randomizer.Next(0, BroNames.Count);

									output = null;

									if(attempts > 2000) {
										// Not good. :D
										break;
									}
								}

								if(found) {
									player.Message("Giving you name: " + BroNames[index]);

									player.Info.DisplayedName = Color.ReplacePercentCodes(player.Info.Rank.Color + player.Info.Rank.Prefix + BroNames[index]);

									NamesRegistered++;

									RegisteredBroNames[index] = player;
								} else {
									player.Message("Could not find a name for you.");
								}
							} else {
								player.Message("All bro names have been assigned.");
							}
						} catch(Exception ex) {
							Logger.Log(LogType.Error, "BroMode.RegisterPlayer: " + ex);
						}
					}
				}
			}
		}

		public void UnregisterPlayer(Player p) {
			try {
				for(int i = 0; i < BroNames.Count; i++) {
					if(RegisteredBroNames.ContainsKey(i) && RegisteredBroNames[i].Name.Equals(p.Name)) {
						Logger.Log(LogType.SystemActivity, "Unregistering bro name '" + BroNames[i] + "' for player '" + p.Name + "'");

						RegisteredBroNames.Remove(i);

						NamesRegistered--;

						if(!p.Info.changedName) {
							p.Info.DisplayedName = null;
						}

						if(p.Info.changedName) {
							p.Info.DisplayedName = p.Info.oldname;
							// Clears oldname if its ever removed in setinfo.
							p.Info.oldname = null;

							p.Info.changedName = false;
						}
					}
				}
			} catch(Exception ex) {
				Logger.Log(LogType.Error, "BroMode.UnregisterPlayer: " + ex);
			}
		}
	}
}
