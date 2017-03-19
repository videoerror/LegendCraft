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

using fCraft.Drawing;

namespace fCraft.Commands.CommandHandlers.MathHandlers {

	// Draws volume, defined by an inequality.
	public class InequalityDrawOperation : DrawOperation {

		private Expression _Expression;

		private Scaler _Scaler;

		private int Count;

		public InequalityDrawOperation(Player player, Command cmd) : base(player) {
			string strFunc = cmd.Next();

			if(String.IsNullOrWhiteSpace(strFunc)) {
				throw new ArgumentException("empty inequality expression");
			}

			if(strFunc.Length < 3) {
				throw new ArgumentException("expression is too short(should be like f(x,y,z)>g(x,y,z))");
			}
			
			strFunc = strFunc.ToLower();

			_Expression = SimpleParser.Parse(strFunc, new string[] {
				"x", "y", "z"
			});

			if(!_Expression.IsInEquality()) {
				throw new ArgumentException("the expression given is not an inequality(should be like f(x,y,z)>g(x,y,z))");
			}

			Player.Message("Expression parsed as " + _Expression.Print());

			string scalingStr = cmd.Next();

			_Scaler = new Scaler(scalingStr);
		}

		public override int DrawBatch(int maxBlocksToDraw) {
			// Ignoring maxBlocksToDraw.
			int exCount = 0;
			Count = 0;

			for(Coords.X = Bounds.XMin; Coords.X <= Bounds.XMax && MathCommands.MaxCalculationExceptions >= exCount; ++Coords.X) {
				for(Coords.Y = Bounds.YMin; Coords.Y <= Bounds.YMax && MathCommands.MaxCalculationExceptions >= exCount; ++Coords.Y) {
					for(Coords.Z = Bounds.ZMin; Coords.Z <= Bounds.ZMax; ++Coords.Z) {
						try {
							// 1.0 means true.
							if(_Expression.Evaluate(_Scaler.ToFuncParam(Coords.X, Bounds.XMin, Bounds.XMax),
													_Scaler.ToFuncParam(Coords.Y, Bounds.YMin, Bounds.YMax),
													_Scaler.ToFuncParam(Coords.Z, Bounds.ZMin, Bounds.ZMax)) > 0) {
								if(DrawOneBlock()) {
									++Count;
								}
							}

							// if(TimeToEndBatch) {
							//	 return _count;
							// }
						} catch(Exception) {
							// The exception here is kinda of normal, for functions(especially interesting ones)
							// may have eg punctured points; we just have to keep an eye on the number, since producing 10000
							// exceptions in the multiclient application is not the best idea.
							if(++exCount > MathCommands.MaxCalculationExceptions) {
								Player.Message("Drawing is interrupted: too many(>" +
											   MathCommands.MaxCalculationExceptions +
											   ") calculation exceptions.");

								break;
							}
						}
					}
				}
			}

			IsDone = true;

			return Count;
		}

		public override bool Prepare(Vector3I[] marks) {
			if(!base.Prepare(marks)) {
				return false;
			}

			BlocksTotalEstimate = Bounds.Volume;

			return true;
		}

		public override string Name {
			get {
				return "Inequality";
			}
		}
	}
}
