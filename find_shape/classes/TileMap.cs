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
using proof_of_concept_find_shape.classes;

namespace monogame_cros_platform.classes
{
    public class TileMap
    {
        static float calcHexTileRadius()
        {
            return squareTileRadius / (float)(Math.Pow(27, 1 / 4));
        }

        public Color[] colorOptions = new Color[] { Color.Red, Color.Blue, Color.Yellow };
        public List<Tile> tiles = new List<Tile>();
        public GraphicsDevice gd;
        public float padding = 1;
        public static float squareTileRadius = 20;
        public float hexTileRadius = calcHexTileRadius();
        public TileTypeEnum type = TileTypeEnum.Hexagon;
        public float mapWidening = 100;

        public int transforming = 0;
        public double transformTime = 0;
        public double transformationLength = 0.4;

        public double hexagonPointAngleDifference = 60;
        public double hexagonStartingPointAngle = 30;

        public double squarePointAngleDifference = 90;
        public double squareStartingPointAngle = 45;

        public double additionalRotationAngle = 0;

        public Vector2[] tilePrototype;

        public Vector2 screenCenter;
        public Tile tileCenter;

        private BasicEffect getBasicEfect(GraphicsDevice _gd)
        {
            BasicEffect be = new BasicEffect(_gd)
            {
                VertexColorEnabled = true,
                Projection = Matrix.CreateOrthographicOffCenter(
                0,
                _gd.Viewport.Width,
                _gd.Viewport.Height,
                0,
                0,
                1
                ),
            };
            // for smooth edges
            _gd.RasterizerState = new RasterizerState
            {
                MultiSampleAntiAlias = true,
                CullMode = CullMode.None
            };
            return be;
        }

        public BasicEffect basicEffect;

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
            return (hexTileRadius - padding) * Helper.direction(i * hexagonPointAngleDifference + hexagonStartingPointAngle + additionalRotationAngle);
        }

        public Vector2 vectorSq(int i)
        {
            return (squareTileRadius - padding) * Helper.direction(i * squarePointAngleDifference + squareStartingPointAngle + additionalRotationAngle);
        }

        public Vector2 neighburDifferenceHex(int i)
        {
            return hexagonEdgeDistance * 2 * Helper.direction(i * hexagonPointAngleDifference + additionalRotationAngle);
        }

        public Vector2 neighburDifferenceSq(int i)
        {
            return squareEdgeDistance * 2 * Helper.direction(i * squarePointAngleDifference + additionalRotationAngle);
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
                return Math.Max(0, Math.Min((float)(transformTime / transformationLength), 1));
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
            get
            {
                return ha * hexTileRadius;
            }
        }

