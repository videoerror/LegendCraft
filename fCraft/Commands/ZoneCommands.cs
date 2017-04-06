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
using System.Linq;
using fCraft.MapConversion;
using System.Collections.Generic;
using fCraft.Events;

namespace fCraft.Commands {

	/// <summary>
	/// Contains commands related to zone management.
	/// </summary>
	internal static class ZoneCommands {

		internal static void Init() {
			CommandManager.RegisterCommand(CdDoorCheck);
			CommandManager.RegisterCommand(CdDoorList);

			CommandManager.RegisterCustomCommand(CdDoor);
			CommandManager.RegisterCustomCommand(CdDoorRemove);

			Player.Clicked += PlayerClickedDoor;

			CommandManager.RegisterCommand(CdZoneAdd);
			CommandManager.RegisterCommand(CdZoneEdit);
			CommandManager.RegisterCommand(CdZoneInfo);
			CommandManager.RegisterCommand(CdZoneList);
			CommandManager.RegisterCommand(CdZoneMark);
			CommandManager.RegisterCommand(CdZoneRemove);
			CommandManager.RegisterCommand(CdZoneRename);
			CommandManager.RegisterCommand(CdZoneTest);
		}

		private static readonly TimeSpan DoorCloseTimer = TimeSpan.FromMilliseconds(1500);

		// Change for max door area.
		private const int MaxDoorBlocks = 30;

		private static List<Zone> OpenDoors = new List<Zone>();

		internal struct DoorInfo {
			public readonly Zone Zone;

			public readonly Block[] Buffer;

			public readonly Map WorldMap;

			public DoorInfo(Zone zone, Block[] buffer, Map worldMap) {
				Zone = zone;
				Buffer = buffer;
				WorldMap = worldMap;
			}
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

		private static readonly CommandDescriptor CdDoorCheck = new CommandDescriptor() {
			Name = "DoorCheck",
			Usage = "/DoorCheck",

			Category = CommandCategory.Zone,

			Permissions = new Permission[] {
				Permission.Build
			},

			Help = "Allows you to identify a door's name. Left click to check doors. " +
				   "To remove a door, use /DoorRemove. To list doors on a world, use /DoorList. " +
				   "To create a door, use /Door.",

			Handler = DoorCheckH
		};

		private static void DoorCheckH(Player player, Command cmd) {
			player.Message("Left click to select a door.");

			player.Info.isDoorChecking = true;

			player.Info.doorCheckTime = DateTime.UtcNow;
		}

		private static readonly CommandDescriptor CdDoorList = new CommandDescriptor() {
			Name = "DoorList",
			Usage = "/DoorList [world]",

			Category = CommandCategory.Zone,

			Permissions = new Permission[] {
				Permission.Build
			},

			Help = "Lists all doors in the target world. Leave world blank to list current world. " +
				   "To remove a door, use /DoorRemove. To create a door, use /Door. " +
				   "To check a door, use /DoorCheck.",

			Handler = DoorListH
		};

		private static void DoorListH(Player player, Command cmd) {
			// If no world is given, list doors on current world.
			string world = cmd.Next();

			World targetWorld = null;

			if(String.IsNullOrEmpty(world)) {
				targetWorld = player.World;
			} else {
				targetWorld = WorldManager.FindWorldExact(world);

				if(targetWorld == null) {
					player.Message("Could not find world '{0}'!", world);

					return;
				}
			}

			player.Message("__Doors on {0}__", targetWorld.Name);

			var doors = from d in targetWorld.Map.Zones
						where d.Name.StartsWith("Door_")
						select d;

			// Loop through each door zone and print it out.
			foreach(Zone zone in doors) {
				player.Message(zone.Name);
			}
		}
		#endregion

		#region DOOR

		private static readonly CommandDescriptor CdDoor = new CommandDescriptor() {
			Name = "Door",
			Usage = "/Door [Name]",

			Category = CommandCategory.Zone,

			Permissions = new Permission[] {
				Permission.Build
			},

			Help = "Creates door zone. Left click to open doors. To remove a door, use /DoorRemove. " +
				   "To list doors on a world, use /DoorList. To check a door, use /DoorCheck.",

			Handler = Door
		};

