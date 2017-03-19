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
using System.Linq;
using System.Xml.Linq;

using JetBrains.Annotations;

// Legacy autorank support for fCraft.
namespace fCraft.AutoRank {

	public sealed class fCraftCriterion : ICloneable {

		public Rank FromRank {
			get;

			set;
		}

		public Rank ToRank {
			get;

			set;
		}

		public ConditionSet Condition {
			get;

			set;
		}

		public fCraftCriterion() {
		}

		public fCraftCriterion([NotNull] fCraftCriterion other) {
			if(other == null) {
				throw new ArgumentNullException("other");
			}

			FromRank = other.FromRank;
			ToRank = other.ToRank;

			Condition = other.Condition;
		}

		public fCraftCriterion([NotNull] Rank fromRank, [NotNull] Rank toRank, [NotNull] ConditionSet condition) {
			if(fromRank == null) {
				throw new ArgumentNullException("fromRank");
			}

			if(toRank == null) {
				throw new ArgumentNullException("toRank");
			}

			if(condition == null) {
				throw new ArgumentNullException("condition");
			}

			FromRank = fromRank;
			ToRank = toRank;

			Condition = condition;
		}

		public fCraftCriterion([NotNull] XElement el) {
			if(el == null) {
				throw new ArgumentNullException("el");
			}

			// ReSharper disable PossibleNullReferenceException.
			FromRank = Rank.Parse(el.Attribute("fromRank").Value);
			// ReSharper restore PossibleNullReferenceException.

			if(FromRank == null) {
				throw new FormatException("Could not parse \"fromRank\"");
			}

			// ReSharper disable PossibleNullReferenceException.
			ToRank = Rank.Parse(el.Attribute("toRank").Value);
			// ReSharper restore PossibleNullReferenceException.

			if(ToRank == null) {
				throw new FormatException("Could not parse \"toRank\"");
			}

			Condition = (ConditionSet)AutoRank.fCraftConditions.Parse(el.Elements().First());
		}

		public object Clone() {
			return new fCraftCriterion(this);
		}

		public override string ToString() {
			return String.Format("Criteria({0} from {1} to {2})",
								 (FromRank < ToRank ? "promote" : "demote"),
								 FromRank.Name,
								 ToRank.Name);
		}

		public XElement Serialize() {
			XElement el = new XElement("Criterion");

			el.Add(new XAttribute("fromRank", FromRank.FullName));
			el.Add(new XAttribute("toRank", ToRank.FullName));

			if(Condition != null) {
				el.Add(Condition.Serialize());
			}

			return el;
		}
	}
}
