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

// LegendCraft Edit: Copy and Pasted Handler From 800craft modified for /spring and later similar cmds to follow. This handler was made by 800craft so proper credit shall be given.
// This handler was made to use in those cmds specifically so that no messages would be displayed to the user when doing them.

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
using System.Data;

using fCraft.Drawing;

namespace fCraft.Commands.CommandHandlers.MathHandlers {

	public class StartSpringDraw : DrawOperation {

		private Scaler _Scaler;

		private Expression[] Expressions;

		private double[][] ParameterIterations;

		private const double MaxIterationSteps = 1000000;

		public StartSpringDraw(Player p, Command cmd) : base(p) {
			Expressions = PrepareParametrizedManifold.GetPlayerParametrizationCoordsStorage(p);

			if(Expressions[0] == null) {
				throw new InvalidExpressionException("x is undefined");
			}

			if(Expressions[1] == null) {
				throw new InvalidExpressionException("y is undefined");
			}

			if(Expressions[2] == null) {
				throw new InvalidExpressionException("z is undefined");
			}

			ParameterIterations = PrepareParametrizedManifold.GetPlayerParametrizationParamsStorage(p);

			if(ParameterIterations[0] == null && ParameterIterations[1] == null && ParameterIterations[2] == null) {
				throw new InvalidExpressionException("all parametrization variables are undefined");
			}

			if(GetNumOfSteps2(0) * GetNumOfSteps2(1) * GetNumOfSteps2(2) > MaxIterationSteps) {
				throw new InvalidExpressionException("too many iteration steps(over " + MaxIterationSteps + ")");
			}

			_Scaler = new Scaler(cmd.Next());

		}

		public override string Name {
			// What it will say when doing one of these math figure cmds.
			get {
				return "MathFigure";
			}
		}

		public override int DrawBatch(int maxBlocksToDraw) {
			int count = 0;

			double fromT, toT, stepT;
			double fromU, toU, stepU;
			double fromV, toV, stepV;

			GetIterationBounds2(0, out fromT, out toT, out stepT);
			GetIterationBounds2(1, out fromU, out toU, out stepU);
			GetIterationBounds2(2, out fromV, out toV, out stepV);

			for(double t = fromT; t <= toT; t += stepT) {
				for(double u = fromU; u <= toU; u += stepU) {
					for(double v = fromV; v <= toV; v += stepV) {
						Coords.X = _Scaler.FromFuncResult(Expressions[0].Evaluate(t, u, v), Bounds.XMin, Bounds.XMax);
						Coords.Y = _Scaler.FromFuncResult(Expressions[1].Evaluate(t, u, v), Bounds.YMin, Bounds.YMax);
						Coords.Z = _Scaler.FromFuncResult(Expressions[2].Evaluate(t, u, v), Bounds.ZMin, Bounds.ZMax);

						if(DrawOneBlock()) {
							++count;
						}

						// if(TimeToEndBatch) {
						//	 return count;
						// }
					}
				}
			}

			IsDone = true;

			return count;
		}

		private double GetNumOfSteps2(int idx) {
			if(ParameterIterations[idx] == null) {
				return 1;
			}

			return(ParameterIterations[idx][1] - ParameterIterations[idx][0]) / ParameterIterations[idx][2] + 1;
		}

		private void GetIterationBounds2(int idx, out double from, out double to, out double step) {
			if(ParameterIterations[idx] == null) {
				from = 0;
				to = 0;
				step = 1;

				return;
			}

			from = ParameterIterations[idx][0];
			to = ParameterIterations[idx][1];
			step = ParameterIterations[idx][2];
		}

		public override bool Prepare(Vector3I[] marks) {
			if(!base.Prepare(marks)) {
				return false;
			}

			BlocksTotalEstimate = Bounds.Volume;

			return true;
		}
	}
}
