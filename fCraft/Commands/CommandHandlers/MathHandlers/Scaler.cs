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

namespace fCraft.Commands.CommandHandlers.MathHandlers {

	// Scales coords according to the defined possibilities.
	public class Scaler {

		// Scalings:
		// ZeroToMaxBound means that every dimension of the selected area is measured from zero to its size in cubes minus one;
		// normalized means that every dimension is measured from zero to one.
		private enum Scaling {
			ZeroToMaxBound,
			Normalized,
			DoubleNormalized,
		}

		private Scaling _Scaling;
		
		public Scaler(string scaling) {
			if(String.IsNullOrWhiteSpace(scaling)) {
				_Scaling = Scaling.ZeroToMaxBound;
			} else if(scaling.ToLower() == "u") {
				_Scaling = Scaling.Normalized;
			} else if(scaling.ToLower() == "uu") {
				_Scaling = Scaling.DoubleNormalized;
			} else {
				throw new ArgumentException("unrecognized scaling " + scaling);
			}
		}

		public double ToFuncParam(double coord, double min, double max) {
			switch(_Scaling) {
				case Scaling.ZeroToMaxBound:
					return coord - min;
				
				case Scaling.Normalized:
					return(coord - min) / Math.Max(1, max - min);
				
				case Scaling.DoubleNormalized:
					return max == min ? 0 : 2.0*(coord - min)/Math.Max(1, max - min) - 1;
				
				default:
					throw new Exception("unknown scaling");
			}
		}

		public int FromFuncResult(double result, double min, double max) {
			switch(_Scaling) {
				case Scaling.ZeroToMaxBound:
					return(int)(result + min);
				
				case Scaling.Normalized:
					return(int)(result * Math.Max(1, max - min) + min);
				
				case Scaling.DoubleNormalized:
					return(int)((result + 1) * Math.Max(1, max - min) / 2.0 + min);
				
				default:
					throw new Exception("unknown scaling");
			}
		}
	}
}