		private static void Door(Player player, Command cmd) {
			string name = cmd.Next();

			if(String.IsNullOrEmpty(name)) {
				player.Message("You must have a name for your door! Usage is /Door [name]");

				return;
			}

			if(player.WorldMap.Zones.FindExact("Door_" + name) != null) {
				player.Message("There is a door on this world with that name already!");

				return;
			}

			Zone door = new Zone();

			door.Name = "Door_" + name;

			player.SelectionStart(2, DoorAdd, door, CdDoor.Permissions);

			player.Message("Door: Place a block or type /mark to use your location.");
		}

		private static void DoorAdd(Player player, Vector3I[] marks, object tag) {
			int sx = Math.Min(marks[0].X, marks[1].X);
			int ex = Math.Max(marks[0].X, marks[1].X);
			int sy = Math.Min(marks[0].Y, marks[1].Y);
			int ey = Math.Max(marks[0].Y, marks[1].Y);
			int sh = Math.Min(marks[0].Z, marks[1].Z);
			int eh = Math.Max(marks[0].Z, marks[1].Z);
			int volume = (ex - sx + 1) *(ey - sy + 1) *(eh - sh + 1);

			if(volume > MaxDoorBlocks) {
				player.Message("Doors are only allowed to be {0} blocks", MaxDoorBlocks);

				return;
			}

			Zone door = (Zone)tag;

			door.Create(new BoundingBox(marks[0], marks[1]), player.Info);

			player.WorldMap.Zones.Add(door);

			Logger.Log(LogType.UserActivity,
					   "{0} created door {1}(on world {2})",
					   player.Name,
					   door.Name,
					   player.World.Name);

			player.Message("Door created: {0}x{1}x{2}",
						   door.Bounds.Dimensions.X,
						   door.Bounds.Dimensions.Y,
						   door.Bounds.Dimensions.Z);
		}
		#endregion

		#region DOOR_REMOVE

		private static readonly CommandDescriptor CdDoorRemove = new CommandDescriptor() {
			Name = "DoorRemove",
			Usage = "/DoorRemove [name]",

			Aliases = new string[] {
				"rd", "RemoveDoor"
			},

			Category = CommandCategory.Zone,

			Permissions = new Permission[] {
				Permission.Build
			},

			Help = "Removes a door. To create a door, use /Door. To list doors on a world, use /DoorList. " +
				   "To check a door, use /DoorCheck.",

			Handler = DoorRemove
		};

		private static void DoorRemove(Player player, Command cmd) {
			Zone zone = null;

			string name = cmd.Next();

			if(String.IsNullOrEmpty(name)) {
				player.Message("You must have a name for your door to remove! Usage is /DoorRemove [name]");

				return;
			}

			if(name.StartsWith("Door_")) {
				name = name.Substring(5);
			}

			if((zone = player.WorldMap.Zones.FindExact("Door_" + name)) != null) {
				player.WorldMap.Zones.Remove(zone);

				player.Message("Door removed.");
			} else {
				player.Message("Could not find door: " + name + " on this map!");
			}
		}
		#endregion

		private static readonly object OpenDoorsLock = new object();

		public static void PlayerClickedDoor(object sender, PlayerClickedEventArgs e) {
			// After 10s, revert effects of /DoorCheck.
			if((DateTime.UtcNow - e.Player.Info.doorCheckTime).TotalSeconds > 10 &&
			   e.Player.Info.doorCheckTime != DateTime.MaxValue) {
				e.Player.Info.doorCheckTime = DateTime.MaxValue;

				e.Player.Info.isDoorChecking = false;
			}

			Zone[] allowed = null, denied = null;

			if(e.Player.WorldMap.Zones.CheckDetailed(e.Coords, e.Player,
													 out allowed, out denied)) {
				foreach(Zone zone in allowed) {
					if(zone.Name.StartsWith("Door_")) {
						Player.RaisePlayerPlacedBlockEvent(e.Player, e.Player.WorldMap,
														   e.Coords, e.Block,
														   e.Block, BlockChangeContext.Manual);

						// If player is checking a door, print the door info instead of opening it.
						if(e.Player.Info.isDoorChecking) {
							e.Player.Message(zone.Name);

							e.Player.Message("Created by {0} on {1}",
											 zone.CreatedBy,
											 zone.CreatedDate);

							return;
						}

						lock(OpenDoorsLock) {
							if(!OpenDoors.Contains(zone)) {
								OpenDoor(zone, e.Player);

								OpenDoors.Add(zone);
							}
						}
					}
				}
			}

		}

