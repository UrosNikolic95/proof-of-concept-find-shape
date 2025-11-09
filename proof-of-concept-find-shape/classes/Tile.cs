using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using monogame_cros_platform.enums;

namespace monogame_cros_platform.classes
{
    public class Tile
    {
        public Vector2[] previousPoints = new Vector2[6];
        public Vector2[] _currentPoints = new Vector2[6];
        public Vector2[] currentPoints
        {
            get
            {
                return _currentPoints;
            }
            set
            {
                _currentPoints = value;
                calculateMinAndMax();
            }
        }



        public Vector2 position;
        public Color color = Color.Red;
        public TileMap map;
        public float xMin;
        public float yMin;
        public float xMax;
        public float yMax;


        public Color displayColor()
        {
            return map.mouseHoveringOverGroup != null && map.mouseHoveringOverGroup.Contains(this) ? Color.White : color;
        }
   

        public void calculateMinAndMax()
        {
            xMin = float.PositiveInfinity;
            yMin = float.PositiveInfinity;
            xMax = float.NegativeInfinity;
            yMax = float.NegativeInfinity;
            for (int i = 0; i < currentPoints.Length; i++)
            {
                if (xMin > currentPoints[i].X) xMin = currentPoints[i].X;
                if (yMin > currentPoints[i].Y) yMin = currentPoints[i].Y;
                if (xMax < currentPoints[i].X) xMax = currentPoints[i].X;
                if (yMax < currentPoints[i].Y) yMax = currentPoints[i].Y;
            }
        }

        public bool mouseHovering()
        {
            Point p = Mouse.GetState().Position;
            return xMin < p.X && yMin < p.Y && xMax > p.X && yMax > p.Y;
        }

     

        public Tile getNeighbur(int i)
        {
            Vector2 neighburPosition = position + map.neighburDifference(i);
            return map.tiles.Find(el => Vector2.Distance(el.position, neighburPosition) < 10);
        }

        public void recalculatePoints()
        {
            for (int i = 0; i < currentPoints.Length; i++)
            {
                currentPoints[i] = position + map.tilePrototype[i];
            }
            calculateMinAndMax();
        }

        public void transformPoints()
        {
            for (int i = 0; i < currentPoints.Length; i++)
            {
                previousPoints[i] = currentPoints[i];
                currentPoints[i] = position + map.tilePrototype[i];
            }
            calculateMinAndMax();
        }

        private static VertexPositionColor transformVectorsToVertexPositionColor(Vector2 v3, Color color)
        {
                VertexPositionColor vertex = new VertexPositionColor();
                vertex.Position = new Vector3(v3.X, v3.Y, 0);
                vertex.Color = color;
            return vertex;
        }

        private static VertexPositionColor[] transformVectorsToVertexPositionColor(Vector2[] v3, Color color)
        {
            VertexPositionColor[] vertexPositionColors = new VertexPositionColor[v3.Length];
            for (int i = 0; i < v3.Length; i++) {
                vertexPositionColors[i] = transformVectorsToVertexPositionColor(v3[i], color);
            }
            return vertexPositionColors;
        }

        private Vector2[] midphase()
        {
            Vector2[] res = new Vector2[currentPoints.Length];
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = previousPoints[i] + ((currentPoints[i] - previousPoints[i]) * map.transformationPercentage);
            }
            return res;
        }

   


        private Vector2[] points()
        {
            if (!map.transforming) return currentPoints;
            if (map.transforming) return midphase();
            return null;
        }

        public static void DrawPolygon(GraphicsDevice gd, Vector2[] points, Color color, Vector2 position)
        {
            VertexPositionColor[] vertices = transformVectorsToVertexPositionColor(points, color);
            int triangles = vertices.Length;
            VertexPositionColor[] indices = new VertexPositionColor[triangles * 3];
            VertexPositionColor center = transformVectorsToVertexPositionColor(position,color);
            for (int i = 0; i < triangles - 1; i++)
            {
                int i0 = i * 3;
                indices[i0] = center;
                indices[i0 + 1] = vertices[i];
                indices[i0 + 2] = vertices[i + 1];
            }
            indices[indices.Length - 3] = center;
            indices[indices.Length - 2] = vertices[vertices.Length - 1];
            indices[indices.Length - 1] = vertices[0];
            BasicEffect basicEffect = new BasicEffect(gd)
            {
                VertexColorEnabled = true,
                Projection = Matrix.CreateOrthographicOffCenter(
             0, gd.Viewport.Width,
             gd.Viewport.Height, 0,
             0, 1
         )
            };
            foreach (EffectPass effectPass in basicEffect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                gd.DrawUserPrimitives(PrimitiveType.TriangleList, indices, 0, indices.Length / 3);
            }
        }

        public void Draw()
        {
            DrawPolygon(map.gd, points(), displayColor(), position);
        }
    }
}
