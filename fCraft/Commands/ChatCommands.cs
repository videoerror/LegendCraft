// Copyright 2009-2012 Matvei Stefarov <me@matvei.org>

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
using System.Linq;

namespace fCraft.Commands {

	internal static class ChatCommands {

		private const int PlayersPerPage = 20;

		public static void Init() {
			CommandManager.RegisterCommand(CdBanHammer);
			CommandManager.RegisterCommand(CdBarf);
			CommandManager.RegisterCommand(CdBroFist);
			CommandManager.RegisterCommand(CdCalculator);
			CommandManager.RegisterCommand(CdFortuneCookie);
			// CommandManager.RegisterCommand(CdGive);
			CommandManager.RegisterCommand(CdGlobal);
			CommandManager.RegisterCommand(CdGPS);
			// CommandManager.RegisterCommand(CdJelly);
			CommandManager.RegisterCommand(CdMad);
			CommandManager.RegisterCommand(CdPlugins);
			CommandManager.RegisterCommand(CdSecret);
			CommandManager.RegisterCommand(CdSTFU);
			CommandManager.RegisterCommand(CdStopWatch);
			CommandManager.RegisterCommand(CdVote);

			CommandManager.RegisterCommand(CdAdminChat);
			CommandManager.RegisterCommand(CdAway);
			CommandManager.RegisterCommand(CdBroMode);
			CommandManager.RegisterCommand(CdCustomChat);
			CommandManager.RegisterCommand(CdHigh5);
			CommandManager.RegisterCommand(CdModerate);
			CommandManager.RegisterCommand(CdPoke);
			CommandManager.RegisterCommand(CdQuit);
			CommandManager.RegisterCommand(CdRageQuit);
			CommandManager.RegisterCommand(CdReview);

			CommandManager.RegisterCommand(CdClear);

			CommandManager.RegisterCommand(CdDeafen);

			CommandManager.RegisterCommand(CdIgnore);
			CommandManager.RegisterCommand(CdUnignore);

			CommandManager.RegisterCommand(CdMe);

			CommandManager.RegisterCommand(CdRoll);

			CommandManager.RegisterCommand(CdSay);

			CommandManager.RegisterCommand(CdStaff);

			CommandManager.RegisterCommand(CdTimer);


			Player.Moved += new EventHandler<Events.PlayerMovedEventArgs>(PlayerIsBack);
		}

		#region LEGEND_CRAFT
		/* Copyright(c) <2012-2014> <LeChosenOne, DingusBungus>
		   Permission is hereby granted, free of charge, to any person obtaining a copy
		   of this software and associated documentation files(the "Software"), to deal
		   in the Software without restriction, including without limitation the rights
		   to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
		   copies of the Software, and to permit persons to whom the Software is
		   furnished to do so, subject to the following conditions:
		   
		   The above copyright notice and this permission notice shall be included in
		   all copies or substantial portions of the Software.
		   
		   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
		   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
		   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
		   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
		   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
		   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
		   THE SOFTWARE. */

		private static readonly CommandDescriptor CdBanHammer = new CommandDescriptor() {
			Name = "BanHammer",

			Aliases = new string[] {
				"Bh"
			},

			Category = CommandCategory.Chat,

			Permissions = new Permission[] {
				Permission.Ban
			},

			IsConsoleSafe = false,

			Usage = "/banhammer",
			Help = "&8Activate thou banhammer!.",

			NotRepeatable = true,

			Handler = BanHammerHandler
		};

		private static void BanHammerHandler(Player player, Command cmd) {
			Server.Message("{0}&W has activated the &0Banhammer!", player.ClassyName);
		}

		// An old plugin I made, finally fully functional.
		private static readonly CommandDescriptor CdBarf = new CommandDescriptor() {
			Name = "Barf",

			Aliases = new string[] {
				"puke", "blowchunks"
			},

			Category = CommandCategory.Chat | CommandCategory.Fun ,

			Usage = "/Barf(Player)(option)",
			Help = "&SBarfs on a player. Can leave option blank to just barf.",

			NotRepeatable = true,

			Handler = BarfHandler
		};

		internal static void BarfHandler(Player player, Command cmd) {
			if(!player.MaySpeakFurther()) {
				return;
			}

			string name = cmd.Next();

			if(String.IsNullOrEmpty(name)) {
				Server.Players.CanSee(player).Except(player).Message("{0} &6Barfed&s.", player.ClassyName);

				player.Message("&sYou &6Barfed&s.");

				player.Info.LastUsedBarf = DateTime.UtcNow;

				return;
			}

			// Try to find the player.
			Player target = Server.FindPlayerOrPrintMatches(player, name, false, true);

			if(target == null) {
				return;
			}

			if(target == player) {
				Server.Players.CanSee(target).Except(target).Message("{1}&S just &6Barfed &sall over themsleves...", target.ClassyName, player.ClassyName);

				IRC.PlayerSomethingMessage(player, "barfed on", target, null);

				player.Message("&sYou &6Barfed &sall over yourself...");

				// Return to stop the code.
				return;
			}

			if(!player.Can(Permission.HighFive)) {
				player.Message("You do not have permissions to barf on others");

				return;
			}

			double time = (DateTime.UtcNow - player.Info.LastUsedBarf).TotalSeconds;

			if(time <= 20) {
				player.Message("You cannot use barf for another &W{0}&s seconds", Math.Round(20 - time));

				return;
			}

			string item = cmd.Next();
			string msg = String.Empty;

			if(String.IsNullOrEmpty(item)) {
				msg = String.Format("{0} &Swas &6Barfed &son by {1}&s.", target.ClassyName, player.ClassyName);
			} else if(item.ToLower() == "throwup") {
				msg = String.Format("{0}&s was &6Thrown Up &Son by {1}&s.", target.ClassyName, player.ClassyName);
			} else if(item.ToLower() == "puke") {
				msg = String.Format("{0}&s was &6Puked &Son by {1}&s.", target.ClassyName, player.ClassyName);
			} else if(item.ToLower() == "blowchunks") {
				msg = String.Format("{1} &6Blew Chunks &son {0}&s.", target.ClassyName, player.ClassyName);
			} else {
				msg = String.Format("{0} &Swas &6Barfed &son by {1}&s.", target.ClassyName, player.ClassyName);
			}

			Server.Players.CanSee(target).Union(target).Message(msg);

			IRC.PlayerSomethingMessage(player, "barfed on", target, null);

			player.Info.LastUsedBarf = DateTime.UtcNow;
		}

		private static readonly CommandDescriptor CdBroFist = new CommandDescriptor() {
			Name = "Brofist",

			Aliases = new string[] {
				"Bf"
			},

			Category = CommandCategory.Chat | CommandCategory.Fun,

			Permissions = new Permission[] {
				Permission.Brofist
			},

			IsConsoleSafe = true,

			Usage = "/Brofist playername",
			Help = "&8Brofists &Sa given player.",

			NotRepeatable = true,

			Handler = BrofistHandler
		};