		private static void OpenDoor(Zone zone, Player player) {
			int sx = zone.Bounds.XMin;
			int ex = zone.Bounds.XMax;
			int sy = zone.Bounds.YMin;
			int ey = zone.Bounds.YMax;
			int sz = zone.Bounds.ZMin;
			int ez = zone.Bounds.ZMax;

			Block[] buffer = new Block[zone.Bounds.Volume];

			int counter = 0;

			for(int x = sx; x <= ex; x++) {
				for(int y = sy; y <= ey; y++) {
					for(int z = sz; z <= ez; z++) {
						buffer[counter] = player.WorldMap.GetBlock(x, y, z);

						player.WorldMap.QueueUpdate(new BlockUpdate(null, new Vector3I(x, y, z), Block.Air));

						counter++;
					}
				}
			}

			DoorInfo info = new DoorInfo(zone, buffer, player.WorldMap);

			// Reclose door.
			Scheduler.NewTask(DoorTimerElapsed).RunOnce(info, DoorCloseTimer);
		}

		private static void DoorTimerElapsed(SchedulerTask task) {
			DoorInfo info = (DoorInfo)task.UserState;

			int counter = 0;

			for(int x = info.Zone.Bounds.XMin; x <= info.Zone.Bounds.XMax; x++) {
				for(int y = info.Zone.Bounds.YMin; y <= info.Zone.Bounds.YMax; y++) {
					for(int z = info.Zone.Bounds.ZMin; z <= info.Zone.Bounds.ZMax; z++) {
						info.WorldMap.QueueUpdate(new BlockUpdate(null, new Vector3I(x, y, z), info.Buffer[counter]));

						counter++;
					}
				}
			}

			lock(OpenDoorsLock) {
				OpenDoors.Remove(info.Zone);
			}
		}

		#region ZONE_ADD

		private static readonly CommandDescriptor CdZoneAdd = new CommandDescriptor() {
			Name = "ZAdd",

			Category = CommandCategory.Zone,

			Aliases = new string[] {
				"zone"
			},

			Permissions = new Permission[] {
				Permission.ManageZones
			},

			Usage = "/ZAdd ZoneName RankName [{+|-}PlayerName] msg=CustomDenyMessage",
			Help = "Create a zone that overrides build permissions. " +
				   "This can be used to restrict access to an area(by setting RankName to a high rank) " +
				   "or to designate a guest area(by lowering RankName).",

			Handler = ZoneAddHandler
		};

