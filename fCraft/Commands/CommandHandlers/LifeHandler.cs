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

// Copyright(C) <2012> Lao Tszy(lao_tszy@yahoo.co.uk)

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
using System.Text;

namespace fCraft.Commands.CommandHandlers {

	public class LifeHandler {

		private class LifeCommand {

			public string[] Names;

			public string Help;

			public Action<Player, Command> F;
		}

		private class Param {
			public string Name;
			public string Help;

			public Action<Player, Life2DZone, string> SetValue;
		}

		private static Dictionary<string, LifeCommand> Commands = new Dictionary<string, LifeCommand>();

		private static StringBuilder AllCommands = new StringBuilder();

		private static Dictionary<string, Param> Params = new Dictionary<string, Param>();

		private static StringBuilder AllParams = new StringBuilder();

		// Static preparation.
		static LifeHandler() {
			CreateParams();
			CreateCommands();

			Commands["help"].Help += AllCommands.ToString();
		}

		private static void CreateParams() {
			AddParam(new Param() {
				Name = "Delay",
				Help = "&hDelay in msec before the next life state is drawn. Must be >=20.",

				SetValue = SetDelay
			});
			AddParam(new Param() {
				Name = "IntrDelay",
				Help = "&hIf >0 the intermediate state of the life is drawn and shown for this anount of timein msec. " +
					   "If 0 the intermediate state is not shown.",
				
				SetValue = SetHalfDelay
			});
			AddParam(new Param() {
				Name = "Normal",
				Help = "&hBlock type representing the living cell.",

				SetValue = SetNormal
			});
			AddParam(new Param() {
				Name = "Empty",
				Help = "&hBlock type representing the empty cell.",
				SetValue = SetEmpty
			});
			AddParam(new Param() {
				Name = "Dead",
				Help = "&hBlock type representing the dying cell(only relevant for intermediate state).",

				SetValue = SetDead
			});
			AddParam(new Param() {
				Name = "Newborn",
				Help = "&hBlock type representing the newborn cell(only relevant for intermediate state).",

				SetValue = SetNewborn
			});
			AddParam(new Param() {
				Name = "Torus",
				Help = "&hBoolean parameter telling if the life area must be understood as a torus" +
					   "(i.e. the top side is connected to the bottom and the left side with the right one).",
				
				SetValue = SetTorus
			});
			AddParam(new Param() {
				Name = "AutoReset",
				Help = "&hThis parameter tells if the life must be auto reset after the detection of a short-periodical state. " +
					   "Possible values are None(no), ToInitial(i), ToRandom(r).",
				
				SetValue = SetAutoReset
			});
		}

		private static void AddParam(Param p) {
			if(Params.Count > 0) {
				AllParams.Append(", ");
			}

			AllParams.Append(p.Name);

			Params.Add(p.Name.ToLower(), p);
		}

		private static void CreateCommands() {
			// Assuming each command has at least one name and no names repeating. Will throw on start if it is not the case.
			AddCommand(new LifeCommand() {
				Names = new string[] {
					"help", "h"
				},

				Help = "&hPrints help on commands. Usage: /life help <command|param>." +
					   "For the list of parameters type '/life help set'. Commands are: ",
				
				F = OnHelp
			});
			AddCommand(new LifeCommand() {
				Names = new string[] {
					"create", "new"
				},

				Help = "&hCreates a new life. Usage: /life create <name>. Then mark two blocks to define a *flat* rectangle." +
					   "The life is created stopped and with default settings. After that you can set params by 'set' command and start it by 'start' command.",
				
				F = OnCreate
			});
			AddCommand(new LifeCommand() {
				Names = new string[] {
					"delete", "del", "remove"
				},

				Help = "&hDeletes a life. Usage: /life delete <name>. If this life exists it will be stopped and removed from the map book keeping.",

				F = OnDelete
			});
			AddCommand(new LifeCommand() {
				Names = new string[] {
					"start", "run"
				},

				Help = "&hStarts a stopped life. Usage: /life start <name>. If this life exists and is stopped it will be started. Otherwise nothing happens.",

				F = OnStart
			});
			AddCommand(new LifeCommand() {
				Names = new string[] {
					"stop", "pause"
				},

				Help = "&hStops a life. Usage: /life stop <name>. If this life exists and is started it will be stopped. Otherwise nothing happens.",

				F = OnStop
			});
			AddCommand(new LifeCommand() {
				Names = new string[] {
					"set"
				},

				Help = "&hSets a life parameter. Usage: /life set <name> <param>=<value>[| <param>=<value>]." +
					   "Sets parameter 'param' value for the life 'name'. Parameters are: " + AllParams.ToString(),
				
				F = OnSet
			});
			AddCommand(new LifeCommand() {
				Names = new string[] {
					"list", "ls"
				},

				Help = "&hPrints all lifes in players world. Usage: /life list [started|stopped]",

				F = OnList
			});
			AddCommand(new LifeCommand() {
				Names = new string[] {
					"print", "cat"
				},

				Help = "&hPrints this life settings. Usage: /life print <name>",

				F = OnPrint
			});
		}

