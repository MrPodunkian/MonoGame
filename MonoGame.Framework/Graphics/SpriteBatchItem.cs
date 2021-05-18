// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    internal class SpriteBatchItem : IComparable<SpriteBatchItem>
	{
		public Texture2D Texture;
        public float SortKey;

        public VertexPositionColorTexture vertexTL;
		public VertexPositionColorTexture vertexTR;
		public VertexPositionColorTexture vertexBL;
		public VertexPositionColorTexture vertexBR;
		public SpriteBatchItem ()
		{
			vertexTL = new VertexPositionColorTexture();
            vertexTR = new VertexPositionColorTexture();
            vertexBL = new VertexPositionColorTexture();
            vertexBR = new VertexPositionColorTexture();            
		}
		
		public void Set ( float x, float y, float dx, float dy, float w, float h, float sin, float cos, Color color, Vector2 texCoordTL, Vector2 texCoordBR, float depth, Matrix? transformMatrix)
		{
            // TODO, Should we be just assigning the Depth Value to Z?
            // According to http://blogs.msdn.com/b/shawnhar/archive/2011/01/12/spritebatch-billboards-in-a-3d-world.aspx
            // We do.
			vertexTL.Position.X = x+dx*cos-dy*sin;
            vertexTL.Position.Y = y+dx*sin+dy*cos;
            vertexTL.Position.Z = depth;
            vertexTL.Color = color;
            vertexTL.TextureCoordinate.X = texCoordTL.X;
            vertexTL.TextureCoordinate.Y = texCoordTL.Y;

			vertexTR.Position.X = x+(dx+w)*cos-dy*sin;
            vertexTR.Position.Y = y+(dx+w)*sin+dy*cos;
            vertexTR.Position.Z = depth;
            vertexTR.Color = color;
            vertexTR.TextureCoordinate.X = texCoordBR.X;
            vertexTR.TextureCoordinate.Y = texCoordTL.Y;

			vertexBL.Position.X = x+dx*cos-(dy+h)*sin;
            vertexBL.Position.Y = y+dx*sin+(dy+h)*cos;
            vertexBL.Position.Z = depth;
            vertexBL.Color = color;
            vertexBL.TextureCoordinate.X = texCoordTL.X;
            vertexBL.TextureCoordinate.Y = texCoordBR.Y;

			vertexBR.Position.X = x+(dx+w)*cos-(dy+h)*sin;
            vertexBR.Position.Y = y+(dx+w)*sin+(dy+h)*cos;
            vertexBR.Position.Z = depth;
            vertexBR.Color = color;
            vertexBR.TextureCoordinate.X = texCoordBR.X;
            vertexBR.TextureCoordinate.Y = texCoordBR.Y;

            // ARTHUR 5/18/2021: Added normalized quad coordinates.
            vertexTL.TextureCoordinate1.X = 0;
            vertexTL.TextureCoordinate1.Y = 0;
            vertexTR.TextureCoordinate1.X = 1;
            vertexTR.TextureCoordinate1.Y = 0;
            vertexBL.TextureCoordinate1.X = 0;
            vertexBL.TextureCoordinate1.Y = 1;
            vertexBR.TextureCoordinate1.X = 1;
            vertexBR.TextureCoordinate1.Y = 1;

            // HACK: Allow passing in a transform matrix.
            if (transformMatrix != null)
            {
                vertexTL.Position = Vector3.Transform(vertexTL.Position, transformMatrix.Value);
                vertexTR.Position = Vector3.Transform(vertexTR.Position, transformMatrix.Value);
                vertexBL.Position = Vector3.Transform(vertexBL.Position, transformMatrix.Value);
                vertexBR.Position = Vector3.Transform(vertexBR.Position, transformMatrix.Value);
            }
		}

        public void Set(float x, float y, float w, float h, Color color, Vector2 texCoordTL, Vector2 texCoordBR, float depth, Matrix? transformMatrix)
        {
            vertexTL.Position.X = x;
            vertexTL.Position.Y = y;
            vertexTL.Position.Z = depth;
            vertexTL.Color = color;
            vertexTL.TextureCoordinate.X = texCoordTL.X;
            vertexTL.TextureCoordinate.Y = texCoordTL.Y;

            vertexTR.Position.X = x + w;
            vertexTR.Position.Y = y;
            vertexTR.Position.Z = depth;
            vertexTR.Color = color;
            vertexTR.TextureCoordinate.X = texCoordBR.X;
            vertexTR.TextureCoordinate.Y = texCoordTL.Y;

            vertexBL.Position.X = x;
            vertexBL.Position.Y = y + h;
            vertexBL.Position.Z = depth;
            vertexBL.Color = color;
            vertexBL.TextureCoordinate.X = texCoordTL.X;
            vertexBL.TextureCoordinate.Y = texCoordBR.Y;

            vertexBR.Position.X = x + w;
            vertexBR.Position.Y = y + h;
            vertexBR.Position.Z = depth;
            vertexBR.Color = color;
            vertexBR.TextureCoordinate.X = texCoordBR.X;
            vertexBR.TextureCoordinate.Y = texCoordBR.Y;

            // ARTHUR 5/18/2021: Added normalized quad coordinates.
            vertexTL.TextureCoordinate1.X = 0;
            vertexTL.TextureCoordinate1.Y = 0;
            vertexTR.TextureCoordinate1.X = 1;
            vertexTR.TextureCoordinate1.Y = 0;
            vertexBL.TextureCoordinate1.X = 0;
            vertexBL.TextureCoordinate1.Y = 1;
            vertexBR.TextureCoordinate1.X = 1;
            vertexBR.TextureCoordinate1.Y = 1;

            // HACK: Allow passing in a transform matrix.
            if (transformMatrix != null)
            {
                vertexTL.Position = Vector3.Transform(vertexTL.Position, transformMatrix.Value);
                vertexTR.Position = Vector3.Transform(vertexTR.Position, transformMatrix.Value);
                vertexBL.Position = Vector3.Transform(vertexBL.Position, transformMatrix.Value);
                vertexBR.Position = Vector3.Transform(vertexBR.Position, transformMatrix.Value);
            }
        }

        #region Implement IComparable
        public int CompareTo(SpriteBatchItem other)
        {
            return SortKey.CompareTo(other.SortKey);
        }
        #endregion
    }
}

