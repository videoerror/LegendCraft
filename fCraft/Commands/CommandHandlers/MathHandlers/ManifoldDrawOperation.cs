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
using System.Data;

using fCraft.Drawing;

namespace fCraft.Commands.CommandHandlers.MathHandlers {

	public class ManifoldDrawOperation : DrawOperation {

		private Scaler _Scaler;

		private Expression[] _Expressions;

		private double[][] ParamIterations;

		private const double MaxIterationSteps = 1000000;

		public ManifoldDrawOperation(Player p, Command cmd) : base(p) {
			_Expressions = PrepareParametrizedManifold.GetPlayerParametrizationCoordsStorage(p);

			if(_Expressions[0] == null) {
				throw new InvalidExpressionException("x is undefined");
			}

			if(_Expressions[1] == null) {
				throw new InvalidExpressionException("y is undefined");
			}

			if(_Expressions[2] == null) {
				throw new InvalidExpressionException("z is undefined");
			}

			ParamIterations = PrepareParametrizedManifold.GetPlayerParametrizationParamsStorage(p);

			if(ParamIterations[0] == null && ParamIterations[1] == null && ParamIterations[2] == null) {
				throw new InvalidExpressionException("all parametrization variables are undefined");
			}

			if(GetNumOfSteps(0) * GetNumOfSteps(1) * GetNumOfSteps(2) > MaxIterationSteps) {
				throw new InvalidExpressionException("too many iteration steps(over " + MaxIterationSteps + ")");
			}

			_Scaler = new Scaler(cmd.Next());

			p.Message("Going to draw the following parametrization:\nx=" +
					  _Expressions[0].Print() +
					  "\ny=" +
					  _Expressions[1].Print() +
					  "\nz=" +
					  _Expressions[2].Print());
		}

		public override string Name {
			get {
				return "ParametrizedManifold";
			}
		}

		public override int DrawBatch(int maxBlocksToDraw) {
			int count = 0;

			double fromT, toT, stepT;
			double fromU, toU, stepU;
			double fromV, toV, stepV;

			GetIterationBounds(0, out fromT, out toT, out stepT);
			GetIterationBounds(1, out fromU, out toU, out stepU);
			GetIterationBounds(2, out fromV, out toV, out stepV);

			for(double t = fromT; t <= toT; t += stepT) {
				for(double u = fromU; u <= toU; u += stepU) {
					for(double v = fromV; v <= toV; v += stepV) {
						Coords.X = _Scaler.FromFuncResult(_Expressions[0].Evaluate(t, u, v), Bounds.XMin, Bounds.XMax);
						Coords.Y = _Scaler.FromFuncResult(_Expressions[1].Evaluate(t, u, v), Bounds.YMin, Bounds.YMax);
						Coords.Z = _Scaler.FromFuncResult(_Expressions[2].Evaluate(t, u, v), Bounds.ZMin, Bounds.ZMax);

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

		private double GetNumOfSteps(int idx) {
			if(ParamIterations[idx] == null) {
				return 1;
			}

			return (ParamIterations[idx][1] - ParamIterations[idx][0]) / ParamIterations[idx][2] + 1;
		}

		private void GetIterationBounds(int idx, out double from, out double to, out double step) {
			if(ParamIterations[idx] == null) {
				from = 0;
				to = 0;
				step = 1;

				return;
			}

			from = ParamIterations[idx][0];
			to = ParamIterations[idx][1];
			step = ParamIterations[idx][2];
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
