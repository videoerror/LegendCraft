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

namespace fCraft.Commands {

	/// <summary>
	/// Command categories. A command may belong to more than one category.
	/// Use binary flag logic(value & flag == flag) to test whether a command belongs to a particular category.
	/// </summary>
	[Flags]
	public enum CommandCategory {

		/// <summary>
		/// Default command category. Do not use it.
		/// </summary>
		None = 0,
		/// <summary>
		/// Building-related commands: drawing, binding, copy/paste.
		/// </summary>
		Building = 1,
		/// <summary>
		/// Chat-related commands: messaging, ignoring, muting, etc.
		/// </summary>
		Chat = 2,
		/// <summary>
		/// Information commands: server, world, zone, rank, and player infos.
		/// </summary>
		Info = 4,
		/// <summary>
		/// Moderation commands: kick, ban, rank, tp/bring, etc.
		/// </summary>
		Moderation = 8,
		/// <summary>
		/// Server maintenance commands: reloading configs, editing PlayerDB, importing data, etc.
		/// </summary>
		Maintenance = 16,
		/// <summary>
		/// World-related commands: joining, loading, renaming, etc.
		/// </summary>
		World = 32,
		/// <summary>
		/// Zone-related commands: creating, editing, testing, etc.
		/// </summary>
		Zone = 64,
		/// <summary>
		/// Commands that are only used for diagnostics and debugging.
		/// </summary>
		Debug = 128,
		/// <summary>
		/// Commands that are just fun.
		/// </summary>
		Fun = 256,
		/// <summary>
		/// Commands that use advanced mathematics.
		/// </summary>
		Math = 512
	}
}
