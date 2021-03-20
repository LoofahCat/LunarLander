using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Lunar_Lander
{
    public struct Line
    {
        Point point1;
        Point point2;
    }
    public class Game1 : Game
    {
        Random random;
        private GraphicsDeviceManager _graphics;
        private BasicEffect basicEffect;
        private List<VertexPositionColor> vertexList;
        private List<int> indexList;
        private List<Point> polygonPoints;
        private VertexPositionColor[] vertexTri;
        private int[] indexTri;
        private SpriteBatch _spriteBatch;
        int screenWidth;
        int screenHeight;
        double s;
        double mean = 0;
        double variance = 1;
        Texture2D backgroundTexture;
        Texture2D menuTexture;
        Texture2D landerTexture;
        enum screen { MAIN, LEVEL1, LEVEL2, WIN, LOSE, CREDITS, CONTROLS, HIGH_SCORES, NULL, QUIT }
        screen curScreen;
        Point mousePosition;
        public List<Point> lines;

        public Game1()
        {
            random = new Random();
            lines = new List<Point>();
            vertexList = new List<VertexPositionColor>();
            indexList = new List<int>();
            _graphics = new GraphicsDeviceManager(this);
            _graphics.IsFullScreen = false;//TODO: Test with True
            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;  
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;   
            _graphics.ApplyChanges();
            curScreen = screen.MAIN;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            s = 0.2;
        }

        public void prepNewTerrain()
        {
            lines.Clear();
            vertexList.Clear();
            indexList.Clear();
            polygonPoints.Clear();
            vertexTri = new VertexPositionColor[] { };
            indexTri = new int[] { };
        }
        /* Notes
         * Randomly select two landing zones (two points for each zone of a determined width)
         * Call Divide on (P1, Z1.1), (Z1.2, Z2.1), (Z2.2, P2)
         * Use a different rule of thumb rather than numDivisions (maybe set numDivisions = distance between points / [some-num])
         * Record landing zone locations for win condition
         */

        public void createPolygon(Point P1, Point P2)
        {
            int x = 0;
            int numDivisions = 10;
            polygonPoints = new List<Point>();
            polygonPoints.Add(P1);
            polygonPoints.Add(P2);//add landing zones depending on screen
            Divide(P1, P2, numDivisions);
            polygonPoints.Sort((p1, p2) => (p1.X.CompareTo(p2.X)));
            for (int i = 0; i < polygonPoints.Count; i++)
            {
                Point p = polygonPoints[i];
                Point pY = new Point(p.X, screenHeight);
                Vector3 vector = new Vector3(p.X, p.Y, 0);
                Vector3 vectorY = new Vector3(pY.X, pY.Y, 0);
                VertexPositionColor vpc = new VertexPositionColor();
                VertexPositionColor vpcY = new VertexPositionColor();
                vpc.Position = vector;
                vpcY.Position = vectorY;
                vpc.Color = Color.Black;
                vpcY.Color = Color.Black;
                vertexList.Add(vpc);
                vertexList.Add(vpcY);
                if(i == 0)
                {
                    //prep forward triangle only
                    x = 0;
                    indexList.Add(0);
                    indexList.Add(2);
                    indexList.Add(1);
                }
                else if(i == polygonPoints.Count - 1)
                {
                    //prep backward triangle only
                    indexList.Add(i + 1 + x);
                    indexList.Add(i + 2 + x);
                    indexList.Add(i + x);
                }
                else
                {
                    //prep both forward and backward triangle
                    //forward
                    indexList.Add(i + 1 + x);
                    indexList.Add(i + 2 + x);
                    indexList.Add(i + x);
                    //backward
                    indexList.Add(i + 1 + x);
                    indexList.Add(i + 3 + x);
                    indexList.Add(i + 2 + x);
                    x++;
                }
            }
            vertexTri = vertexList.ToArray();
            indexTri = indexList.ToArray();
        }

        public void Divide(Point P1, Point P2, int numDivisions)
        {
            if (numDivisions > 0)
            {
                //================================================================
                Random rand = new Random(); //reuse this if you are generating many
                double u1 = 1.0 - rand.NextDouble(); //uniform(0,1] random doubles
                double u2 = 1.0 - rand.NextDouble();
                double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                             Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
                double randNormal =
                             mean + variance * randStdNormal; //random normal(mean,stdDev^2)
                //================================================================

                Point midPoint = new Point((int)((P2.X + P1.X) / 2), (int)((P1.Y + P2.Y) / 2));
                if((int)((P1.Y + P2.Y) / 2 + (s * randNormal) * Math.Abs(P2.X - P1.X)) > screenHeight)
                {
                    midPoint.Y = (int)((P1.Y + P2.Y) / 2 + (s * randNormal * -1) * Math.Abs(P2.X - P1.X));
                }
                else if (((int)((P1.Y + P2.Y) / 2 + (s * randNormal) * Math.Abs(P2.X - P1.X)) < (screenHeight * 0.25)))
                {
                    midPoint.Y = (int)((P1.Y + P2.Y) / 2 + (s * randNormal * -1) * Math.Abs(P2.X - P1.X));
                }
                else
                {
                    midPoint.Y = (int)((P1.Y + P2.Y) / 2 + (s * randNormal) * Math.Abs(P2.X - P1.X));
                }
                polygonPoints.Add(midPoint);
                Divide(P1, midPoint, numDivisions - 1);
                Divide(midPoint, P2, numDivisions - 1);
            }
        }

        public Texture2D GetDotTexure(GraphicsDevice gd)
        {
            Texture2D tex = new Texture2D(gd, 1, 1);
            tex.SetData<Color>(new Color[1] { new Color((byte)255, (byte)255, (byte)255, (byte)255) });
            return tex;
        }

        protected override void Initialize()
        {
            _graphics.GraphicsDevice.RasterizerState = new RasterizerState
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.CullCounterClockwiseFace,   // CullMode.None If you want to not worry about triangle winding order
                MultiSampleAntiAlias = true,
            };

            basicEffect = new BasicEffect(_graphics.GraphicsDevice)
            {
                VertexColorEnabled = true,
                View = Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 1.0f), Vector3.Zero, Vector3.Up),
                Projection = Matrix.CreatePerspectiveOffCenter(0.0f, _graphics.GraphicsDevice.Viewport.Width, _graphics.GraphicsDevice.Viewport.Height, 0, 1.0f, 1000.0f)
            };

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            backgroundTexture = Content.Load<Texture2D>("space");
            menuTexture = Content.Load<Texture2D>("menu");
            landerTexture = Content.Load<Texture2D>("lunarLander");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            switch (curScreen)
            {
                case screen.MAIN:
                    if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                    {
                        mousePosition = Mouse.GetState().Position;//Allow for keyboard controls (arrow keys with floating box)
                        if (mousePosition.X > (screenWidth * 0.4) && mousePosition.X < screenWidth * 0.63)
                        {
                            if (mousePosition.Y > (screenHeight * 0.36) && mousePosition.Y < (screenHeight * 0.40))
                            {
                                //PLAY
                                createPolygon(new Point(0, (int)(screenHeight * (0.8))), new Point(screenWidth, (int)(screenHeight * (0.8)))); //TODO: Random doubles in range for start points
                                curScreen = screen.LEVEL1;
                            }
                            else if (mousePosition.Y > (screenHeight * 0.41) && mousePosition.Y < (screenHeight * 0.45))
                            {
                                //HIGH SCORES
                            }
                            else if (mousePosition.Y > (screenHeight * 0.46) && mousePosition.Y < (screenHeight * 0.49))
                            {
                                //CONTROLS
                            }
                            else if (mousePosition.Y > (screenHeight * 0.5) && mousePosition.Y < (screenHeight * 0.54))
                            {
                                //CREDITS
                            }
                        }
                    }
                    break;
                case screen.LEVEL1:
                    if (Keyboard.GetState().IsKeyDown(Keys.F1))
                    {
                        prepNewTerrain();
                        createPolygon(new Point(0, (int)(screenHeight * (0.8))), new Point(screenWidth, (int)(screenHeight * (0.8))));
                    }
                    break;
            }
            

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();


            _spriteBatch.Draw(backgroundTexture, new Rectangle(0, 0, screenWidth, screenHeight), Color.White);

            
            switch (curScreen) 
            {
                case screen.MAIN:
                    _spriteBatch.Draw(menuTexture, new Rectangle(screenWidth / 4, (int)(screenHeight * 0.21), screenWidth / 2, (int)(screenHeight * 0.42)), Color.White);
                    _spriteBatch.End();
                    break;
                case screen.LEVEL1:
                    _spriteBatch.End();
                    foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        _graphics.GraphicsDevice.DrawUserIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            vertexTri, 0, vertexTri.Length,
                            indexTri, 0, indexTri.Length / 3);
                    }
                    break;
            }

            

            
            base.Draw(gameTime);
        }
    }
}