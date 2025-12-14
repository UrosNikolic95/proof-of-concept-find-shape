using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using monogame_cros_platform.enums;

namespace monogame_cros_platform.classes
{
    public class TileMap
    {
        public Color[] colorOptions = new Color[] { Color.Red, Color.Blue, Color.Yellow };
        public List<Tile> tiles = new List<Tile>();
        public GraphicsDevice gd;
        public float padding = 1;
        public float tileRadius = 20;
        public TileTypeEnum type = TileTypeEnum.Hexagon;
        public float mapWidening = 100;

        public bool transforming = false;
        public double transformTime = 0;
        public double transformationLength = 0.25;

        public double hexagonPointAngleDifference = 60;
        public double hexagonStartingPointAngle = 30;

        public double squarePointAngleDifference = 90;
        public double squareStartingPointAngle = 45;

        public double additionalRotationAngle = 0;

        public Vector2[] tilePrototype;

        public Vector2 screenCenter;
        public Tile tileCenter;

        public void moveAdditionalRotation()
        {
            if (type == TileTypeEnum.Hexagon) 
            {
                additionalRotationAngle += hexagonPointAngleDifference;
            } 
            else
            {
                additionalRotationAngle += squarePointAngleDifference;
            }
        }


        public Vector2 rowDirection()
        {
            return Helper.direction(additionalRotationAngle + 90);
        }

        public Vector2 vectorHex(int i)
        {
            return (tileRadius - padding) * Helper.direction(i * hexagonPointAngleDifference + hexagonStartingPointAngle + additionalRotationAngle);
        }

        public Vector2 vectorSq(int i)
        {
            return (tileRadius - padding) * Helper.direction(i * squarePointAngleDifference + squareStartingPointAngle + additionalRotationAngle);
        }

        public Vector2 neighburDifferenceHex(int i)
        {
            return hexagonEdgeDistance * 2 * Helper.direction(i * hexagonPointAngleDifference + additionalRotationAngle);
        }

        public Vector2 neighburDifferenceSq(int i)
        {
            return   squareEdgeDistance * 2 * Helper.direction(i * squarePointAngleDifference + additionalRotationAngle);
        }

        public Vector2 neighburDifference(int i)
        {
            if (type == TileTypeEnum.Hexagon) return neighburDifferenceHex(i);
            if (type == TileTypeEnum.Square) return neighburDifferenceSq(i);
            return Vector2.Zero;
        }


        public Vector2[] points()
        {
            if (type == TileTypeEnum.Hexagon) return hexagonPoints();
            if (type == TileTypeEnum.Square) return squarePoints();
            return null; 
        }

        public Vector2[] hexagonPoints()
        {
            Vector2[] points = new Vector2[6];
            for (int i = 0; i < 6; i++)
            {
                points[i] = vectorHex(i);
            }
            return points;
        }

        public Vector2[] squarePoints()
        {
            Vector2[] points = new Vector2[6];
            Vector2 v1 = vectorSq(0);
            Vector2 v2 = vectorSq(1);
            Vector2 v3 = vectorSq(2);
            Vector2 v4 = vectorSq(3);
            points[0] = v1;
            points[1] = Helper.between(v1, v2);
            points[2] = v2;
            points[3] = v3;
            points[4] = Helper.between(v3, v4);
            points[5] = v4;
            return points;
        }

        public float transformationPercentage
        {
            get
            {
                return Math.Max(0, Math.Min((float)(transformTime / transformationLength),1));
            }
        }

        public float ha
        {
            get
            {
                return (float)(Math.Sqrt(3) / 2);
            }
        }

        public float sqrt2
        {
            get
            {
                return (float)Math.Sqrt(2);
            }
        }


        public float hexagonEdgeDistance
        {
            get {
                return ha * tileRadius;
            }
        }

        public float squareEdgeDistance
        {
            get
            {
                return tileRadius / sqrt2;
            }
        }


        public float edgeDistance
        {
            get
            {
                if (type == TileTypeEnum.Hexagon) return hexagonEdgeDistance;
                else return squareEdgeDistance;
            }
        }

        public float squareDistance
        {
            get
            {
                return squareEdgeDistance * 2;
            }
        }

        public float hexagonDistance
        {
            get
            {
                return hexagonEdgeDistance * 2;
            }
        }

        public float hexagonHeghth
        {
            get
            {
                return (tileRadius * 3) / 2;
            }
        }


        public int startingAngle
        {
            get
            {
                return angle / 2;
            }
        }

