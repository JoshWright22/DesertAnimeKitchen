using UnityEngine;

namespace DessertFactory
{
    public enum Direction
    {
        Up,
        Right,
        Down,
        Left
    }

    public static class DirectionExtensions
    {
        public static Vector2Int ToOffset(this Direction dir)
        {
            switch (dir)
            {
                case Direction.Up: return Vector2Int.up;
                case Direction.Right: return Vector2Int.right;
                case Direction.Down: return Vector2Int.down;
                default: return Vector2Int.left;
            }
        }

        public static Direction RotateClockwise(this Direction dir)
        {
            return (Direction)(((int)dir + 1) % 4);
        }

        public static float ToAngle(this Direction dir)
        {
            return -90f * (int)dir;
        }
    }
}
