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

namespace fCraft.AutoRank {

	/// <summary>
	/// Simple class to hold values for autorank conditions
	/// </summary>
	public class Condition {

		// Values.
		public string StartingRank;
		public string EndingRank;

		public Dictionary<string, Tuple<string, int>> Conditions = new Dictionary<string, Tuple<string, int>>();

		// Constructor.
		public Condition(string start, string end, string cond, string oper, string value) {
			StartingRank = start;
			EndingRank = end;

			Conditions.Add(cond, new Tuple<string, int>(oper, Convert.ToInt32(value)));
		}
	}
}