        public int angle
        {
            get
            {
                if (type == TileTypeEnum.Hexagon) return 60;
                if (type == TileTypeEnum.Square) return 90;
                return 0;
            }
        }

        public int neighburs
        {
            get
            {
                if (type == TileTypeEnum.Hexagon) return 6;
                if (type == TileTypeEnum.Square) return 4;
                return 0;
            }
        }
        

        public Vector2 direction(int i)
        {
            return Helper.direction(i * angle);
        }

        public Vector2 getCenter()
        {
            return new Vector2(gd.Viewport.Width / 2, gd.Viewport.Height / 2);
        }

        public void centerTiles()
        {
            Vector2 tileCenterPosition = tileCenter.position;
            foreach (Tile tile in tiles)
            {
                tile.position = tile.position - tileCenterPosition + screenCenter;
            }
        }


        public void setPoints()
        {
            Vector2 tileCenterPosition = tileCenter.position;
            foreach (Tile tile in tiles)
            {
                tile.transformPoints();
            }
        }

        public Tile[] chosenHexes;
        public Tile[] mouseHoveringOverGroup;

        public TileMap(GraphicsDevice gd)
        {
            this.type = TileTypeEnum.Square;
            this.gd = gd;
            this.tilePrototype = points();
            for (int row = 0; row < 11; row++) {
                for (int column = 0; column < 11; column++)
                {
                    Tile t = new Tile()
                    {
                        color = colorOptions[Random.Shared.Next() % colorOptions.Length],
                        position = new Vector2(column * squareDistance, row * squareDistance),
                        map = this
                    };
                    tiles.Add(t);
                    if (row == 5 && column == 5) tileCenter = t;
                }
            }
            screenCenter = getCenter();
            centerTiles();
            setPoints();
            chosenHexes = pickUniq();
        }

        public Tile[] takeWithSameColour(Tile start)
        {
            Queue<Tile> toProcess = new Queue<Tile>();
            toProcess.Enqueue(start);
            HashSet<Tile> visited = new HashSet<Tile>();
            visited.Add(start);
            while (toProcess.Count > 0) { 
                Tile tile = toProcess.Dequeue();
                for (int i = 0; i < neighburs; i++)
                {
                    Tile next = tile.getNeighbur(i);
                    if (next != null && !visited.Contains(next) && next.color == start.color) {
                        toProcess.Enqueue(next);
                        visited.Add(next);
                    }
                }
            }
            return visited.ToArray();
        }

        public Tile[][] groupTiles()
        {
            List<Tile[]> groups = new List<Tile[]>();
            HashSet<Tile> visited = new();
            foreach (Tile tile in tiles)
            {
                if(visited.Contains(tile)) continue;
                Tile[] group = takeWithSameColour(tile);
                visited.UnionWith(group);
                groups.Add(group);
            }
            return groups.ToArray();
        }

        public Tile[] pickUniq()
        {
            Tile[][] groups = groupTiles();
            Dictionary<int, int> counted = new Dictionary<int, int>();
            foreach (Tile[] group in groups) {
               if(!counted.ContainsKey(group.Length)) counted[group.Length] = 0;
               counted[group.Length]++;
            }
            foreach (KeyValuePair<int, int> c in counted)
            {
                if(c.Value != 1)
                {
                    counted.Remove(c.Key);
                }
            }
            Tile[][] remainingGroups = groups.Where(el => counted.ContainsKey(el.Length)).ToArray();
            return remainingGroups.Length != 0 ? remainingGroups[Random.Shared.Next() % remainingGroups.Length] : groups[Random.Shared.Next() % groups.Length];
        }

        public Tile[] pickRandomGroup()
        {
            return takeWithSameColour(pickRandomTile());
        }

        public Tile pickRandomTile()
        {
            return tiles.ElementAt(Random.Shared.Next() % tiles.Count);
        }

         public void evenRowShift()
        {
            Vector2 columnDirection = Helper.direction(additionalRotationAngle);
            Vector2 rowDirection = Helper.direction(additionalRotationAngle + 90);
            Vector2 startingPoint = tiles.ElementAt(0).position;
            if (type == TileTypeEnum.Square)
            {
                foreach (Tile tile in tiles)
                {
                    Vector2 hexColumnDirection = hexagonDistance * columnDirection;
                    Vector2 hexRowDirection = hexagonHeghth * rowDirection;
                    float hexRowIntensity = Helper.intensity(tile.position - startingPoint, hexRowDirection);
                    Vector2 move = Math.Round(hexRowIntensity) % 2 == 0 ? hexColumnDirection / 2 : Vector2.Zero;
                    tile.position -= move;
                }
            }
            else
            {
                foreach (Tile tile in tiles)
                {
                    Vector2 squareColumnDirection = squareDistance * columnDirection;
                    Vector2 squareRowDirection = squareDistance * rowDirection;
                    float squareRowIntensity = Helper.intensity(tile.position - startingPoint, squareRowDirection);
                    Vector2 move = Math.Round(squareRowIntensity) % 2 == 0 ? squareColumnDirection / 2 : Vector2.Zero;
                    tile.position += move;
                }
            }
        }