		private static void BrofistHandler(Player player, Command cmd) {
			if(!player.MaySpeakFurther()) {
				return;
			}

			string targetName = cmd.Next();

			if(targetName == null) {
				player.Message("Enter a playername.");

				return;
			}

			Player target = Server.FindPlayerOrPrintMatches(player, targetName, false, true);

			if(target == null) {
				player.MessageNoPlayer(targetName);

				return;
			}

			if(target == player) {
				Server.Players.CanSee(target).Except(target).Message("{1}&S just tried to &8Brofist &Sthemsleves...", target.ClassyName, player.ClassyName);

				IRC.PlayerSomethingMessage(player, "brofisted", target, null);

				player.Message("&SYou just tried to &8Brofist &Syourself... That's sad...");

				return;
			}

			Server.Players.CanSee(target).Except(target).Message("{1}&S gave {0}&S a &8Brofist&S.", target.ClassyName, player.ClassyName);

			IRC.PlayerSomethingMessage(player, "brofisted", target, null);

			target.Message("{0}&S's fist met yours for a &8Brofist&S.", player.ClassyName);
		}

		#region CALCULATOR

		private static readonly CommandDescriptor CdCalculator = new CommandDescriptor() {
			Name = "Calculator",

			Aliases = new string[] {
				"Calc"
			},

			Category = CommandCategory.Chat | CommandCategory.Math,

			Permissions = new Permission[] {
				Permission.Chat
			},

			IsConsoleSafe = true,

			Usage = "/Calculator [number] [+, -, *, /, sqrt, sqr] [(for +,-,*, or /)number]",
			Help = "Lets you use a simple calculator in minecraft. Valid options are [ + , - , * ,  / , sqrt, and sqr].",

			NotRepeatable = false,

			Handler = CalcHandler
		};

		private static void CalcHandler(Player player, Command cmd) {
			string numberone = cmd.Next();
			string numbertwo = cmd.Next();
			string op = cmd.Next();

			double no1 = 1;
			double no2 = 1;

			if(numberone == null || op == null) {
				CdCalculator.PrintUsage(player);

				return;
			}

			if(!Double.TryParse(numberone, out no1)) {
				player.Message("Please choose from a whole number.");

				return;
			}

			if(numbertwo != null) {
				if(!Double.TryParse(numbertwo, out no2)) {
					player.Message("Please choose from a whole number.");

					return;
				}
			}									

			if(player.Can(Permission.Chat)) {
				if(numberone != null || op != null) {
					if(op == "+" | op == "-" | op == "*" | op == "/" |
					   op == "sqrt" | op == "sqr") {
						if(op == "+") {
							if(numbertwo == null) {
								player.Message("You must select a second number!");

								return;
							}

							double add = no1 + no2;

							if(add < 0 | no1 < 0 | no2 < 0) {
								player.Message("Negative Number Detected, please choose from a whole number.");

								return;
							} else {
								player.Message("&0Calculator&f: {0} + {1} = {2}", no1, no2, add);
							}
						}

						if(op == "-") {
							if(numbertwo == null) {
								player.Message("You must select a second number!");

								return;
							}

							double subtr = no1 - no2;

							if(subtr < 0 | no1 < 0 | no2 < 0) {
								player.Message("Negative Number Detected, please choose from a whole number.");

								return;
							} else {
								player.Message("&0Calculator&f: {0} - {1} = {2}", no1, no2, subtr);
							}
						}

						if(op == "*") {
							if(numbertwo == null) {
								player.Message("You must select a second number!");

								return;
							}

							double mult = no1 * no2;

							if(mult < 0 | no1 < 0 | no2 < 0) {
								player.Message("Negative Number Detected, please choose from a whole number.");

								return;
							} else {
								player.Message("&0Calculator&f: {0} * {1} = {2}", no1, no2, mult);
							}
						}

						if(op == "/") {
							if(numbertwo == null) {
								player.Message("You must select a second number!");

								return;
							}

							double div = no1 / no2;

							if(div < 0 | no1 < 0 | no2 < 0) {
								player.Message("Negative Number Detected, please choose from a whole number.");

								return;
							} else {
								player.Message("&0Calculator&f: {0} / {1} = {2}", no1, no2, div);

								return;
							}
						}

						if(op == "sqrt") {
							double sqrt = Math.Round(Math.Sqrt(no1), 2);

							if(no1 < 0) {
								player.Message("Negative Number Detected, please choose from a whole number.");

								return;
							} else {
								player.Message("&0Calculator&f: Square Root of {0} = {1}", no1, sqrt);

								return;
							}
						}

						if(op == "sqr") {
							double sqr = no1 * no1;

							if(no1 < 0) {
								player.Message("Negative Number Detected, please choose from a whole number.");

								return;
							} else {
								player.Message("&0Calculator&f: Square of {0} = {1}", no1, sqr);

								return;
							}
						}
					} else {
						player.Message("&cInvalid Operator. Please choose from '+' , '-' , '*' , '/' , 'sqrt' , or 'sqr'");

						return;
					}
				} else {
					CdCalculator.PrintUsage(player);
				}
			}
		}
		#endregion

		private static readonly CommandDescriptor CdFortuneCookie = new CommandDescriptor() {
			Name = "FortuneCookie",

			Aliases = new string[] {
				"FC", "Fortune"
			},

			Category = CommandCategory.Chat,

			Permissions = new Permission[] {
				Permission.Chat
			},

			IsConsoleSafe = true,

			Usage = "/FortuneCookie",
			Help = "Reads you your fortune",

			NotRepeatable = true,

			Handler = FCHandler
		};

		private static void FCHandler(Player player, Command cmd) {
			player.Message("You ate the fortune cookie, here is your fortune!");

			string[] msgStrings = new string[] {
				"&3You will die in your life time.",
				"&3Early to bed, early to rise, makes a man healthy, wealthy, and tired.",
				"&3About time someone got me out of that stale cookie.",
				"&3Ignore previous fortune.",
				"&3You are not illiterate.",
				"&3Only listen to the fortune cookie. Disregard all other fortune telling units.",
				"&3This is not the fortune cookie you are looking for.",
				"&3That wasn't actually a cookie you just ate...",
				"&3Warning, do not break open or eat your fortune cookie.",
				"&3You may still be hungry. Order takeout now.",
				"&3There is something you wanted to ask?",
				"&3For the last time, I am not a fortune, just a small piece of paper.",
				"&3Durka Durka, Muhammed Jihad.",
				"&3Just focus on the cookie because this fortune sucks...",
				"&3How many people eat the cookie and don't even look at the fortune? I guess you aren't one of those people...",
				"&3You know, being a cookie is really crummy.",
				"&3A wise man once told me that fortunes are bollocks.",
				"&3Focus is the key to being successful in life."
			};

			Random RandmsgString = new Random();

			player.Message(msgStrings[RandmsgString.Next(0, msgStrings.Length)]);

			string[] numStrings = new string[] {
				" &cLucky Numbers:&e 1,2,3,4,5",
				" &cLucky Numbers:&e 0,0,0,0,0",
				" &cLucky Numbers:&e .5,0,-1,10035,poop",
				" &cLucky Numbers:&e 1,2,1,2,1",
				" &cLucky Numbers:&e p,o,o,p,y",
				" &cLucky Numbers:&e 18,11,22,36,8",
				" &cLucky Numbers:&e 11,7,68,0,0",
				" &cLucky Numbers:&e 11,11,11,12,11",
				" &cLucky Numbers:&e M,II,XL,IV,V",
				" &cLucky Numbers:&e 3,2,1,2,1",
				" &cLucky Numbers:&e 8,6,7,5,3,0,9",
				" &cLucky Numbers:&e en,tre,sju,sex,femton",
				" &cLucky Numbers:&e 1,10,100,1000,10000",
				" &cLucky Numbers:&e 19,12,3,45,6",
				" &cLucky Numbers:&e 2,4,8,16,32",
				" &cLucky Numbers:&e 1,11,12,23,35",
				" &cLucky Numbers:&e 1,23,4,17,26",
				" &cLucky Numbers:&e 4,9,16,33,46"
			};

			Random RandnumString = new Random();

			player.Message(numStrings[RandnumString.Next(0, numStrings.Length)]);
		}

