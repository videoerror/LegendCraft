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

using fCraft;

namespace fCraft.Commands.CommandHandlers.MathHandlers {

	public class FuncDrawOperationPoints : FuncDrawOperation {

		public FuncDrawOperationPoints(Player player, Command cmd) : base(player, cmd) {
		}

		protected override void DrawFasePrepare(int min1, int max1, int min2, int max2) {
		}

		protected override void DrawFase1(int fval, ref int arg1, ref int arg2, ref int val, int min1, int max1, int min2, int max2,
										  int minV, int maxV, int maxBlocksToDraw) {
			if(fval <= maxV && fval >= minV) {
				val = fval;

				if(DrawOneBlock()) {
					++Count;
				}
			}
		}

		protected override void DrawFase2(ref int arg1, ref int arg2, ref int val, int min1, int max1, int min2, int max2,
										  int minV, int maxV, int maxBlocksToDraw) {
		}

		public override string Name {
			get {
				return base.Name + "Points";
			}
		}
	}

	public class FuncDrawOperationFill : FuncDrawOperation {

		public FuncDrawOperationFill(Player player, Command cmd) : base(player, cmd) {
		}

		protected override void DrawFasePrepare(int min1, int max1, int min2, int max2) {
		}

		protected override void DrawFase1(int fval, ref int arg1, ref int arg2, ref int val, int min1, int max1, int min2, int max2,
										  int minV, int maxV, int maxBlocksToDraw) {
			for(val = minV; val <= fval && val <= maxV; ++val) {
				if(DrawOneBlock()) {
					++Count;

					// if(TimeToEndBatch) {
					//	 return;
					// }
				}
			}
		}

		protected override void DrawFase2(ref int arg1, ref int arg2, ref int val, int min1, int max1, int min2, int max2,
										  int minV, int maxV, int maxBlocksToDraw) {
		}

		public override string Name {
			get {
				return base.Name + "Fill";
			}
		}
	}

	public class FuncDrawOperationSurface : FuncDrawOperation {

		private int[][] Surface;

		public FuncDrawOperationSurface(Player player, Command cmd) : base(player, cmd) {
		}

		protected override void DrawFasePrepare(int min1, int max1, int min2, int max2) {
			Surface = new int[max1 - min1 + 1][];

			for(int i = 0; i < Surface.Length; ++i) {
				Surface[i] = new int[max2 - min2 + 1];

				for(int j = 1; j < Surface[i].Length; ++j) {
					Surface[i][j] = int.MaxValue;
				}
			}
		}

		protected override void DrawFase1(int fval, ref int arg1, ref int arg2, ref int val, int min1, int max1, int min2, int max2,
										  int minV, int maxV, int maxBlocksToDraw) {
			Surface[arg1 - min1][arg2 - min2] = fval;
		}

		protected override void DrawFase2(ref int arg1, ref int arg2, ref int val, int min1, int max1, int min2, int max2,
										  int minV, int maxV, int maxBlocksToDraw) {
			int count = 0;

			for(arg1 = min1; arg1 <= max1; ++arg1) {
				for(arg2 = min2; arg2 <= max2; ++arg2) {
					int a1 = arg1 - min1, a2 = arg2 - min2;

					if(Surface[a1][a2] == int.MaxValue) {
						continue;
					}

					// Find min value around.
					int minVal = Surface[a1][a2];

					if(a1 - 1 >= 0) {
						minVal = Math.Min(minVal, Surface[a1 - 1][a2] + 1);
					}

					if(a1 + 1 < Surface.Length) {
						minVal = Math.Min(minVal, Surface[a1 + 1][a2] + 1);
					}

					if(a2 - 1 >= 0) {
						minVal = Math.Min(minVal, Surface[a1][a2 - 1] + 1);
					}

					if(a2 + 1 < Surface[a1].Length) {
						minVal = Math.Min(minVal, Surface[a1][a2 + 1] + 1);
					}

					minVal = Math.Max(minVal, minV);

					for(val = minVal; val <= Surface[a1][a2] && val <= maxV; ++val) {
						if(DrawOneBlock()) {
							++count;

							// if(TimeToEndBatch) {
							//	 return;
							// }
						}
					}
				}
			}
		}

		public override string Name {
			get {
				return base.Name + "Surface";
			}
		}
	}
}