        public void adjustRowAndColumnDistances()
        {
            Vector2 columnDirection = Helper.direction(additionalRotationAngle);
            Vector2 rowDirection = Helper.direction(additionalRotationAngle + 90);
            Vector2 startingPoint = tiles.ElementAt(0).position;
            if (type == TileTypeEnum.Square)
            {
                foreach (Tile tile in tiles)
                {
                    float rowIntensity = Helper.intensity(tile.position, rowDirection);
                    float columnIntensity = Helper.intensity(tile.position, columnDirection);
                    tile.position = ((columnDirection * columnIntensity * squareDistance) / hexagonDistance) + ((rowDirection * rowIntensity * squareDistance) / hexagonHeghth);
                }
            }
            else
            {
                foreach (Tile tile in tiles)
                {
                    float rowIntensity = Helper.intensity(tile.position, rowDirection);
                    float columnIntensity = Helper.intensity(tile.position, columnDirection);
                    tile.position = ((columnDirection * columnIntensity * hexagonDistance) / squareDistance) + ((rowDirection * rowIntensity * hexagonHeghth) / squareDistance);
                }
            }
        }

        public void rotateAll()
        {
            moveAdditionalRotation();
            tilePrototype = points();
            foreach (Tile tile in tiles)
            {
                tile.recalculatePoints();
            }
        }

        public void transform(GameTime gameTime)
        {
            transforming = true;
            transformTime = gameTime.ElapsedGameTime.TotalSeconds;
            if (type == TileTypeEnum.Hexagon) type = TileTypeEnum.Square; else type = TileTypeEnum.Hexagon;
            tilePrototype = points();
            evenRowShift();
            adjustRowAndColumnDistances();
            centerTiles();
            setPoints();
        }

        bool wasLeftMouseButtonDown = false;


        public void Draw(GameTime gameTime)
        {
            if (Mouse.GetState().LeftButton == ButtonState.Released)
            {
                wasLeftMouseButtonDown = false;
            }
            if (Mouse.GetState().LeftButton == ButtonState.Pressed && !wasLeftMouseButtonDown)
            {
                wasLeftMouseButtonDown = true;
                foreach (Tile tile in tiles)
                {
                    if (tile.mouseHovering() && chosenHexes.Contains(tile))
                    {
                        rotateAll();
                        transform(gameTime);
                        chosenHexes = pickUniq();
                        break;
                    }
                }
            }
            foreach(Tile tile in tiles)
            {
                if (tile.mouseHovering())
                {
                    mouseHoveringOverGroup = takeWithSameColour(tile);
                }
            }
            if (transforming)
            {
                transformTime += gameTime.ElapsedGameTime.TotalSeconds;
                if (transformTime > transformationLength)
                {
                    transforming = false;
                    transformTime = 0;
                }
            }
            foreach (Tile tile in tiles)
            {
                tile.Draw();
            }

            float xMin = float.PositiveInfinity;
            float yMin = float.PositiveInfinity;
            float xMax = float.NegativeInfinity;
            float yMax = float.NegativeInfinity;
            foreach (Tile tile in chosenHexes)
            {
                    if(xMin > tile.xMin) xMin = tile.xMin;
                    if(yMin > tile.yMin) yMin = tile.yMin;
                    if(xMax < tile.xMax) xMax = tile.xMax;
                    if(yMax < tile.yMax) yMax = tile.yMax;
            }
            float width = xMax - xMin;
            float height = yMax - yMin;
            Vector2 minV = new Vector2(xMin, yMin);
            Vector2 padding = new Vector2(10,10);
            float scale = 100 / width;
            foreach (Tile tile in chosenHexes)
            {
                Vector2[] v0 = Helper.addAll(tile.currentPoints, -minV);
                Vector2[] v1 = Helper.multiplyAll(v0, scale);
                Vector2[] v2 = Helper.addAll(v1, padding);
                Vector2 c0 = tile.position - minV;
                Vector2 c1 = c0 * scale;
                Vector2 c2 = c1 + padding;
                Tile.DrawPolygon(gd, v2, tile.color, c2);
            }

        }
    }
}