		private static readonly CommandDescriptor CdGive = new CommandDescriptor() {
			Name = "Give",

			Aliases = new string[] {
				"lend"
			},

			Category = CommandCategory.Chat,

			Permissions = new Permission[] {
				Permission.Teleport
			},

			RepeatableSelection = true,
			IsConsoleSafe = true,

			Usage = "/Give [playername] [item] [amount]",
			Help = "Gives a player somethin` useful.",

			Handler = Give,
		};

		internal static void Give(Player player, Command cmd) {
			string targetName = cmd.Next();

			if(targetName == null) {
				player.Message("&WPlease insert a playername");

				return;
			}

			Player target = Server.FindPlayerOrPrintMatches(player, targetName, false, true);

			if(target == null) {
				player.MessageNoPlayer(targetName);

				return;
			}

			if(target == player) {
				player.Message("&WYou cannot give yourself something.");

				return;
			}

			string item = cmd.Next();

			if(item == null) {
				player.Message("&WPlease insert an item.");

				return;
			}

			string itemnumber = cmd.Next();

			if(itemnumber == null) {
				player.Message("&WPlease insert the item number.");

				return;
			} else {
				Server.Players.CanSee(target).Message("{0} &egave {1} &e{2} {3}.", player.ClassyName, target.ClassyName, itemnumber, item);
			}
		}

		private static readonly CommandDescriptor CdGlobal = new CommandDescriptor() {
			Name = "Global",

			Category = CommandCategory.Chat,

			Aliases = new string[] {
				"gl"
			},

			IsConsoleSafe = true,

			Permissions = new Permission[] {
				Permission.Chat
			},

			Usage = "/Global [ <message here> / ignore / unignore / disconnect / connect / help]",
			Help = "Sends a global message to other LegendCraft servers",

			Handler = GHandler
		};

		private static void GHandler(Player player, Command cmd) {
			string message = cmd.NextAll();

			if(!GlobalChat.isEnabled) {
				player.Message("&cGlobal Chat has been disabled on this server.");

				return;
			}

			if(message == "connect") {
				if(player.Can(Permission.ReadAdminChat)) {
					if(GlobalChat.GlobalThread.isConnected) {
						player.Message("&c{0}&c is already connected to the LegendCraft Global Chat Network!", ConfigKey.ServerName.GetString());

						return;
					}

					GlobalChat.GlobalThread.GCReady = true;

					Server.Message("&eAttempting to connect to LegendCraft Global Chat Network. This may take up to two minutes.");

					GlobalChat.Init();
					GlobalChat.Start();

					return;
				} else {
					player.Message("&eYou don't have the required permissions to do that!");

					return;
				}
			}

			if(!GlobalChat.GlobalThread.GCReady) {
				player.Message("&cGlobal Chat is not connected.");

				return;
			}

			var SendList = Server.Players.Where(p => p.GlobalChatAllowed && !p.IsDeaf);

			if(message == "disconnect") {
				if(player.Can(Permission.ReadAdminChat)) {
					Server.Message("&e{0}&e disconnected {1}&e from the LegendCraft Global Chat Network.", player.ClassyName, ConfigKey.ServerName.GetString());

					GlobalChat.GlobalThread.SendChannelMessage("&i" + ConfigKey.ServerName.GetString() + "&i has disconnected from the LegendCraft Global Chat Network.");

					GlobalChat.GlobalThread global = new GlobalChat.GlobalThread();

					global.DisconnectThread();

					return;
				} else {
					player.Message("&eYou don't have the required permissions to do that!");

					return;
				}
			}

			if(message == "ignore") {
				if(player.GlobalChatIgnore) {
					player.Message("You are already ignoring global chat!");

					return;
				} else {
					player.Message("&eYou are now ignoring global chat. To return to global chat, type /global unignore.");

					player.GlobalChatIgnore = true;

					return;
				}
			}

			if(message == "unignore") {
				if(player.GlobalChatIgnore) {
					player.Message("You are no longer ignoring global chat.");

					player.GlobalChatIgnore = false;

					return;
				} else {
					player.Message("&cYou are not currently ignoring global chat!");

					return;
				}
			} else if(message == "help") {
				player.Message("_LegendCraft GlobalChat Network Help_\n" +
							   "Ignore: Usage is '/global ignore'. Allows a user to ignore and stop using global chat." +
							   "Type /global unignore to return to global chat. \n" +
							   "Unignore: Usage is '/global unignore.' Allows a user to return to global chat. \n" +
							   "Connect: For admins only. Usage is /global connect. Connects your server to the LegendCraft GlobalChat Network. \n" +
							   "Disconnect: For admins only. Usage is /global disconnect. Disconnects your server from the LegendCraft GlobalChat Network. \n" +
							   "Message: Usage is '/global <your message here>'. Will send your message to the rest of the servers connected to GlobalChat.");

				return;
			}

			if(player.Info.IsMuted) {
				player.MessageMuted();

				return;
			} else if(!player.GlobalChatAllowed) {
				player.Message("Global Chat Rules: By using global chat, you automatically agree to these terms and conditions. " +
							   "Failure to agree may result in a global chat kick or ban. \n" +
							   "1) No Spamming or deliberate insulting. \n" +
							   "2) No advertising of any server or other minecraft related/unrelated service or product. \n" +
							   "3) No discussion of illegal or partially illegal tasks is permitted. \n" +
							   "4) Connecting bots to the Global Chat Network is not permitted, unless approved by the LegendCraft Team. \n" +
							   "&aYou are now permitted to use /global on this server.");

				player.GlobalChatAllowed = true;
			} else if(message == null) {
				player.Message("&eYou must enter a message!");

				return;
			} else if(player.GlobalChatAllowed) {
				string rawMessage = player.ClassyName + Color.White + ": " + message;
				message = player.ClassyName + Color.Black + ": " + message;

				SendList.Message("&g[Global] " + rawMessage); 

				GlobalChat.GlobalThread.SendChannelMessage(Color.ReplacePercentCodes(Color.MinecraftToIrcColors(message))); 
			}
		}

