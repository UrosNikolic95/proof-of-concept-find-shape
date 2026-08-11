using System;
using System.Collections;
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
using proof_of_concept_find_shape.classes;

namespace monogame_cros_platform.classes
{
    public class Tile
    {

        public TilePoints previousPoints;
        public TilePoints currentPoints;
        public TilePoints nextPoints;

        public Vector2 position;
        public Color color = Color.Red;
        public TileMap map;
        public MinMax minMax;

        public TilePoints lastPoints
        {
            get {
                return nextPoints != null ? nextPoints : currentPoints;
            }
        }

        public Color displayColor()
        {
            return map.mouseHoveringOverGroup != null && map.mouseHoveringOverGroup.Contains(this) ? Color.White : color;
        }

        public void calculateMinAndMax()
        {
            minMax = new MinMax(currentPoints);
        }

        public bool mouseHovering()
        {
            Point p = Mouse.GetState().Position;
            return minMax.xMin < p.X && minMax.yMin < p.Y && minMax.xMax > p.X && minMax.yMax > p.Y;
        }

     

        public Tile getNeighbur(int i)
        {
            Vector2 neighburPosition = position + map.neighburDifference(i);
            return map.tiles.Find(el => Vector2.Distance(el.position, neighburPosition) < 10);
        }

        public void recalculatePoints()
        {
            currentPoints = new TilePoints(position, map.tilePrototype);
            calculateMinAndMax();
        }

        public void transformPoints()
        {
            previousPoints = currentPoints;
            currentPoints = new TilePoints(position, map.tilePrototype);
            calculateMinAndMax();
        }

        public void transformNextPoints()
        {
            nextPoints = new TilePoints(position, map.tilePrototype);
            calculateMinAndMax();
        }

        public void shiftPoints()
        {
            previousPoints = currentPoints;
            currentPoints = nextPoints;
            nextPoints = null;
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

        private TilePoints midphase()
        {
            return TilePoints.midphase(previousPoints,currentPoints,map.transformationPercentage);
        }

   


        private TilePoints points()
        {
            if (map.transforming == 0) return currentPoints;
            if (map.transforming != 0) return midphase();
            return null;
        }


        private static readonly RasterizerState rasterizerState = new RasterizerState
        {
            MultiSampleAntiAlias = true,
            CullMode = CullMode.None
        };

        private static BasicEffect basicEffect;

        public static void DrawPolygon(GraphicsDevice gd, TilePoints points, Color color)
        {
            VertexPositionColor[] vertices = transformVectorsToVertexPositionColor(points.points, color);
            int triangles = vertices.Length;
            VertexPositionColor[] indices = new VertexPositionColor[triangles * 3];
            VertexPositionColor center = transformVectorsToVertexPositionColor(points.position, color);
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
            if (basicEffect == null || basicEffect.GraphicsDevice != gd)
            {
                basicEffect = new BasicEffect(gd) { VertexColorEnabled = true };
            }
            basicEffect.Projection = Matrix.CreateOrthographicOffCenter(
                0,
                gd.Viewport.Width,
                gd.Viewport.Height,
                0,
                0,
                1
            );
            gd.RasterizerState = rasterizerState;
            foreach (EffectPass effectPass in basicEffect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                gd.DrawUserPrimitives(PrimitiveType.TriangleList, indices, 0, indices.Length / 3);
            }
        }
        public void Draw()
        {
            DrawPolygon(map.gd, points(), displayColor());
        }
    }
}