		private static void AddCommand(LifeCommand c) {
			if(Commands.Count > 0) {
				AllCommands.Append(", ");
			}

			AllCommands.Append(c.Names[0]);

			foreach(string name in c.Names) {
				Commands.Add(name.ToLower(), c);
			}
		}

		// Processing.
		private string Name;

		private World World;

		private Life2DZone Life;

		public static void ProcessCommand(Player p, Command cmd) {
			string command = cmd.Next();

			if(String.IsNullOrWhiteSpace(command)) {
				p.Message("&WLife command is missing or empty");

				return;
			}

			LifeCommand c;

			if(!Commands.TryGetValue(command.ToLower(), out c)) {
				p.Message("&WUnknown life command " + command +
						  ". &hType '/life help' for the list of commands.");

				return;
			}

			c.F(p, cmd);
		}

		private static string AliasesStr(LifeCommand cmd) {
			if(cmd.Names.Length < 2) {
				return String.Empty;
			}

			StringBuilder sb = new StringBuilder("&hAliases: ");

			for(int i = 1; i < cmd.Names.Length; ++i) {
				if(i > 1) {
					sb.Append(", ");
				}

				sb.Append(cmd.Names[i]);
			}

			return sb.Append(".").ToString();
		}

		private static void OnHelp(Player p, Command cmd) {
			string cOrP = cmd.Next();

			if(String.IsNullOrWhiteSpace(cOrP)) {
				p.Message("&hLife commands are: " + AllCommands.ToString() + ".\n" +
						  "Type '/life help <command|param> for detailed command or param info." +
						  "Type '/life help set' for the list of parameters.");

				return;
			}

			LifeCommand c = null;

			Param param = null;

			string help = String.Empty;

			if(!Commands.TryGetValue(cOrP.ToLower(), out c)) {
				if(!Params.TryGetValue(cOrP.ToLower(), out param)) {
					p.Message("&WUnknown life command/parameter " + cOrP + ". &hType '/life help' for the list of commands.");

					return;
				}

				help = param.Help;
			} else {
				help = AliasesStr(c) + c.Help;
			}

			p.Message(help);
		}

		private bool CheckAndGetLifeZone(Player p, Command cmd) {
			Life = null;

			World = null;

			Name = cmd.Next();

			if(String.IsNullOrWhiteSpace(Name)) {
				p.Message("&WLife name is missing or empty");

				return false;
			}

			World = p.World;

			if(World == null) {
				p.Message("&WYou are in limbo state. Prepare for eternal torment.");

				return false;
			}

			lock(World.SyncRoot) {
				if(World.Map == null) {
					return false;
				}

				Life = World.GetLife(Name);

				return true;
			}
		}

		private static LifeHandler GetCheckedLifeHandler(Player p, Command cmd) {
			LifeHandler handler = new LifeHandler();

			if(!handler.CheckAndGetLifeZone(p, cmd)) {
				return null;
			}

			if(handler.Life == null) {
				p.Message("&WLife " + handler.Name + " does not exist.");

				return null;
			}

			return handler;
		}

		private static void OnCreate(Player p, Command cmd) {
			LifeHandler handler = new LifeHandler();

			if(!handler.CheckAndGetLifeZone(p, cmd)) {
				return;
			}

			if(!handler.CheckWorldPermissions(p)) {
				return;
			}

			if(handler.Life != null) {
				p.Message("&WLife with such name exists already, choose another");

				return;
			}
			
			p.SelectionStart(2, handler.LifeCreateCallback, null, Permission.DrawAdvanced);

			p.MessageNow("Select life zone: place/remove a block or type /Mark to use your location.");
		}