		private static readonly CommandDescriptor CdGPS = new CommandDescriptor() {
			Name = "GPS",

			Category = CommandCategory.Chat,

			Permissions = new Permission[] {
				Permission.Chat
			},

			IsConsoleSafe = false,

			Usage = "/GPS",
			Help = "Displays your coordinates.",

			NotRepeatable = true,

			Handler = GPSHandler
		};

		private static void GPSHandler(Player player, Command cmd) {
			LegendCraft.coords(player);
		}

		private static readonly CommandDescriptor CdJelly = new CommandDescriptor() {
			Name = "Jelly",

			Category = CommandCategory.Chat,

			IsConsoleSafe = false,

			Permissions = new Permission[] {
				Permission.EditPlayerDB
			},

			Usage = "/jelly PlayerName",
			Help = "&SHe jelly",

			Handler = JellyHandler
		};

		private static void JellyHandler(Player player, Command cmd) {
			string targetName = cmd.Next();
			string jelly = "jelly";

			try {
				Player target = Server.FindPlayerOrPrintMatches(player, targetName, false, true);

				if(target == player) {
					player.Message("You can't make yourself jealous!");

					return;
				}

				if(target == null) {
					player.Message("No players found matching {0}", target.ClassyName);

					return;
				}

				if(player.Can(Permission.EditPlayerDB, target.Info.Rank)) {
					PlayerInfo info = PlayerDB.FindPlayerInfoOrPrintMatches(player, targetName);

					if(info == null) {
						return;
					}

					string oldDisplayedName = info.DisplayedName;
					info.DisplayedName = jelly;

					if(target.Info.isMad == false | target.Info.isJelly == false) {
						if(oldDisplayedName == null) {
							Server.Message("&W{0} = Jelly", info.Name);
						} else {
							Server.Message("&W{0] = Jelly", info.Name);
						}
					} else {
						player.Message("Target's name was reset!");

						target.Info.isJelly = false;
						target.Info.isMad = false;

						target.Info.DisplayedName = target.Info.Name;
					}
				} else {
					player.Message("&W You can only make players jelly ranked {0}&W and below", player.Info.Rank.GetLimit(Permission.EditPlayerDB).ClassyName);
				}
			} catch(ArgumentNullException) {
				player.Message("Expected format: /mad playername.");
			}
		}

		private static readonly CommandDescriptor CdMad = new CommandDescriptor() {
			Name = "Mad",

			Category = CommandCategory.Chat,

			IsConsoleSafe = false,

			Permissions = new Permission[] {
				Permission.EditPlayerDB
			},

			Usage = "/mad PlayerName",
			Help = "&SHe mad.(Protip: Use /mad playername again to make the player not mad anymore)>",

			Handler = MadHandler
		};

		private static void MadHandler(Player player, Command cmd) {
			string targetName = cmd.Next();
			string mad = "mad";

			try {
				Player target = Server.FindPlayerOrPrintMatches(player, targetName, false, true);

				if(target == player) {
					player.Message("You can't make yourself mad!");

					return;
				}

				if(target == null) {
					player.Message("No player found matching {0}!", target.ClassyName);

					return;
				}

				if(player.Can(Permission.EditPlayerDB, target.Info.Rank)) {
					PlayerInfo info = PlayerDB.FindPlayerInfoOrPrintMatches(player, targetName);

					if(target.Info.isMad == false | target.Info.isJelly == false) {
						if(info == null) {
							return;
						}

						string oldDisplayedName = target.Info.DisplayedName;
						target.Info.DisplayedName = mad;

						if(oldDisplayedName == null) {
							Server.Message("&W{0} = " + mad, info.Name);

							target.Info.isMad = true;
						} else {
							Server.Message("&W{0] = " + mad, info.Name);
						}
					} else {
						player.Message("Target's name was reset!");

						target.Info.isMad = false;
						target.Info.isJelly = false;

						target.Info.DisplayedName = target.Info.Name;
					}
				} else {
					player.Message("&W You can only make players mad ranked {0}&W and below",
								   player.Info.Rank.GetLimit(Permission.EditPlayerDB).ClassyName);
				}
			} catch(ArgumentNullException) {
				player.Message("Expected format: /mad playername.");
			}
		}

		private static readonly CommandDescriptor CdPlugins = new CommandDescriptor() {
			Name = "Plugins",

			Aliases = new string[] {
				"plugin"
			},

			Category = CommandCategory.Chat,

			Permissions = new Permission[] {
				Permission.Chat
			},

			IsConsoleSafe = true,

			Usage = "/Plugins",
			Help = "Displays all plugins on the server.",

			Handler = PluginsHandler
		};

		private static void PluginsHandler(Player player, Command cmd) {
			List<String> plugins = new List<String>();

			player.Message("&c_Current plugins on {0}&c_", ConfigKey.ServerName.GetString());

			// Sloppy :P, PluginManager.Plugins adds ".Init", so this should split the ".Init" from the plugin name.
			foreach(Plugin plugin in PluginManager.Plugins) {
				string pluginString = plugin.ToString();

				string[] splitPluginString = pluginString.Split('.');

				plugins.Add(splitPluginString[0]);
			}

			player.Message(String.Join(", ", plugins));
		}

		private static readonly CommandDescriptor CdSecret = new CommandDescriptor() {
			Name = "Secret",

			Category = CommandCategory.Chat,

			Permissions = new Permission[] {
				Permission.Chat
			},

			Usage = "/Secret",
			Help = "&SDon't tell anyone!!!",

			NotRepeatable = true,

			Handler = SecretHandler
		};

		internal static void SecretHandler(Player player, Command cmd) {
			Server.Message("{0} is a loser", player.ClassyName);
		}

		private static readonly CommandDescriptor CdSTFU = new CommandDescriptor() {
			Name = "STFU",

			Aliases = new string[] {
				"ShutUp", "TrollMute"
			},

			Category = CommandCategory.Chat | CommandCategory.Fun,

			Permissions = new Permission[] {
				Permission.STFU
			},

			IsConsoleSafe = true,

			Usage = "/STFU playername",
			Help = "'Mutes' a player for 999m.",

			NotRepeatable = true,

			Handler = STFUHandler
		};

		private static void STFUHandler(Player player, Command cmd) {
			string name = cmd.Next();

			// If no name is given.
			if(name == null) {
				player.Message("Please enter a name");

				return;
			}

			Player target = Server.FindPlayerOrPrintMatches(player, name, false, true);

			if(target == player) {
				player.Message("You cannot mute yourself.");

				return;
			}

			if(target == null) {
				return;
			}

			//Broadcasts a server message saying the player is muted, even though they're not.
			if(player.Can(Permission.Slap, target.Info.Rank)) {
				Server.Players.CanSee(target).Except(target).Message("&sPlayer {0}&6*&s was muted by {1}&s for 999m.", target.ClassyName, player.ClassyName);

				IRC.PlayerSomethingMessage(player, "muted", target, null);

				target.Message("&sYou were muted by {0}&s for 999m.", player.ClassyName);

				return;
				// Meaning if the player can't use the command.
			} else {
				player.Message("You can only mute players ranked {0}&S or lower", player.Info.Rank.GetLimit(Permission.Mute).ClassyName);
			}
		}

