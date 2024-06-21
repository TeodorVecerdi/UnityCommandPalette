using System;
using System.Collections.Generic;
using System.Linq;

// Source: https://github.com/microsoft/PowerToys/blob/d66fac3c3c486a750743d7b1f240df001e1e9224/src/modules/launcher/Plugins/Microsoft.PowerToys.Run.Plugin.Calculator/BracketHelper.cs

namespace CommandPalette.Math {
    public static class BracketHelper {
        public static bool IsBracketComplete(string query) {
            if (string.IsNullOrWhiteSpace(query)) {
                return true;
            }

            IEnumerable<(TrailDirection direction, TrailType type)> valueTuples =
                query.Select(BracketTrail).Where(r => r != default);

            Stack<TrailType> trailTest = new Stack<TrailType>();

            foreach ((TrailDirection direction, TrailType type) in valueTuples) {
                switch (direction) {
                    case TrailDirection.Open:
                        trailTest.Push(type);
                        break;
                    case TrailDirection.Close:
                        // Try to get item out of stack
                        if (!trailTest.TryPop(out TrailType popped)) {
                            return false;
                        }

                        if (type != popped) {
                            return false;
                        }

                        continue;
                    default: {
                        throw new ArgumentOutOfRangeException(nameof(direction), direction, "Can't process value");
                    }
                }
            }

            return !trailTest.Any();
        }

        private static (TrailDirection direction, TrailType type) BracketTrail(char @char) {
            switch (@char) {
                case '(':
                    return (TrailDirection.Open, TrailType.Round);
                case ')':
                    return (TrailDirection.Close, TrailType.Round);
                case '[':
                    return (TrailDirection.Open, TrailType.Bracket);
                case ']':
                    return (TrailDirection.Close, TrailType.Bracket);
                default:
                    return default;
            }
        }

        private enum TrailDirection {
            None,
            Open,
            Close,
        }

        private enum TrailType {
            None,
            Bracket,
            Round,
        }
    }
}