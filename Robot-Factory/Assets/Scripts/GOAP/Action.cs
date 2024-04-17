using System;
using System.Collections.Generic;
using UnityEngine;

namespace GOAP
{
    public class Action
    {
        public string Name { get; } = "";
        public Robot Robot { get; } = null;
        public Dictionary<Goal, float> FloatPreconditions { get; set; } = new Dictionary<Goal, float>();
        public Dictionary<Goal, Tuple<float, float>> FloatRangePreconditions { get; set; } = new Dictionary<Goal, Tuple<float, float>>();
        public Dictionary<Goal, bool> BoolPreconditions { get; set; } = new Dictionary<Goal, bool>();
        public Dictionary<Goal, Vector2> PositionPreconditions { get; set; } = new Dictionary<Goal, Vector2>();
        public Dictionary<Goal, float> FloatEffects { get; set; } = new Dictionary<Goal, float>();
        public Dictionary<Goal, bool> BoolEffects { get; set; } = new Dictionary<Goal, bool>();
        public Dictionary<Goal, Vector2> PositionEffects { get; set; } = new Dictionary<Goal, Vector2>();

        public bool PositionPreconditionsUseOr = false;

        public Action(string name, Robot robot)
        {
            Name = name;
            Robot = robot;
        }

        public bool PreconditionsSatisfied(WorldState state)
        {
            foreach (Goal goal in FloatPreconditions.Keys)
                if (state.FloatGoals[goal] < FloatPreconditions[goal]) return false;

            foreach (Goal goal in FloatRangePreconditions.Keys)
                if (state.FloatGoals[goal] < FloatRangePreconditions[goal].Item1 || state.FloatGoals[goal] >= FloatRangePreconditions[goal].Item2) return false;

            foreach (Goal goal in BoolPreconditions.Keys)
                if (state.BoolGoals[goal] != BoolPreconditions[goal]) return false;

            foreach (Goal goal in PositionPreconditions.Keys)
                if (Vector2.Distance(state.PositionGoals[goal], PositionPreconditions[goal]) > 1.0f) return false;

            return true;
        }

        public WorldState GetSuccessor(WorldState state)
        {
            WorldState successorState = state.Clone();

            foreach (Goal goal in FloatEffects.Keys)
                successorState.FloatGoals[goal] += FloatEffects[goal];

            foreach (Goal goal in BoolEffects.Keys)
                successorState.BoolGoals[goal] = BoolEffects[goal];

            foreach (Goal goal in PositionEffects.Keys)
                successorState.PositionGoals[goal] = PositionEffects[goal];

            return successorState;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