		private static readonly CommandDescriptor CdStopWatch = new CommandDescriptor() {
			Name = "StopWatch",

			Aliases = new string[] {
				"sw"
			},

			Category = CommandCategory.Chat,

			Permissions = new Permission[] {
				Permission.Chat
			},

			IsConsoleSafe = false,

			Usage = "/StopWatch [Start / Stop / Time / Help]",
			Help = "Stopwatch management.",

			Handler = StopWatchHandler
		};

		private static void StopWatchHandler(Player player, Command cmd) {
			string option = cmd.Next();

			if(option == null) {
				player.Message("Please choose either start, stop, time, or help as your option.");

				return;
			}

			if(option == "start") {
				if(Player.stopwatchRunning) {
					player.Message(Player.alreadyStopwatch);

					return;
				} else {
					Server.Message("&cStopWatch&f: {0}&f started a stopwatch!", player.ClassyName);

					Player.stopwatchRunning = true;

					Player.stopwatch.Start();

					return;
				}			 
			} else if(option == "stop") {
				if(Player.stopwatchRunning) {
					if(Player.stopwatch.Elapsed.Seconds < 10) {
						Server.Message("&cStopWatch&f: {0}&f ended the stopwatch at {1}", player.ClassyName, Player.stopwatch.Elapsed.Minutes + ":0" + Player.stopwatch.Elapsed.Seconds);

						Player.stopwatchRunning = false;

						Player.stopwatch.Stop();

						return;
					} else {
						Server.Message("&cStopWatch&f: {0}&f ended the stopwatch at {1}", player.ClassyName, Player.stopwatch.Elapsed.Minutes + ":" + Player.stopwatch.Elapsed.Seconds);

						Player.stopwatchRunning = false;

						Player.stopwatch.Stop();

						return;
					}
				} else {
					player.Message(Player.noStopwatch);

					return;
				}
			} else if(option == "time") {
				if(Player.stopwatchRunning) {
					if(Player.stopwatch.Elapsed.Seconds < 10) {
						Server.Message("&cStopWatch&f: Time is at {0}", Player.stopwatch.Elapsed.Minutes + ":0" + Player.stopwatch.Elapsed.Seconds);

						return;
					} else {
						Server.Message("&cStopWatch&f: Time is at {0}", Player.stopwatch.Elapsed.Minutes + ":" + Player.stopwatch.Elapsed.Seconds);
					}
				} else {
					player.Message(Player.noStopwatch);

					return;
				}								  
			} else if(option == "help") {
				player.Message("&eStopWatch: \n" +
							   "&eStart: Will start the stopwatch. \n" +
							   "&eStop: Will stop the stopwatch, reset it, and print out the total time. \n" +
							   "&eTime: Will print out the current time that the stopwatch is at.");

				return;
			} else {
				player.Message("&eUsage is: /StopWatch [Start / Stop / Time / Help]");

				return;
			}
		}

		private static readonly CommandDescriptor CdVote = new CommandDescriptor() {
			Name = "Vote",

			Category = CommandCategory.Chat,

			Permissions = new Permission[] {
				Permission.Chat
			},

			IsConsoleSafe = false,

			Usage = "/Vote Yes/No/Ask/Abort",
			Help = "Allows you to vote.",

			Handler = VoteHandler
		};

		private static void VoteHandler(Player player, Command cmd) {
			fCraft.Commands.CommandHandlers.VoteHandler.VoteParams(player, cmd);
		}
		#endregion

		#region 800_CRAFT
		// Copyright(C) <2012> <Jon Baker, Glenn Mariën and Lao Tszy>

		// This program is free software: you can redistribute it and/or modify
		// it under the terms of the GNU General Public License as published by
		// the Free Software Foundation, either version 3 of the License, or
		// (at your option) any later version.

		// This program is distributed in the hope that it will be useful,
		// but WITHOUT ANY WARRANTY; without even the implied warranty of
		// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
		// GNU General Public License for more details.

		// You should have received a copy of the GNU General Public License
		// along with this program. If not, see <http://www.gnu.org/licenses/>.

		private static readonly CommandDescriptor CdAdminChat = new CommandDescriptor() {
			Name = "Adminchat",

			Aliases = new string[] {
				"ac"
			},

			Category = CommandCategory.Chat | CommandCategory.Moderation,

			Permissions = new Permission[] {
				Permission.Chat
			},

			IsConsoleSafe = true,
			NotRepeatable = true,

			Usage = "/Adminchat Message",
			Help = "Broadcasts your message to admins/owners on the server.",

			Handler = AdminChat
		};

		internal static void AdminChat(Player player, Command cmd) {
			if(player.Info.IsMuted) {
				player.MessageMuted();

				return;
			}

			if(DateTime.UtcNow < player.Info.MutedUntil) {
				player.Message("You are muted for another {0:0} seconds.",
							   player.Info.MutedUntil.Subtract(DateTime.UtcNow).TotalSeconds);

				return;
			}

			string message = cmd.NextAll().Trim();

			if(message.Length > 0) {
				if(player.Can(Permission.UseColorCodes) && message.Contains("%")) {
					message = Color.ReplacePercentCodes(message);
				}

				Chat.SendAdmin(player, message);
			}
		}

		private static readonly CommandDescriptor CdAway = new CommandDescriptor() {
			Name = "Away",

			Category = CommandCategory.Chat,

			Aliases = new string[] {
				"afk"
			},

			IsConsoleSafe = true,

			Usage = "/away [optional message]",
			Help = "Shows an away message.",

			NotRepeatable = true,

			Handler = Away
		};

		internal static void Away(Player player, Command cmd) {
			string msg = cmd.NextAll().Trim();

			if(player.Info.IsMuted) {
				player.MessageMuted();

				return;
			}

			if(msg.Length > 0) {
				Server.Message("{0}&S &Eis away &9({1})", player.ClassyName, msg);

				player.IsAway = true;

				return;
			} else {
				Server.Players.Message("&S{0} &Eis away &9(Away From Keyboard)", player.ClassyName);

				player.IsAway = true;
			}
		}

		public static void PlayerIsBack(object sender, Events.PlayerMovedEventArgs e) {
			if(e.Player.IsAway) {
				// We need to have block positions, so we divide by 32.
				Vector3I oldPos = new Vector3I(e.OldPosition.X / 32, e.OldPosition.Y / 32, e.OldPosition.Z / 32);
				Vector3I newPos = new Vector3I(e.NewPosition.X / 32, e.NewPosition.Y / 32, e.NewPosition.Z / 32);

				// Check if the player actually moved and not just rotated.
				if((oldPos.X != newPos.X) || (oldPos.Y != newPos.Y) || (oldPos.Z != newPos.Z)) {
					Server.Players.Message("{0} &Eis back", e.Player.ClassyName);

					e.Player.IsAway = false;
				}
			}
		}

		private static readonly CommandDescriptor CdBroMode = new CommandDescriptor() {
			Name = "Bromode",

			Aliases = new string[] {
				"bm"
			},

			Category = CommandCategory.Chat | CommandCategory.Fun,

			Permissions = new Permission[] {
				Permission.BroMode
			},

			IsConsoleSafe = true,

			Usage = "/Bromode",
			Help = "Toggles bromode.",

			Handler = BroMode
		};

