using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Describes a custom vertex format structure that contains position,
    /// color, and one set of texture coordinates.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexPositionColorTexture : IVertexType
    {
        /// <inheritdoc cref="VertexPosition.Position"/>
        public Vector3 Position;
        /// <inheritdoc cref="VertexPositionColor.Color"/>
        public Color Color;
        public Color Color1;

        /// <inheritdoc cref="VertexPositionTexture.TextureCoordinate"/>
		public Vector2 TextureCoordinate;
        public Vector2 TextureCoordinate1; // ARTHUR 5/18/2021: Added a second texture coordinate channel for normalized coordinate. (0, 0 for upper left of quad as submitted by batcher, 1, 1 for lower right)
        public Vector2 TextureCoordinate2;
        public Vector2 TextureCoordinate3;
        public Vector2 Color2RG;
        public Vector2 Color2BA;
        /// <inheritdoc cref="IVertexType.VertexDeclaration"/>
        public static readonly VertexDeclaration VertexDeclaration;


        /// <summary>
        /// Creates an instance of <see cref="VertexPositionColorTexture"/>.
        /// </summary>
        /// <param name="position">Position of the vertex.</param>
        /// <param name="color">Color of the vertex.</param>
        /// <param name="textureCoordinate">Texture coordinate of the vertex.</param>
		public VertexPositionColorTexture(Vector3 position, Color color, Color color1, Color color2, Vector2 textureCoordinate, Vector2 textureCoordinate1, Vector2 textureCoordinate2, Vector2 textureCoordinate3)
        {
            Position = position;
            Color = color;
            Color2RG = new Vector2(color2.R / 255.0F, color2.G / 255.0F);
            Color2BA = new Vector2(color2.B / 255.0F, color2.A / 255.0F);
            TextureCoordinate = textureCoordinate;
            TextureCoordinate1 = textureCoordinate1;
            TextureCoordinate2 = textureCoordinate2;
            TextureCoordinate3 = textureCoordinate3;
        }
		
        VertexDeclaration IVertexType.VertexDeclaration
        {
            get
            {
                return VertexDeclaration;
            }
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Position.GetHashCode();
                hashCode = (hashCode * 397) ^ Color.GetHashCode();
                hashCode = (hashCode * 397) ^ Color1.GetHashCode();
                hashCode = (hashCode * 397) ^ TextureCoordinate.GetHashCode();
                hashCode = (hashCode * 397) ^ TextureCoordinate1.GetHashCode();
                hashCode = (hashCode * 397) ^ TextureCoordinate2.GetHashCode();
                hashCode = (hashCode * 397) ^ TextureCoordinate3.GetHashCode();
                hashCode = (hashCode * 397) ^ Color2RG.GetHashCode();
                hashCode = (hashCode * 397) ^ Color2BA.GetHashCode();
                return hashCode;
            }
        }

        /// <inheritdoc cref="VertexPosition.ToString()"/>
        public override string ToString()
        {
            return "{{Position:" + this.Position + " Color:" + this.Color + " Color1:" + this.Color1 + "Color2: " + this.Color2RG + this.Color2BA + " TextureCoordinate:" + this.TextureCoordinate + " TextureCoordinate1:" + this.TextureCoordinate1 + " TextureCoordinate2:" + this.TextureCoordinate2 + " TextureCoordinate3:" + this.TextureCoordinate3 + "}}";
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="VertexPositionColorTexture"/> are equal
        /// </summary>
        /// <param name="left">The object on the left of the equality operator.</param>
        /// <param name="right">The object on the right of the equality operator.</param>
        /// <returns>
        /// <see langword="true"/> if the objects are the same; <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator ==(VertexPositionColorTexture left, VertexPositionColorTexture right)
        {
            return (((((((((left.Position == right.Position) && (left.Color == right.Color)) && (left.Color1 == right.Color1)) && (left.Color2RG == right.Color2RG)) && (left.Color2BA == right.Color2BA)) && (left.TextureCoordinate == right.TextureCoordinate)) && (left.TextureCoordinate1 == right.TextureCoordinate1)) && (left.TextureCoordinate2 == right.TextureCoordinate2)) && (left.TextureCoordinate3 == right.TextureCoordinate3));
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="VertexPositionColorTexture"/> are different
        /// </summary>
        /// <param name="left">The object on the left of the inequality operator.</param>
        /// <param name="right">The object on the right of the inequality operator.</param>
        /// <returns>
        /// <see langword="true"/> if the objects are different; <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator !=(VertexPositionColorTexture left, VertexPositionColorTexture right)
        {
            return !(left == right);
        }

        /// <inheritdoc cref="VertexPosition.Equals(object)"/>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != base.GetType())
                return false;

            return (this == ((VertexPositionColorTexture)obj));
        }

        static VertexPositionColorTexture()
        {
            var elements = new VertexElement[]
            {
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new VertexElement(16, VertexElementFormat.Color, VertexElementUsage.Color, 1),
                new VertexElement(20, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                new VertexElement(28, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
                new VertexElement(36, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 2),
                new VertexElement(44, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 3),
                new VertexElement(52, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 4),
                new VertexElement(60, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 5),

            };

            VertexDeclaration = new VertexDeclaration(elements);
        }
    }
}
