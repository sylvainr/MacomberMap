using System;
using System.Runtime.InteropServices;
using SharpDX;

namespace MacomberMapClient.User_Interfaces.NetworkMap.DX
{
    /// <summary>
    /// Vertex containing position, color and normal.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 16 + 16 + 12)]
    public struct VertexPositionColorNormal : IEquatable<VertexPositionColorNormal>
    {
        /// <summary>
        /// Initializes a new <see cref="VertexPositionNormalTexture"/> instance.
        /// </summary>
        /// <param name="position">The position of this vertex.</param>
        /// <param name="color">Vertex color.</param>
        /// <param name="normal">The vertex normal.</param>
        public VertexPositionColorNormal(Vector4 position, Vector4 color, Vector3 normal) : this()
        {
            Position = position;
            Color = color;
            Normal = normal;
        }

        public VertexPositionColorNormal(Vector4 position, Vector4 color) : this()
        {
            Position = position;
            Color = color;
            Normal = Vector3.Zero;
        }
        //[FieldOffset(0)]
        public Vector4 Position;

        //[FieldOffset(16)]
        public Vector4 Color;

        // [FieldOffset(32)]
        public Vector3 Normal;

        #region Equality members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(VertexPositionColorNormal other)
        {
            return Position.Equals(other.Position) && Color.Equals(other.Color);// && Normal.Equals(other.Normal);
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false. 
        /// </returns>
        /// <param name="obj">The object to compare with the current instance. </param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is VertexPositionColorNormal && Equals((VertexPositionColorNormal)obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Position.GetHashCode();
                hashCode = (hashCode * 397) ^ Color.GetHashCode();
                hashCode = (hashCode * 397) ^ Normal.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(VertexPositionColorNormal left, VertexPositionColorNormal right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(VertexPositionColorNormal left, VertexPositionColorNormal right)
        {
            return !left.Equals(right);
        }

        #endregion
    }
}