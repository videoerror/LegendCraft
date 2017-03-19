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

	// Draws all func variants: any axis as value axis, points, surface, filled.
	public abstract class FuncDrawOperation : DrawOperation {

		public enum ValueAxis {
			Z,
			Y,
			X
		}

		private Expression _Expression;

		private Scaler _Scaler;

		private ValueAxis Vaxis;

		protected int Count;
		
		protected FuncDrawOperation(Player player, Command cmd) : base(player) {
			string strFunc = cmd.Next();

			if(string.IsNullOrWhiteSpace(strFunc)) {
				player.Message("&WEmpty function expression");

				return;
			}

			if(strFunc.Length < 3) {
				player.Message("&WExpression is too short(should be like z=f(x,y))");

				return;
			}
			
			strFunc = strFunc.ToLower();

			Vaxis = GetAxis(SimpleParser.PreparseAssignment(ref strFunc));

			_Expression = SimpleParser.Parse(strFunc, GetVarArray(Vaxis));
			
			Player.Message("Expression parsed as " + _Expression.Print());

			string scalingStr = cmd.Next();

			_Scaler = new Scaler(scalingStr);
		}

		private static string[] GetVarArray(ValueAxis axis) {
			switch(axis) {
				case ValueAxis.Z:
					return new string[] {"x", "y"};
				
				case ValueAxis.Y:
					return new string[] { "x", "z" };
				
				case ValueAxis.X:
					return new string[] { "y", "z" };
			}

			throw new ArgumentException("Unknown value axis direction " +
										axis +
										". This software is not released for use in spaces with dimension higher than three.");
		}

		private static ValueAxis GetAxis(string varName) {
			if(varName.Length == 1) {
				switch(varName[0]) {
					case 'x':
						return ValueAxis.X;
					
					case 'y':
						return ValueAxis.Y;
					
					case 'z':
						return ValueAxis.Z;
				}
			}

			throw new ArgumentException("value axis " +
										varName +
										" is not valid, must be one of 'x', 'y', or 'z'");
		}

		public override int DrawBatch(int maxBlocksToDraw) {
			// Ignoring maxBlocksToDraw.
			switch(Vaxis) {
				case ValueAxis.Z:
					InternalDraw(ref Coords.X, ref Coords.Y, ref Coords.Z,
								 Bounds.XMin, Bounds.XMax, Bounds.YMin, Bounds.YMax, Bounds.ZMin, Bounds.ZMax,
								 maxBlocksToDraw);

					break;
				
				case ValueAxis.Y:
					InternalDraw(ref Coords.X, ref Coords.Z, ref Coords.Y,
								 Bounds.XMin, Bounds.XMax, Bounds.ZMin, Bounds.ZMax, Bounds.YMin, Bounds.YMax,
								 maxBlocksToDraw);

					break;
				
				case ValueAxis.X:
					InternalDraw(ref Coords.Y, ref Coords.Z, ref Coords.X,
								 Bounds.YMin, Bounds.YMax, Bounds.ZMin, Bounds.ZMax, Bounds.XMin, Bounds.XMax,
								 maxBlocksToDraw);

					break;
				
				default:
					throw new ArgumentException("Unknown value axis direction " +
												Vaxis +
												". This software is not released for use in spaces with dimension higher than three.");
			}
			
			IsDone = true;

			return Count;
		}

		// This method exists to box coords nicely as ref params.
		private int InternalDraw(ref int arg1, ref int arg2, ref int val, int min1, int max1, int min2, int max2, int minV, int maxV, int maxBlocksToDraw) {
			int exCount = 0;
			Count = 0;

			DrawFasePrepare(min1, max1, min2, max2);

			for(arg1 = min1; arg1 <= max1 && MathCommands.MaxCalculationExceptions >= exCount; ++arg1) {
				for(arg2 = min2; arg2 <= max2; ++arg2) {
					try {
						int fval =
							_Scaler.FromFuncResult(
								_Expression.Evaluate(_Scaler.ToFuncParam(arg1, min1, max1),
													 _Scaler.ToFuncParam(arg2, min2, max2)),
								minV, maxV);

						DrawFase1(fval, ref arg1, ref arg2, ref val, min1, max1, min2, max2, minV, maxV, maxBlocksToDraw);

						// if(TimeToEndBatch) {
						//	 return _count;
						// }
					} catch(Exception) {
						// The exception here is kinda of normal, for functions(especially interesting ones)
						// may have eg punctured points; we just have to keep an eye on the number, since producing 10000
						// exceptions in the multiclient application is not the best idea.
						if(++exCount > MathCommands.MaxCalculationExceptions) {
							Player.Message("Function drawing is interrupted: too many(>" +
										   MathCommands.MaxCalculationExceptions +
										   ") calculation exceptions.");

							break;
						}
					}
				}
			}

			// The real drawing for the surface variant.
			DrawFase2(ref arg1, ref arg2, ref val, min1, max1, min2, max2, minV, maxV, maxBlocksToDraw);

			return Count;
		}

		protected abstract void DrawFasePrepare(int min1, int max1, int min2, int max2);

		protected abstract void DrawFase1(int fval, ref int arg1, ref int arg2, ref int val, int min1, int max1, int min2, int max2,
										  int minV, int maxV, int maxBlocksToDraw);

		protected abstract void DrawFase2(ref int arg1, ref int arg2, ref int val, int min1, int max1, int min2, int max2,
										  int minV, int maxV, int maxBlocksToDraw);

		public override bool Prepare(Vector3I[] marks) {
			if(!base.Prepare(marks)) {
				return false;
			}

			BlocksTotalEstimate = Bounds.Volume;

			return true;
		}

		public override string Name {
			get {
				return "Func";
			}
		}
	}
}