		private static void BroMode(Player player, Command command) {
			if(!fCraft.Commands.CommandHandlers.BroMode.Active) {
				foreach(Player p in Server.Players) {
					fCraft.Commands.CommandHandlers.BroMode.GetInstance().RegisterPlayer(p);
				}

				fCraft.Commands.CommandHandlers.BroMode.Active = true;

				Server.Players.Message("{0}&S turned Bro mode on.", player.Info.Rank.Color + player.Name);

				IRC.SendAction(player.Name + " turned Bro mode on.");
			} else {
				foreach(Player p in Server.Players) {
					fCraft.Commands.CommandHandlers.BroMode.GetInstance().UnregisterPlayer(p);
				}

				fCraft.Commands.CommandHandlers.BroMode.Active = false;

				Server.Players.Message("{0}&S turned Bro Mode off.", player.Info.Rank.Color + player.Name);

				IRC.SendAction(player.Name + " turned Bro mode off");
			}
		}

		private static readonly CommandDescriptor CdCustomChat = new CommandDescriptor() {
			Name = ConfigKey.CustomChatName.GetString(),

			Category = CommandCategory.Chat,

			Aliases = new string[] {
				ConfigKey.CustomAliasName.GetString()
			},

			Permissions = new Permission[] {
				Permission.Chat
			},

			IsConsoleSafe = true,
			NotRepeatable = true,

			Usage = "/Customname Message",
			Help = "Broadcasts your message to all players allowed to read the CustomChatChannel.",

			Handler = CustomChatHandler
		};

		private static void CustomChatHandler(Player player, Command cmd) {
			if(!player.MaySpeakFurther()) {
				return;
			}

			string message = cmd.NextAll().Trim();

			if(message.Length > 0) {
				if(player.Can(Permission.UseColorCodes) && message.Contains("%")) {
					message = Color.ReplacePercentCodes(message);
				}

				Chat.SendCustom(player, message);
			}
		}

		private static readonly CommandDescriptor CdHigh5 = new CommandDescriptor() {
			Name = "High5",

			Aliases = new string[] {
				"H5"
			},

			Category = CommandCategory.Chat | CommandCategory.Fun,

			Permissions = new Permission[] {
				Permission.HighFive
			},

			IsConsoleSafe = true,

			Usage = "/High5 playername",
			Help = "High fives a given player.",

			NotRepeatable = true,

			Handler = High5Handler
		};

		internal static void High5Handler(Player player, Command cmd) {
			if(!player.MaySpeakFurther()) {
				return;
			}

			string targetName = cmd.Next();

			if(targetName == null) {
				CdHigh5.PrintUsage(player);

				return;
			}

			Player target = Server.FindPlayerOrPrintMatches(player, targetName, false, true);

			if(target == null) {
				return;
			}

			if(target == player) {
				player.Message("&WYou cannot high five yourself.");

				return;
			}

			Server.Players.CanSee(target).Except(target).Message("{0}&S was just &chigh fived &Sby {1}&S",
																 target.ClassyName,
																 player.ClassyName);

			IRC.PlayerSomethingMessage(player, "high fived", target, null);

			target.Message("{0}&S high fived you.", player.ClassyName);
		}

		private static readonly CommandDescriptor CdModerate = new CommandDescriptor() {
			Name = "Moderate",

			Aliases = new string[] {
				"Moderation"
			},

			Category = CommandCategory.Moderation,

			IsConsoleSafe = true,

			Permissions = new Permission[] {
				Permission.Moderation
			},

			NotRepeatable = true,

			Help = "Create a server-wide silence, muting all players until called again.",
			Usage = "/Moderate [Voice / Devoice] [PlayerName]",

			Handler = ModerateHandler
		};

		internal static void ModerateHandler(Player player, Command cmd) {
			string option = cmd.Next();

			if(option == null) {
				if(Server.Moderation) {
					Server.Moderation = false;

					Server.Message("{0}&W deactivated server moderation, the chat feed is enabled", player.ClassyName);

					IRC.SendAction(player.ClassyName + " &Sdeactivated server moderation, the chat feed is enabled");

					Server.VoicedPlayers.Clear();
				} else {
					Server.Moderation = true;

					Server.Message("{0}&W activated server moderation, the chat feed is disabled", player.ClassyName);

					IRC.SendAction(player.ClassyName + " &Sactivated server moderation, the chat feed is disabled");

					// Console safe.
					if(player.World != null) {
						Server.VoicedPlayers.Add(player);
					}
				}
			} else {
				string name = cmd.Next();

				if(option.ToLower() == "voice" && Server.Moderation) {
					if(name == null) {
						player.Message("Please enter a player to Voice");

						return;
					}

					Player target = Server.FindPlayerOrPrintMatches(player, name, false, true);

					if(target == null) {
						return;
					}

					if(Server.VoicedPlayers.Contains(target)) {
						player.Message("{0}&S is already voiced", target.ClassyName);

						return;
					}

					Server.VoicedPlayers.Add(target);

					Server.Message("{0}&S was given Voiced status by {1}", target.ClassyName, player.ClassyName);

					return;
				} else if(option.ToLower() == "devoice" && Server.Moderation) {
					if(name == null) {
						player.Message("Please enter a player to Devoice");

						return;
					}

					Player target = Server.FindPlayerOrPrintMatches(player, name, false, true);

					if(target == null) {
						return;
					}

					if(!Server.VoicedPlayers.Contains(target)) {
						player.Message("&WError: {0}&S does not have voiced status", target.ClassyName);

						return;
					}

					Server.VoicedPlayers.Remove(target);

					player.Message("{0}&S is no longer voiced", target.ClassyName);
					target.Message("You are no longer voiced");

					return;
				} else {
					player.Message("&WError: Server moderation is not activated");
				}
			}
		}

		private static readonly CommandDescriptor CdPoke = new CommandDescriptor() {
			Name = "Poke",

			Category = CommandCategory.Chat | CommandCategory.Fun,

			IsConsoleSafe = true,

			Usage = "/poke playername",
			Help = "&SPokes a Player.",

			NotRepeatable = true,

			Handler = PokeHandler
		};

		internal static void PokeHandler(Player player, Command cmd) {
			if(!player.MaySpeakFurther()) {
				return;
			}

			string targetName = cmd.Next();

			if(targetName == null) {
				CdPoke.PrintUsage(player);

				return;
			}

			Player target = Server.FindPlayerOrPrintMatches(player, targetName, false, true);

			if(target == null) {
				return;
			}

			if(target.Immortal) {
				player.Message("&SYou failed to poke {0}&S, they are immortal", target.ClassyName);

				return;
			}

			if(target == player) {
				player.Message("You cannot poke yourself.");

				return;
			}

			if(!Player.IsValidName(targetName)) {
				return;
			} else {
				target.Message("&8You were just poked by {0}", player.ClassyName);
				player.Message("&8Successfully poked {0}", target.ClassyName);
			}
		}