		private void LifeCreateCallback(Player player, Vector3I[] marks, object state) {
			try {
				lock(World.SyncRoot) {
					if(!CheckWorldPermissions(player)) {
						return;
					}

					if(World.Map == null) {
						return;
					}

					// Check it again since someone could create it in between.
					if(World.GetLife(Name) != null) {
						player.Message("&WLife with such name exists already, choose another");

						return;
					}

					Life2DZone life = new Life2DZone(Name, World.Map, marks, player,
													 (player.Info.Rank.NextRankUp ?? player.Info.Rank).Name);

					if(World.TryAddLife(life)) {
						player.Message("&yLife was created. Named " + Name);
					} else {
						// Really unknown: we are under a lock so nobody could create a life with the same name in between.
						player.Message("&WCoulnd't create life for some reason unknown.");
					}
				}
			} catch(Exception e) {
				player.Message("&WCreate life error: " + e.Message);
			}
		}

		private static void OnStart(Player p, Command cmd) {
			LifeHandler handler = GetCheckedLifeHandler(p, cmd);

			if(handler == null) {
				return;
			}

			if(!handler.CheckChangePermissions(p)) {
				return;
			}

			handler.Life.Start();

			p.Message("&yLife " + handler.Life.Name + " is started");
		}

		private static void OnStop(Player p, Command cmd) {
			LifeHandler handler = GetCheckedLifeHandler(p, cmd);

			if(handler == null) {
				return;
			}

			if(!handler.CheckChangePermissions(p)) {
				return;
			}

			handler.Life.Stop();

			p.Message("&yLife " + handler.Life.Name + " is stopped");
		}

		private static void OnDelete(Player p, Command cmd) {
			LifeHandler handler = GetCheckedLifeHandler(p, cmd);

			if(handler == null) {
				return;
			}

			if(!handler.CheckChangePermissions(p)) {
				return;
			}

			handler.Life.Stop();

			handler.World.DeleteLife(handler.Name);

			p.Message("&yLife " + handler.Life.Name + " is deleted");
		}

		private static void OnList(Player p, Command cmd) {
			World world = p.World;

			if(world == null) {
				p.Message("&WYou are in limbo state. Prepare for eternal torment.");

				return;
			}

			string param = cmd.Next();

			Func<Life2DZone, bool> f = l => true;

			if(!String.IsNullOrWhiteSpace(param)) {
				if(param == "started") {
					f = l => !l.Stopped;
				} else if(param == "stopped") {
					f = l => l.Stopped;
				} else {
					p.Message("&WUnrecognised parameter " + param + ". Ignored.\n");
				}
			}

			int i = 0;

			foreach(Life2DZone life in world.GetLifes().Where(life => f(life))) {
				if(i++ > 0) {
					p.Message(", ");
				}

				p.Message((life.Stopped?"&8":"&2") + life.Name);
			}
		}

		private static void OnPrint(Player p, Command cmd) {
			LifeHandler handler = GetCheckedLifeHandler(p, cmd);

			if(handler == null) {
				return;
			}

			Life2DZone l = handler.Life;

			p.Message("&y" + l.Name + ": " + (l.Stopped ? "stopped" : "started") +
					  ", delay " + l.Delay +
					  ", intermediate delay " + l.HalfStepDelay + ", is" + (l.Torus ? "" : " not") + " on torus, " +
					  "auto reset strategy is " + Enum.GetName(typeof(AutoResetMethod), l.AutoReset) +
					  ", owner is " + l.CreatorName +
					  ", changable by " + l.MinRankToChange +
					  ", block types: " + l.Normal + " is normal, " + l.Empty + " is empty, " + l.Dead + " is dead, " + l.Newborn + " is newborn");
		}

		private static void OnSet(Player p, Command cmd) {
			LifeHandler handler = GetCheckedLifeHandler(p, cmd);

			if(handler == null) {
				return;
			}

			if(!handler.CheckChangePermissions(p)) {
				return;
			}

			string paramStr = cmd.Next();

			if(String.IsNullOrWhiteSpace(paramStr)) {
				p.Message("&WEmpty parameter name. &hAccepted names are " +
						  AllParams.ToString());

				return;
			}

			Param param = null;

			if(!Params.TryGetValue(paramStr, out param)) {
				p.Message("&WUknown parameter name" + paramStr + ". " +
						  "&hAccepted names are " + AllParams.ToString());

				return;
			}

			string val = cmd.Next();
			if(String.IsNullOrWhiteSpace(val)) {
				p.Message("&WEmpty value.");

				return;
			}

			param.SetValue(p, handler.Life, val);
		}