		private static void ZoneAddHandler(Player player, Command cmd) {
			World playerWorld = player.World;

			if(playerWorld == null) {
				PlayerOpException.ThrowNoWorld(player);
			}

			string givenZoneName = cmd.Next();

			if(givenZoneName == null) {
				CdZoneAdd.PrintUsage(player);

				return;
			}

			if(!player.Info.Rank.AllowSecurityCircumvention) {
				SecurityCheckResult buildCheck = playerWorld.BuildSecurity.CheckDetailed(player.Info);

				switch(buildCheck) {
					case SecurityCheckResult.BlackListed:
						player.Message("Cannot add zones to world {0}&S: You are barred from building here.",
									   playerWorld.ClassyName);

						return;
					
					case SecurityCheckResult.RankTooLow:
						player.Message("Cannot add zones to world {0}&S: You are not allowed to build here.",
									   playerWorld.ClassyName);

						return;
					
					// case SecurityCheckResult.RankTooHigh:
				}
			}

			Zone newZone = new Zone();

			ZoneCollection zoneCollection = player.WorldMap.Zones;

			if(givenZoneName.StartsWith("+")) {
				// Personal zone(/ZAdd +Name).
				givenZoneName = givenZoneName.Substring(1);

				// Find the target player.
				PlayerInfo info = PlayerDB.FindPlayerInfoOrPrintMatches(player, givenZoneName);

				if(info == null) {
					return;
				}

				// Make sure that the name is not taken already.
				// If a zone named after the player already exists, try adding a number after the name(e.g. "Notch2").
				newZone.Name = info.Name;

				for(int i = 2; zoneCollection.Contains(newZone.Name); i++) {
					newZone.Name = givenZoneName + i;
				}

				newZone.Controller.MinRank = info.Rank.NextRankUp ?? info.Rank;

				newZone.Controller.Include(info);

				player.Message("Zone: Creating a {0}+&S zone for player {1}&S.",
							   newZone.Controller.MinRank.ClassyName,
							   info.ClassyName);
			} else {
				// Adding an ordinary, rank-restricted zone.
				if(!World.IsValidName(givenZoneName)) {
					player.Message("\"{0}\" is not a valid zone name", givenZoneName);

					return;
				}

				if(zoneCollection.Contains(givenZoneName)) {
					player.Message("A zone with this name already exists. Use &H/ZEdit&S to edit.");

					return;
				}

				newZone.Name = givenZoneName;

				string rankName = cmd.Next();

				if(rankName == null) {
					player.Message("No rank was specified. See &H/Help zone");

					return;
				}

				Rank minRank = RankManager.FindRank(rankName);

				if(minRank != null) {
					string name = null;

					while((name = cmd.Next()) != null) {
						if(name.Length == 0) {
							continue;
						}

						if(name.ToLower().StartsWith("msg=")) {
							newZone.Message = name.Substring(4) + " " + (cmd.NextAll() ?? String.Empty);

							player.Message("Zone: Custom denied messaged changed to '" + newZone.Message + "'");

							break;
						}

						PlayerInfo info = PlayerDB.FindPlayerInfoOrPrintMatches(player, name.Substring(1));

						if(info == null) {
							return;
						}

						if(name.StartsWith("+")) {
							newZone.Controller.Include(info);
						} else if(name.StartsWith("-")) {
							newZone.Controller.Exclude(info);
						}
					}

					newZone.Controller.MinRank = minRank;
				} else {
					player.MessageNoRank(rankName);

					return;
				}
			}

			player.Message("Zone "+ newZone.ClassyName + "&S: Place a block or type &H/Mark&S to use your location.");
			player.SelectionStart(2, ZoneAddCallback, newZone, CdZoneAdd.Permissions);
		}

		private static void ZoneAddCallback(Player player, Vector3I[] marks, object tag) {
			World playerWorld = player.World;

			if(playerWorld == null) {
				PlayerOpException.ThrowNoWorld(player);
			}

			if(!player.Info.Rank.AllowSecurityCircumvention) {
				SecurityCheckResult buildCheck = playerWorld.BuildSecurity.CheckDetailed(player.Info);

				switch(buildCheck) {
					case SecurityCheckResult.BlackListed:
						player.Message("Cannot add zones to world {0}&S: You are barred from building here.",
									   playerWorld.ClassyName);

						return;
					
					case SecurityCheckResult.RankTooLow:
						player.Message("Cannot add zones to world {0}&S: You are not allowed to build here.",
									   playerWorld.ClassyName);

						return;
					
					// case SecurityCheckResult.RankTooHigh:
				}
			}

			Zone zone = (Zone)tag;

			var zones = player.WorldMap.Zones;

			lock(zones.SyncRoot) {
				Zone dupeZone = zones.FindExact(zone.Name);

				if(dupeZone != null) {
					player.Message("A zone named \"{0}\" has just been created by {1}",
								   dupeZone.Name,
								   dupeZone.CreatedBy);

					return;
				}

				zone.Create(new BoundingBox(marks[0], marks[1]), player.Info);

				player.Message("Zone \"{0}\" created, {1} blocks total.",
							   zone.Name,
							   zone.Bounds.Volume);

				Logger.Log(LogType.UserActivity,
						   "Player {0} created a new zone \"{1}\" containing {2} blocks.",
						   player.Name,
						   zone.Name,
						   zone.Bounds.Volume);

				zones.Add(zone);
			}
		}
		#endregion

