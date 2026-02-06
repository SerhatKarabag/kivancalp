using System;

namespace Kivancalp.Gameplay.Configuration
{
    public readonly struct LayoutId : IEquatable<LayoutId>
    {
        public LayoutId(int value)
        {
            Value = value;
        }

        public int Value { get; }

        public bool Equals(LayoutId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is LayoutId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public static bool operator ==(LayoutId left, LayoutId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LayoutId left, LayoutId right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
