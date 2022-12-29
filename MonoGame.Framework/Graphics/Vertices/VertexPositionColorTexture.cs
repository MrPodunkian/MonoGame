using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexPositionColorTexture : IVertexType
    {
        public Vector3 Position;
        public Color Color;
        public Color Color1; // Added second color.
        public Vector2 TextureCoordinate;
        public Vector2 TextureCoordinate1; // ARTHUR 5/18/2021: Added a second texture coordinate channel for normalized coordinate. (0, 0 for upper left of quad as submitted by batcher, 1, 1 for lower right)
        public Vector2 TextureCoordinate2;
        public Vector2 TextureCoordinate3;
        public static readonly VertexDeclaration VertexDeclaration;

        public VertexPositionColorTexture(Vector3 position, Color color, Color color1, Vector2 textureCoordinate, Vector2 textureCoordinate1, Vector2 textureCoordinate2, Vector2 textureCoordinate3)
        {
            Position = position;
            Color = color;
            Color1 = color1;
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
                return hashCode;
            }
        }

        public override string ToString()
        {
            return "{{Position:" + this.Position + " Color:" + this.Color + " Color1:" + this.Color1 + " TextureCoordinate:" + this.TextureCoordinate + " TextureCoordinate1:" + this.TextureCoordinate1 + " TextureCoordinate2:" + this.TextureCoordinate2 + " TextureCoordinate3:" + this.TextureCoordinate3 + "}}";
        }

        public static bool operator ==(VertexPositionColorTexture left, VertexPositionColorTexture right)
        {
            return (((((((left.Position == right.Position) && (left.Color == right.Color)) && (left.Color1 == right.Color1)) && (left.TextureCoordinate == right.TextureCoordinate)) && (left.TextureCoordinate1 == right.TextureCoordinate1)) && (left.TextureCoordinate2 == right.TextureCoordinate2)) && (left.TextureCoordinate3 == right.TextureCoordinate3));
        }

        public static bool operator !=(VertexPositionColorTexture left, VertexPositionColorTexture right)
        {
            return !(left == right);
        }

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

            };

            VertexDeclaration = new VertexDeclaration(elements);
        }
    }
}