		#region ZONE_EDIT

		private static readonly CommandDescriptor CdZoneEdit = new CommandDescriptor() {
			Name = "ZEdit",

			Category = CommandCategory.Zone,

			Permissions = new Permission[] {
				Permission.ManageZones
			},

			Usage = "/ZEdit ZoneName [RankName] [+IncludedName] [-ExcludedName] [msg=CustomDenyMessage]",
			Help = "&SAllows editing the zone permissions after creation. " +
				   "You can change the rank restrictions, and include or exclude individual players."+
				   " The custom deny message must be the last pararameter since the rest of the command past the 'msg=' will be considered as the new message",

			Handler = ZoneEditHandler
		};

		private static void ZoneEditHandler(Player player, Command cmd) {
			bool changesWereMade = false;

			string zoneName = cmd.Next();

			if(zoneName == null) {
				player.Message("No zone name specified. See &H/Help ZEdit");

				return;
			}

			Zone zone = player.WorldMap.Zones.Find(zoneName);

			if(zone == null) {
				player.MessageNoZone(zoneName);

				return;
			}

			string name = null;

			while((name = cmd.Next()) != null) {
				if(name.StartsWith("+")) {
					if(name.Length == 1) {
						CdZoneEdit.PrintUsage(player);

						break;
					}

					PlayerInfo info = PlayerDB.FindPlayerInfoOrPrintMatches(player, name.Substring(1));

					if(info == null) {
						return;
					}

					// Prevent players from whitelisting themselves to bypass protection.
					if(!player.Info.Rank.AllowSecurityCircumvention && player.Info == info) {
						if(!zone.Controller.Check(info)) {
							player.Message("You must be {0}+&S to add yourself to this zone's whitelist.",
										   zone.Controller.MinRank.ClassyName);

							continue;
						}
					}

					switch(zone.Controller.Include(info)) {
						case PermissionOverride.None:
							player.Message("{0}&S is now included in zone {1}",
										   info.ClassyName,
										   zone.ClassyName);

							changesWereMade = true;

							break;
						
						case PermissionOverride.Allow:
							player.Message("{0}&S is already included in zone {1}",
										   info.ClassyName,
										   zone.ClassyName);

							break;
						
						case PermissionOverride.Deny:
							player.Message("{0}&S is no longer excluded from zone {1}",
										   info.ClassyName,
										   zone.ClassyName);

							changesWereMade = true;

							break;
					}
				} else if(name.StartsWith("-")) {
					if(name.Length == 1) {
						CdZoneEdit.PrintUsage(player);

						break;
					}

					PlayerInfo info = PlayerDB.FindPlayerInfoOrPrintMatches(player, name.Substring(1));

					if(info == null) {
						return;
					}

					switch(zone.Controller.Exclude(info)) {
						case PermissionOverride.None:
							player.Message("{0}&S is now excluded from zone {1}",
										   info.ClassyName,
										   zone.ClassyName);

							changesWereMade = true;

							break;
						
						case PermissionOverride.Allow:
							player.Message("{0}&S is no longer included in zone {1}",
										   info.ClassyName,
										   zone.ClassyName);

							changesWereMade = true;

							break;
						
						case PermissionOverride.Deny:
							player.Message("{0}&S is already excluded from zone {1}",
										   info.ClassyName,
										   zone.ClassyName);

							break;
					}
				} else if(name.ToLower().StartsWith("msg=")) {
					zone.Message = name.Substring(4) + " " + (cmd.NextAll() ?? String.Empty);

					changesWereMade = true;

					player.Message("Zedit: Custom denied messaged changed to '" + zone.Message + "'");

					break;
				} else {
					Rank minRank = RankManager.FindRank(name);

					if(minRank != null) {
						// Prevent players from lowering rank so bypass protection.
						if(!player.Info.Rank.AllowSecurityCircumvention &&
							zone.Controller.MinRank > player.Info.Rank && minRank <= player.Info.Rank) {
							player.Message("You are not allowed to lower the zone's rank.");

							continue;
						}

						if(zone.Controller.MinRank != minRank) {
							zone.Controller.MinRank = minRank;
							player.Message("Permission for zone \"{0}\" changed to {1}+",
										   zone.Name,
										   minRank.ClassyName);

							changesWereMade = true;
						}
					} else {
						player.MessageNoRank(name);
					}
				}
			}

			if(changesWereMade) {
				zone.Edit(player.Info);
			} else {
				player.Message("No changes were made to the zone.");
			}
		}
		#endregion


