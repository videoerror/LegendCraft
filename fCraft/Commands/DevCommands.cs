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

namespace fCraft.Commands {

	internal static class DevCommands {

		public static void Init() {
			// CommandManager.RegisterCommand(CdSpell);
		}

		private static readonly CommandDescriptor CdSpell = new CommandDescriptor() {
			Name = "Spell",

			Category = CommandCategory.Fun,

			Permissions = new Permission[] {
				Permission.Chat
			},

			IsConsoleSafe = false,
			NotRepeatable = true,

			Usage = "/Spell",
			Help = "Penis",

			UsableByFrozenPlayers = false,

			Handler = SpellHandler,
		};

		public static SpellStartBehavior particleBehavior = new SpellStartBehavior();

		internal static void SpellHandler(Player player, Command cmd) {
			World world = player.World;
			Vector3I pos1 = player.Position.ToBlockCoords();
			Random _r = new Random();
			int n = _r.Next(8, 12);
			for(int i = 0; i < n; ++i)
			{
				double phi = -_r.NextDouble() + -player.Position.L * 2 * Math.PI;
				double ksi = -_r.NextDouble() + player.Position.R * Math.PI - Math.PI / 2.0;

				Vector3F direction = (new Vector3F((float)(Math.Cos(phi) * Math.Cos(ksi)),(float)(Math.Sin(phi) * Math.Cos(ksi)),(float)Math.Sin(ksi))).Normalize();
				world.AddPhysicsTask(new Particle(world,(pos1 + 2 * direction).Round(), direction, player, Block.Obsidian, particleBehavior), 0);
			}
		}
	}
}