		private static readonly CommandDescriptor CdQuit = new CommandDescriptor() {
			Name = "Quitmsg",

			Aliases = new string[] {
				"quit", "quitmessage"
			},

			Category = CommandCategory.Chat,

			IsConsoleSafe = false,

			Permissions = new Permission[] {
				Permission.Chat
			},

			Usage = "/Quitmsg [message]",
			Help = "Adds a farewell message which is displayed when you leave the server.",

			Handler = QuitHandler
		};

		private static void QuitHandler(Player player, Command cmd) {
			string Msg = cmd.NextAll();

			if(Msg.Length < 1) {
				CdQuit.PrintUsage(player);

				return;
			} else {
				player.Info.LeaveMsg = "left the server: &C" + Msg;

				player.Message("Your quit message is now set to: {0}", Msg);
			}
		}

		private static readonly CommandDescriptor CdRageQuit = new CommandDescriptor() {
			Name = "Ragequit",

			Aliases = new string[] {
				"rq"
			},

			Category = CommandCategory.Chat | CommandCategory.Fun,

			IsConsoleSafe = false,

			Permissions = new Permission[] {
				Permission.RageQuit
			},

			Usage = "/Ragequit [reason]",
			Help = "An anger-quenching way to leave the server.",

			Handler = RageHandler
		};

		private static void RageHandler(Player player, Command cmd) {
			string reason = cmd.NextAll();

			if(reason.Length < 1) {
				Server.Players.Message("{0} &4Ragequit from the server", player.ClassyName);

				player.Kick(Player.Console, "Ragequit", LeaveReason.RageQuit, false, false, false);

				IRC.SendAction(player.ClassyName + " &4Ragequit from the server");

				return;
			} else {
				Server.Players.Message("{0} &4Ragequit from the server: &C{1}", player.ClassyName, reason);

				IRC.SendAction(player.ClassyName + " &WRagequit from the server: " + reason);

				player.Kick(Player.Console, reason, LeaveReason.RageQuit, false, false, false);
			}
		}

		private static readonly CommandDescriptor CdReview = new CommandDescriptor() {
			Name = "Review",

			Category = CommandCategory.Chat,

			IsConsoleSafe = true,

			Usage = "/review",

			NotRepeatable = true,

			Help = "&SRequest an Op to review your build.",

			Handler = Review
		};

		internal static void Review(Player player, Command cmd) {
			if(player.Info.IsMuted) {
				player.MessageMuted();

				return;
			}

			var recepientList = Server.Players.Can(Permission.ReadStaffChat)
												  .NotIgnoring(player)
												  .Union(player);

			string message = String.Format("{0}&6 would like staff to check their build", player.ClassyName);

			recepientList.Message(message);

			var ReviewerNames = Server.Players.CanBeSeen(player).Where(r => r.Can(Permission.Promote,
																				  player.Info.Rank));
			if(ReviewerNames.Count() > 0) {
				player.Message("&WOnline players who can review you: {0}",
							   ReviewerNames.JoinToString(r => String.Format("{0}&S",
															  				 r.ClassyName)));

				return;
			} else {
				player.Message("&WThere are no players online who can review you. A member of staff needs to be online.");
			}
		}
		#endregion

		#region CLEAR

		private const int LinesToClear = 30;

		private static readonly CommandDescriptor CdClear = new CommandDescriptor() {
			Name = "Clear",

			UsableByFrozenPlayers = true,

			Category = CommandCategory.Chat,

			Help = "&SClears the chat screen.",

			Handler = ClearHandler
		};

		private static void ClearHandler(Player player, Command cmd) {
			if(cmd.HasNext) {
				CdClear.PrintUsage(player);

				return;
			}

			for(int i = 0; i < LinesToClear; i++) {
				player.Message("");
			}
		}
		#endregion

		#region DEAFEN

		private static readonly CommandDescriptor CdDeafen = new CommandDescriptor() {
			Name = "Deafen",

			Aliases = new string[] {
				"deaf"
			},

			Category = CommandCategory.Chat,

			IsConsoleSafe = true,

			Help = "Blocks all chat messages from being sent to you.",

			Handler = DeafenHandler
		};

		private static void DeafenHandler(Player player, Command cmd) {
			if(cmd.HasNext) {
				CdDeafen.PrintUsage(player);

				return;
			}

			if(!player.IsDeaf) {
				for(int i = 0; i < LinesToClear; i++) {
					player.MessageNow("");
				}

				player.MessageNow("Deafened mode: ON");
				player.MessageNow("You will not see ANY messages until you type &H/Deafen&S again.");

				player.IsDeaf = true;
			} else {
				player.IsDeaf = false;

				player.MessageNow("Deafened mode: OFF");
			}
		}
		#endregion

		#region IGNORE_AND_UNIGNORE

		private static readonly CommandDescriptor CdIgnore = new CommandDescriptor() {
			Name = "Ignore",

			Category = CommandCategory.Chat,

			IsConsoleSafe = true,

			Usage = "/Ignore [PlayerName]",
			Help = "&STemporarily blocks the other player from messaging you. " +
			"If no player name is given, lists all ignored players.",

			Handler = IgnoreHandler
		};

		private static void IgnoreHandler(Player player, Command cmd) {
			string name = cmd.Next();

			if(name != null) {
				if(cmd.HasNext) {
					CdIgnore.PrintUsage(player);

					return;
				}

				PlayerInfo targetInfo = PlayerDB.FindPlayerInfoOrPrintMatches(player, name);

				if(targetInfo == null) {
					return;
				}

				if(player.Ignore(targetInfo)) {
					player.MessageNow("You are now ignoring {0}", targetInfo.ClassyName);
				} else {
					player.MessageNow("You are already ignoring {0}", targetInfo.ClassyName);
				}
			} else {
				PlayerInfo[] ignoreList = player.IgnoreList;

				if(ignoreList.Length > 0) {
					player.MessageNow("Ignored players: {0}", ignoreList.JoinToClassyString());
				} else {
					player.MessageNow("You are not currently ignoring anyone.");
				}

				return;
			}
		}

		private static readonly CommandDescriptor CdUnignore = new CommandDescriptor() {
			Name = "Unignore",

			Category = CommandCategory.Chat,

			IsConsoleSafe = true,

			Usage = "/Unignore PlayerName",
			Help = "Unblocks the other player from messaging you.",

			Handler = UnignoreHandler
		};

		private static void UnignoreHandler(Player player, Command cmd) {
			string name = cmd.Next();

			if(name != null) {
				if(cmd.HasNext) {
					CdUnignore.PrintUsage(player);

					return;
				}

				PlayerInfo targetInfo = PlayerDB.FindPlayerInfoOrPrintMatches(player, name);

				if(targetInfo == null) {
					return;
				}

				if(player.Unignore(targetInfo)) {
					player.MessageNow("You are no longer ignoring {0}", targetInfo.ClassyName);
				} else {
					player.MessageNow("You are not currently ignoring {0}", targetInfo.ClassyName);
				}
			} else {
				PlayerInfo[] ignoreList = player.IgnoreList;

				if(ignoreList.Length > 0) {
					player.MessageNow("Ignored players: {0}", ignoreList.JoinToClassyString());
				} else {
					player.MessageNow("You are not currently ignoring anyone.");
				}

				return;
			}
		}
		#endregion

		#region ME