		#region ZONE_INFO

		private static readonly CommandDescriptor CdZoneInfo = new CommandDescriptor() {
			Name = "ZInfo",

			Aliases = new string[] {
				"ZoneInfo"
			},

			Category = CommandCategory.Zone | CommandCategory.Info,

			Help = "Shows detailed information about a zone.",
			Usage = "/ZInfo ZoneName",

			UsableByFrozenPlayers = true,

			Handler = ZoneInfoHandler
		};

		private static void ZoneInfoHandler(Player player, Command cmd) {
			string zoneName = cmd.Next();

			if(zoneName == null) {
				player.Message("No zone name specified. See &H/Help ZInfo");

				return;
			}

			Zone zone = player.WorldMap.Zones.Find(zoneName);

			if(zone == null) {
				player.MessageNoZone(zoneName);

				return;
			}

			player.Message("About zone \"{0}\": size {1} x {2} x {3}, contains {4} blocks, editable by {5}+.",
						   zone.Name,
						   zone.Bounds.Width,
						   zone.Bounds.Length,
						   zone.Bounds.Height,
						   zone.Bounds.Volume,
						   zone.Controller.MinRank.ClassyName);
			player.Message("  Zone center is at({0},{1},{2}).",
						   (zone.Bounds.XMin + zone.Bounds.XMax) / 2,
						   (zone.Bounds.YMin + zone.Bounds.YMax) / 2,
						   (zone.Bounds.ZMin + zone.Bounds.ZMax) / 2);

			if(zone.CreatedBy != null) {
				player.Message("  Zone created by {0}&S on {1:MMM d} at {1:h:mm}({2} ago).",
							   zone.CreatedByClassy,
							   zone.CreatedDate,
							   DateTime.UtcNow.Subtract(zone.CreatedDate).ToMiniString());
			}

			if(zone.EditedBy != null) {
				player.Message("  Zone last edited by {0}&S on {1:MMM d} at {1:h:mm}({2}d {3}h ago).",
							   zone.EditedByClassy,
							   zone.EditedDate,
							   DateTime.UtcNow.Subtract(zone.EditedDate).Days,
							   DateTime.UtcNow.Subtract(zone.EditedDate).Hours);
			}

			PlayerExceptions zoneExceptions = zone.ExceptionList;

			if(zoneExceptions.Included.Length > 0) {
				player.Message("  Zone whitelist includes: {0}",
							   zoneExceptions.Included.JoinToClassyString());
			}

			if(zoneExceptions.Excluded.Length > 0) {
				player.Message("  Zone blacklist excludes: {0}",
							   zoneExceptions.Excluded.JoinToClassyString());
			}

			if(null != zone.Message) {
				player.Message("  Zone has custom deny build message: " +
					zone.Message);
			} else {
				player.Message("  Zone has no custom deny build message");
			}
		}
		#endregion

		#region ZONE_LIST