		private static void SetDelay(Player p, Life2DZone life, string val) {
			int delay = 0;

			if(!Int32.TryParse(val, out delay) || delay <= 20) {
				p.Message("&WExpected integer value >=20 as delay");

				return;
			}

			life.Delay = delay;

			p.Message("&yStep delay set to " + val);
		}

		private static void SetHalfDelay(Player p, Life2DZone life, string val) {
			int delay = 0;

			if(!Int32.TryParse(val, out delay) || delay < 0) {
				p.Message("&WExpected non-negative integer value as intermediate delay");

				return;
			}

			life.HalfStepDelay = delay;

			p.Message("&yIntermediate step delay set to " + val);
		}
		
		private static void SetNormal(Player p, Life2DZone life, string val) {
			Block block = Map.GetBlockByName(val);

			if(block == Block.Undefined) {
				p.Message("&WUnrecognized block name " + val);

				return;
			}

			life.Normal = block;

			p.Message("&yNormal block set to " + val);
		}

		private static void SetEmpty(Player p, Life2DZone life, string val) {
			Block block = Map.GetBlockByName(val);

			if(block == Block.Undefined) {
				p.Message("&WUnrecognized block name " + val);

				return;
			}

			life.Empty = block;

			p.Message("&yEmpty block set to " + val);
		}

		private static void SetDead(Player p, Life2DZone life, string val) {
			Block block = Map.GetBlockByName(val);

			if(block == Block.Undefined) {
				p.Message("&WUnrecognized block name " + val);

				return;
			}

			life.Dead = block;

			p.Message("&yDead block set to " + val);
		}

		private static void SetNewborn(Player p, Life2DZone life, string val) {
			Block block = Map.GetBlockByName(val);

			if(block == Block.Undefined) {
				p.Message("&WUnrecognized block name " + val);

				return;
			}

			life.Newborn = block;

			p.Message("&yNewborn block set to " + val);
		}

		private static void SetTorus(Player p, Life2DZone life, string val) {
			bool torus = false;

			if(!Boolean.TryParse(val, out torus)) {
				p.Message("&WExpected 'true' or 'false' as torus parameter value");

				return;
			}

			life.Torus = torus;

			p.Message("&yTorus param set to " + val);
		}

		private static void SetAutoReset(Player p, Life2DZone life, string val) {
			AutoResetMethod method = AutoResetMethod.None;

			val = val.ToLower();

			if(val == "no" || val == "none") {
				method = AutoResetMethod.None;
			} else if(val == "toinitial" || val == "i") {
				method = AutoResetMethod.ToInitial;
			} else if(val == "torandom" || val == "r" || val == "rnd") {
				method = AutoResetMethod.ToRandom;
			} else {
				p.Message("&WUnrecognized auto reset method " + val + ".\n" +
						  "&hType '/life help AutoReset' to see all the possible values.");

				return;
			}

			life.AutoReset = method;

			p.Message("&yAutoReset param set to " + Enum.GetName(typeof(AutoResetMethod), method));
		}

		private bool CheckWorldPermissions(Player p) {
			if(!p.Info.Rank.AllowSecurityCircumvention) {
				SecurityCheckResult buildCheck = World.BuildSecurity.CheckDetailed(p.Info);

				switch(buildCheck) {
					case SecurityCheckResult.BlackListed:
						p.Message("Cannot add life to world {0}&S: You are barred from building here.",
								  p.ClassyName);

						return false;
					
					case SecurityCheckResult.RankTooLow:
						p.Message("Cannot add life to world {0}&S: You are not allowed to build here.",
								  p.ClassyName);

						return false;
				}
			}

			return true;
		}

		private bool CheckChangePermissions(Player p) {
			if(String.IsNullOrWhiteSpace(Life.CreatorName) || p.Name == Life.CreatorName) {
				return true;
			}

			if(String.IsNullOrWhiteSpace(Life.MinRankToChange)) {
				return true;
			}

			Rank rank = null;

			if(!RankManager.RanksByName.TryGetValue(Life.MinRankToChange, out rank)) {
				string prevRank = Life.MinRankToChange;

				rank = RankManager.LowestRank.NextRankUp ?? RankManager.LowestRank;

				Life.MinRankToChange = rank.Name;

				p.Message("&WRank "+prevRank+" couldn't be found. Updated to " + rank.Name);
			}

			if(p.Info.Rank >= rank) {
				return true;
			}

			p.Message("&WYour rank is too low to change this life.");

			return false;
		}
	}
}