		private static readonly CommandDescriptor CdMe = new CommandDescriptor() {
			Name = "Me",

			Category = CommandCategory.Chat,

			Permissions = new Permission[] {
				Permission.Chat
			},

			IsConsoleSafe = true,
			NotRepeatable = true,
			DisableLogging = true,

			Usage = "/Me Message",
			Help = "&SSends IRC-style action message prefixed with your name.",

			Handler = MeHandler
		};

		private static void MeHandler(Player player, Command cmd) {
			if(!player.MaySpeakFurther()) return;

			string msg = cmd.NextAll().Trim();

			if(msg.Length > 0) {
				Chat.SendMe(player, msg);
			} else {
				CdMe.PrintUsage(player);
			}
		}
		#endregion

		#region ROLL

		private static readonly CommandDescriptor CdRoll = new CommandDescriptor() {
			Name = "Roll",

			Category = CommandCategory.Chat,

			Permissions = new Permission[] {
				Permission.Chat
			},

			IsConsoleSafe = true,

			Help = "Gives random number between 1 and 100.\n" +
			"&H/Roll MaxNumber\n" +
			"&S Gives number between 1 and max.\n" +
			"&H/Roll MinNumber MaxNumber\n" +
			"&S Gives number between min and max.",

			Handler = RollHandler
		};

		private static void RollHandler(Player player, Command cmd) {
			if(!player.MaySpeakFurther()) {
				return;
			}

			Random rand = new Random();

			int number1 = 0;
			int min = 0, max = 0;

			if(cmd.NextInt(out number1)) {
				int number2 = 0;

				if(!cmd.NextInt(out number2)) {
					number2 = 1;
				}

				min = Math.Min(number1, number2);
				max = Math.Max(number1, number2);
			} else {
				min = 1;
				max = 100;
			}

			int finalNumber = rand.Next(min, max + 1);

			Server.Message(player, "{0}{1} rolled {2}({3}...{4})",
						   player.ClassyName, Color.Silver,
						   finalNumber, min,
						   max);

			player.Message("{0}You rolled {1}({2}...{3})", Color.Silver,
						   finalNumber, min,
						   max);
		}
		#endregion

		#region SAY

		private static readonly CommandDescriptor CdSay = new CommandDescriptor() {
			Name = "Say",

			Category = CommandCategory.Chat,

			IsConsoleSafe = true,
			NotRepeatable = true,
			DisableLogging = true,

			Permissions = new Permission[] {
				Permission.Chat, Permission.Say
			},

			Usage = "/Say Message",
			Help = "&SShows a message in special color, without the player name prefix. " +
				   "Can be used for making announcements.",

			Handler = SayHandler
		};

		private static void SayHandler(Player player, Command cmd) {
			if(!player.MaySpeakFurther()) {
				return;
			}

			if(player.Can(Permission.Say)) {
				string msg = cmd.NextAll().Trim();

				if(msg.Length > 0) {
					Chat.SendSay(player, msg);
				} else {
					CdSay.PrintUsage(player);
				}
			} else {
				player.MessageNoAccess(Permission.Say);
			}
		}
		#endregion

		#region STAFF

		private static readonly CommandDescriptor CdStaff = new CommandDescriptor() {
			Name = "Staff",

			Aliases = new string[] {
				"st"
			},

			Category = CommandCategory.Chat | CommandCategory.Moderation,

			Permissions = new Permission[] {
				Permission.Chat
			},

			NotRepeatable = true,
			IsConsoleSafe = true,
			DisableLogging = true,

			Usage = "/Staff Message",
			Help = "Broadcasts your message to all operators/moderators on the server at once.",

			Handler = StaffHandler
		};

		private static void StaffHandler(Player player, Command cmd) {
			if(!player.MaySpeakFurther()) {
				return;
			}

			string message = cmd.NextAll().Trim();

			if(message.Length > 0) {
				Chat.SendStaff(player, message);
			}
		}
		#endregion

		#region TIMER

		private static readonly CommandDescriptor CdTimer = new CommandDescriptor() {
			Name = "Timer",

			Permissions = new Permission[] {
				Permission.Say
			},

			IsConsoleSafe = true,

			Category = CommandCategory.Chat,

			Usage = "/Timer <Duration> <Message>",
			Help = "&SStarts a timer with a given duration and message. " +
				   "As the timer counts down, announcements are shown globally. See also: &H/Help Timer Abort",

			HelpSections = new Dictionary<string, string> {{
					"abort", "&H/Timer Abort <TimerID>\n&S" +
							 "Aborts a timer with the given ID number. " +
							 "To see a list of timers and their IDs, type &H/Timer&S(without any parameters)."
			}},

			Handler = TimerHandler
		};

		private static void TimerHandler(Player player, Command cmd) {
			string param = cmd.Next();

			// List timers.
			if(param == null) {
				ChatTimer[] list = ChatTimer.TimerList.OrderBy(timer => timer.TimeLeft).ToArray();

				if(list.Length == 0) {
					player.Message("No timers running.");
				} else {
					player.Message("There are {0} timers running:", list.Length);

					foreach(ChatTimer timer in list) {
						player.Message(" #{0} \"{1}&S\"(started by {2}, {3} left)", timer.Id,
									   timer.Message, timer.StartedBy,
									   timer.TimeLeft.ToMiniString());
					}
				}

				return;
			}

			// Abort a timer.
			if(param.Equals("abort", StringComparison.OrdinalIgnoreCase)) {
				int timerId;

				if(cmd.NextInt(out timerId)) {
					ChatTimer timer = ChatTimer.FindTimerById(timerId);

					if(timer == null || !timer.IsRunning) {
						player.Message("Given timer(#{0}) does not exist.", timerId);
					} else {
						timer.Stop();

						string abortMsg = String.Format("&Y(Timer) {0}&Y aborted a timer with {1} left: {2}",
														player.ClassyName, timer.TimeLeft.ToMiniString(),
														timer.Message);

						Chat.SendSay(player, abortMsg);
					}
				} else {
					CdTimer.PrintUsage(player);
				}

				return;
			}

			// Start a timer.
			if(!player.MaySpeakFurther()) {
				return;
			}
			
			TimeSpan duration = TimeSpan.Zero;

			if(!param.TryParseMiniTimespan(out duration)) {
				CdTimer.PrintUsage(player);

				return;
			}

			if(duration > DateTimeUtil.MaxTimeSpan) {
				player.MessageMaxTimeSpan();

				return;
			}

			if(duration < ChatTimer.MinDuration) {
				player.Message("Timer: Must be at least 1 second.");

				return;
			}

			string sayMessage = String.Empty;
			string message = cmd.NextAll();

			if(String.IsNullOrEmpty(message)) {
				sayMessage = String.Format("&Y(Timer) {0}&Y started a {1} timer",
										   player.ClassyName,
										   duration.ToMiniString());
			} else {
				sayMessage = String.Format("&Y(Timer) {0}&Y started a {1} timer: {2}",
										   player.ClassyName,
										   duration.ToMiniString(),
										   message);
			}

			Chat.SendSay(player, sayMessage);

			ChatTimer.Start(duration, message, player.Name);
		}
		#endregion
	}
}