		private static readonly CommandDescriptor CdZoneList = new CommandDescriptor() {
			Name = "Zones",

			Category = CommandCategory.Zone | CommandCategory.Info,

			IsConsoleSafe = true,
			UsableByFrozenPlayers = true,

			Usage = "/Zones [WorldName]",
			Help = "&SLists all zones defined on the current map/world.",

			Handler = ZoneListHandler
		};

		private static void ZoneListHandler(Player player, Command cmd) {
			World world = player.World;

			string worldName = cmd.Next();

			if(worldName != null) {
				world = WorldManager.FindWorldOrPrintMatches(player, worldName);

				if(world == null) {
					return;
				}

				player.Message("List of zones on {0}&S:",
							   world.ClassyName);

			} else if(world != null) {
				player.Message("List of zones on this world:");

			} else {
				player.Message("When used from console, &H/Zones&S command requires a world name.");

				return;
			}

			Map map = world.Map;

			if(map == null) {
				if(!MapUtility.TryLoadHeader(world.MapFileName, out map)) {
					player.Message("&WERROR:Could not load mapfile for world {0}.",
								   world.ClassyName);

					return;
				}
			}

			Zone[] zones = map.Zones.Cache;

			if(zones.Length > 0) {
				foreach(Zone zone in zones) {
					player.Message("   {0}({1}&S) - {2} x {3} x {4}",
								   zone.Name,
								   zone.Controller.MinRank.ClassyName,
								   zone.Bounds.Width,
								   zone.Bounds.Length,
								   zone.Bounds.Height);
				}

				player.Message("   Type &H/ZInfo ZoneName&S for details.");
			} else {
				player.Message("   No zones defined.");
			}
		}
		#endregion

		#region ZONE_MARK

		private static readonly CommandDescriptor CdZoneMark = new CommandDescriptor() {
			Name = "ZMark",

			Category = CommandCategory.Zone | CommandCategory.Building,

			Usage = "/ZMark ZoneName",
			Help = "&SUses zone boundaries to make a selection.",

			Handler = ZoneMarkHandler
		};

		private static void ZoneMarkHandler(Player player, Command cmd) {
			if(player.SelectionMarksExpected == 0) {
				player.MessageNow("Cannot use ZMark - no selection in progress.");
			} else if(player.SelectionMarksExpected == 2) {
				string zoneName = cmd.Next();

				if(zoneName == null) {
					CdZoneMark.PrintUsage(player);

					return;
				}

				Zone zone = player.WorldMap.Zones.Find(zoneName);

				if(zone == null) {
					player.MessageNoZone(zoneName);

					return;
				}

				player.SelectionResetMarks();

				player.SelectionAddMark(zone.Bounds.MinVertex, false);
				player.SelectionAddMark(zone.Bounds.MaxVertex, true);
			} else {
				player.MessageNow("ZMark can only be used for 2-block selection.");
			}
		}
		#endregion

		#region ZONE_REMOVE

		private static readonly CommandDescriptor CdZoneRemove = new CommandDescriptor() {
			Name = "ZRemove",

			Aliases = new string[] {
				"zdelete"
			},

			Category = CommandCategory.Zone,

			Permissions = new Permission[] {
				Permission.ManageZones
			},

			Usage = "/ZRemove ZoneName",
			Help = "Removes a zone with the specified name from the map.",

			Handler = ZoneRemoveHandler
		};

		private static void ZoneRemoveHandler(Player player, Command cmd) {
			string zoneName = cmd.Next();

			if(zoneName == null) {
				CdZoneRemove.PrintUsage(player);

				return;
			}

			ZoneCollection zones = player.WorldMap.Zones;

			Zone zone = zones.Find(zoneName);

			if(zone != null) {
				if(!player.Info.Rank.AllowSecurityCircumvention) {
					switch(zone.Controller.CheckDetailed(player.Info)) {
						case SecurityCheckResult.BlackListed:
							player.Message("You are not allowed to remove zone {0}: you are blacklisted.",
										   zone.ClassyName);

							return;
						
						case SecurityCheckResult.RankTooLow:
							player.Message("You are not allowed to remove zone {0}.",
										   zone.ClassyName);

							return;
					}
				}

				if(!cmd.IsConfirmed) {
					player.Confirm(cmd, "Remove zone {0}&S?", zone.ClassyName);

					return;
				}

				if(zones.Remove(zone.Name)) {
					player.Message("Zone \"{0}\" removed.", zone.Name);
				}
			} else {
				player.MessageNoZone(zoneName);
			}
		}
		#endregion