        public float squareEdgeDistance
        {
            get
            {
                return squareTileRadius / sqrt2;
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

        public float hexagonHegth
        {
            get
            {
                return (hexTileRadius * 3) / 2;
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

        public void setNextPoints()
        {
            Vector2 tileCenterPosition = tileCenter.position;
            foreach (Tile tile in tiles)
            {
                tile.transformNextPoints();
            }
        }

        public Tile[] chosenHexes;
        public Tile[] mouseHoveringOverGroup;

        public TileMap(GraphicsDevice gd)
        {
            this.type = TileTypeEnum.Square;
            this.gd = gd;
            this.tilePrototype = points();
            for (int row = 0; row < 11; row++)
            {
                for (int column = 0; column < 11; column++)
                {
                    Tile t = new Tile()
                    {
                        color = pickRandom(colorOptions),
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
            basicEffect = getBasicEfect(gd);
        }

        public Tile[] takeWithSameColour(Tile start)
        {
            Queue<Tile> toProcess = new Queue<Tile>();
            toProcess.Enqueue(start);
            HashSet<Tile> visited = new HashSet<Tile>
            {
                start
            };
            while (toProcess.Count > 0)
            {
                Tile tile = toProcess.Dequeue();
                for (int i = 0; i < neighburs; i++)
                {
                    Tile next = tile.getNeighbur(i);
                    if (next != null && !visited.Contains(next) && next.color == start.color)
                    {
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
                if (visited.Contains(tile)) continue;
                Tile[] group = takeWithSameColour(tile);
                visited.UnionWith(group);
                groups.Add(group);
            }
            return groups.ToArray();
        }

        public T pickRandom<T>(IEnumerable<T> en)
        {
            return en.ElementAt(Random.Shared.Next() % en.Count());
        }

        public Tile[] pickUniq()
        {
            Tile[][] groups = groupTiles();
            Dictionary<int, int> counted = new Dictionary<int, int>();
            // count how many in each group of tiles with same number of tiles
            foreach (Tile[] group in groups)
            {
                if (!counted.ContainsKey(group.Length)) counted[group.Length] = 0;
                counted[group.Length]++;
            }
            // remove groups with more than 1 tile;
            // remaining is groups with unique number tiles;
            foreach (KeyValuePair<int, int> c in counted)
            {
                if (c.Value != 1)
                {
                    counted.Remove(c.Key);
                }
            }
            // take groups;
            Tile[][] g1 = groups.Where(el => counted.ContainsKey(el.Length)).ToArray();
            if (g1.Length != 0) return pickRandom(g1);
            // take groups with more than one tile;
            Tile[][] g2 = groups.Where(el => el.Length != 1).ToArray();
            if (g2.Length != 0) return pickRandom(g2);
            // take any group;
            return pickRandom(groups);
        }

        public void evenRowShiftGeneralised(float columnDistance, float rowDistance, bool plus = true)
        {
            Vector2 columnDirection = columnDistance * Helper.direction(additionalRotationAngle);
            Vector2 rowDirection = rowDistance * Helper.direction(additionalRotationAngle + 90);
            Vector2 startingPoint = tiles.ElementAt(0).position;
            foreach (Tile tile in tiles)
            {
                float rowIntensity = Helper.intensity(tile.position - startingPoint, rowDirection);
                Vector2 move = Math.Round(rowIntensity) % 2 == 0 ? columnDirection / 2 : Vector2.Zero;
                if (plus) tile.position += move; else tile.position -= move;
            }
        }

        public void adjustRowAndColumnDistancesGeneralised(float columnFrom, float columnTo, float rowFrom, float rowTo)
        {
            Vector2 columnDirection = Helper.direction(additionalRotationAngle);
            Vector2 rowDirection = Helper.direction(additionalRotationAngle + 90);
            Vector2 startingPoint = tiles.ElementAt(0).position;
            foreach (Tile tile in tiles)
            {
                float rowIntensity = Helper.intensity(tile.position, rowDirection);
                float columnIntensity = Helper.intensity(tile.position, columnDirection);
                tile.position = ((columnDirection * columnIntensity * columnTo) / columnFrom) + ((rowDirection * rowIntensity * rowTo) / rowFrom);
            }
        }

        public void adjustRowAndColumnDistances()
        {
            if (type == TileTypeEnum.Square)
            {
                adjustRowAndColumnDistancesGeneralised(hexagonDistance, squareDistance, hexagonHegth, squareDistance);
            }
            else
            {
                adjustRowAndColumnDistancesGeneralised(squareDistance, hexagonDistance, squareDistance, hexagonHegth);
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

        public void transformPoints()
        {
            if (type == TileTypeEnum.Square)
            {
                tilePrototype = points();
                adjustRowAndColumnDistances();
                centerTiles();
                setPoints();
                evenRowShiftGeneralised(squareDistance, squareDistance, true);
                centerTiles();
                setNextPoints();
            }
            else
            {
                evenRowShiftGeneralised(squareDistance, squareDistance, false);
                centerTiles();
                setPoints();
                tilePrototype = points();
                adjustRowAndColumnDistances();
                centerTiles();
                setNextPoints();
            }
        }

        public void transform(GameTime gameTime)
        {
            transforming = 1;
            transformTime = gameTime.ElapsedGameTime.TotalSeconds;
            if (type == TileTypeEnum.Hexagon) type = TileTypeEnum.Square; else type = TileTypeEnum.Hexagon;
            transformPoints();
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
                bool clickedChosenHexes = false;
                foreach (Tile tile in tiles)
                {
                    if (tile.mouseHovering() && chosenHexes.Contains(tile))
                    {
                        clickedChosenHexes = true;
                        AppGame.sharpSound.Play();
                        rotateAll();
                        transform(gameTime);
                        chosenHexes = pickUniq();
                        break;
                    }
                }
                if (!clickedChosenHexes) AppGame.softSound.Play();
            }

            var oldMouseHoveringOverGroup = mouseHoveringOverGroup;
            mouseHoveringOverGroup = null;
            foreach (Tile tile in tiles)
            {
                if (tile.mouseHovering())
                {
                    mouseHoveringOverGroup = takeWithSameColour(tile);
                    break;
                }
            }
            if (transforming == 1 || transforming == 2) transformTime += gameTime.ElapsedGameTime.TotalSeconds;
            if (transforming == 1)
            {
                if (transformTime > transformationLength)
                {
                    foreach (Tile tile in tiles)
                    {
                        tile.shiftPoints();
                    }
                    transforming = 2;
                    transformTime = 0;
                }
            }
            if (transforming == 2)
            {
                if (transformTime > transformationLength)
                {
                    foreach (Tile tile in tiles)
                    {

                    }
                    transforming = 0;
                    transformTime = 0;
                }
            }
            foreach (Tile tile in tiles)
            {
                tile.Draw();
            }

            TilePoints[] tps = chosenHexes.Select(el => el.lastPoints).ToArray();
            MinMax mm = new MinMax(tps);
            float width = mm.xMax - mm.xMin;
            float height = mm.yMax - mm.yMin;
            float largerSide = width > height ? width : height;
            Vector2 minV = new Vector2(mm.xMin, mm.yMin);
            Vector2 padding = new Vector2(10, 10);
            float scale = 100 / largerSide;
            foreach (Tile tile in chosenHexes)
            {
                TilePoints tp = tile.lastPoints;
                TilePoints tp1 = tp + (-minV);
                TilePoints tp2 = tp1 * scale;
                TilePoints tp3 = tp2 + padding;
                Tile.DrawPolygon(basicEffect, tp3, tile.color);
            }
        }
    }
}