		#region ZONE_RENAME

		private static readonly CommandDescriptor CdZoneRename = new CommandDescriptor() {
			Name = "ZRename",

			Category = CommandCategory.Zone,

			Permissions = new Permission[] {
				Permission.ManageZones
			},

			Help = "&SRenames a zone",
			Usage = "/ZRename OldName NewName",

			Handler = ZoneRenameHandler
		};

		private static void ZoneRenameHandler(Player player, Command cmd) {
			World playerWorld = player.World;

			if(playerWorld == null) {
				PlayerOpException.ThrowNoWorld(player);
			}

			// Make sure that both parameters are given.
			string oldName = cmd.Next();
			string newName = cmd.Next();

			if(oldName == null || newName == null) {
				CdZoneRename.PrintUsage(player);

				return;
			}

			// Make sure that the new name is valid.
			if(!World.IsValidName(newName)) {
				player.Message("\"{0}\" is not a valid zone name", newName);

				return;
			}

			// Find the old zone.
			ZoneCollection zones = player.WorldMap.Zones;

			Zone oldZone = zones.Find(oldName);

			if(oldZone == null) {
				player.MessageNoZone(oldName);

				return;
			}

			// Check if a zone with "newName" name already exists.
			Zone newZone = zones.FindExact(newName);
			if(newZone != null && newZone != oldZone) {
				player.Message("A zone with the name \"{0}\" already exists.", newName);

				return;
			}

			// Check if any change is needed.
			string fullOldName = oldZone.Name;

			if(fullOldName == newName) {
				player.Message("The zone is already named \"{0}\"", fullOldName);

				return;
			}

			// Actually rename the zone.
			zones.Rename(oldZone, newName);

			// Announce the rename.
			playerWorld.Players.Message("&SZone \"{0}\" was renamed to \"{1}&S\" by {2}",
										fullOldName,
										oldZone.ClassyName,
										player.ClassyName);

			Logger.Log(LogType.UserActivity,
					   "Player {0} renamed zone \"{1}\" to \"{2}\" on world {3}",
					   player.Name,
					   fullOldName,
					   newName,
					   playerWorld.Name);
		}
		#endregion

		#region ZONE_TEST

		private static readonly CommandDescriptor CdZoneTest = new CommandDescriptor() {
			Name = "ZTest",

			Category = CommandCategory.Zone | CommandCategory.Info,

			RepeatableSelection = true,

			Help = "&SAllows to test exactly which zones affect a particular block. Can be used to find and resolve zone overlaps.",

			Handler = ZoneTestHandler
		};

		private static void ZoneTestHandler(Player player, Command cmd) {
			player.SelectionStart(1, ZoneTestCallback, null);

			player.Message("Click the block that you would like to test.");
		}

		private static void ZoneTestCallback(Player player, Vector3I[] marks, object tag) {
			Zone[] allowed, denied;

			if(player.WorldMap.Zones.CheckDetailed(marks[0], player, out allowed, out denied)) {
				foreach(Zone zone in allowed) {
					SecurityCheckResult status = zone.Controller.CheckDetailed(player.Info);

					player.Message("> Zone {0}&S: {1}{2}", zone.ClassyName, Color.Lime, status);
				}

				foreach(Zone zone in denied) {
					SecurityCheckResult status = zone.Controller.CheckDetailed(player.Info);

					player.Message("> Zone {0}&S: {1}{2}", zone.ClassyName, Color.Red, status);
				}
			} else {
				player.Message("No zones affect this block.");
			}
		}
		#endregion
	}
}
