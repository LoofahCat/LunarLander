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
        public Point point1;
        public Point point2;
        public double length { get { return Math.Sqrt(Math.Pow((point2.X - point1.X), 2) + Math.Pow((point2.Y - point1.Y), 2)); } }
    }

    public class Lander {
        
        //public Vector2 centerPosition { get { return new Vector2(rectangle.Left + (rectangle.Right - rectangle.Left) / 2, rectangle.Top + (rectangle.Bottom - rectangle.Top) / 2); } }
        public Vector2 position { get; set; }
        public float angle;
        public double fuel;
        public double velocity;


    }

    public class Game1 : Game
    {
        Random random;
        Lander lander;
        Keys rotRight;
        Keys rotLeft;
        Keys thrust;
        bool keyPressed;
        Line landingZone1;
        Line landingZone2;
        int landingZoneWidth;
        private GraphicsDeviceManager _graphics;
        private BasicEffect basicEffect;
        private List<VertexPositionColor> vertexList;
        private List<int> indexList;
        private List<Point> polygonPoints;
        private VertexPositionColor[] vertexTri;
        private int[] indexTri;
        private SpriteBatch _spriteBatch;
        private SpriteFont font;
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
            keyPressed = false;
            lander = new Lander();
            rotLeft = Keys.Left; //TODO: Allow user to access keys
            rotRight = Keys.Right;
            thrust = Keys.Up;
            lines = new List<Point>();
            vertexList = new List<VertexPositionColor>();
            indexList = new List<int>();
            _graphics = new GraphicsDeviceManager(this);
            _graphics.IsFullScreen = true;//TODO: Test with True
            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;  
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;   
            _graphics.ApplyChanges();
            curScreen = screen.MAIN;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            s = 0.2;
            landingZoneWidth = 100;
        }

        public void prepNewTerrain()
        {
            lines.Clear();
            vertexList.Clear();
            indexList.Clear();
            polygonPoints.Clear();
            vertexTri = new VertexPositionColor[] { };
            indexTri = new int[] { };
            landingZone1.point1 = new Point();
            landingZone1.point2 = new Point();
            landingZone2.point1 = new Point();
            landingZone2.point2 = new Point();
            lander = new Lander();
            lander.position = new Vector2(100, 100);
            lander.velocity = 0;
            lander.fuel = 100;
            lander.angle = 1f;
        }
        /* Notes
         * Randomly select two landing zones (two points for each zone of a determined width)
         * Call Divide on (P1, Z1.1), (Z1.2, Z2.1), (Z2.2, P2)
         * Use a different rule of thumb rather than numDivisions (log base 2 of the length of the line for consistency on each terrain section)
         * Record landing zone locations for win condition
         */

        public void createPolygon(Point P1, Point P2)
        {
            int x = 0;
            Point LZ11 = new Point(random.Next((int)(screenWidth * 0.15), (int)(screenWidth * 0.85) - landingZoneWidth), random.Next((int)(screenHeight * 0.2), (int)(screenHeight * 0.6)));
            Point LZ12 = new Point(LZ11.X + landingZoneWidth, LZ11.Y);
            landingZone1.point1 = LZ11;
            landingZone1.point2 = LZ12;
            if (curScreen == screen.LEVEL1)
            {
                bool goodZone = false;
                Point LZ21 = new Point();
                Point LZ22 = new Point();
                while (!goodZone)//Make sure landing zone 2 does not overlap landing zone 1 (within one landingzone width)
                {
                    LZ21 = new Point(random.Next((int)(screenWidth * 0.15), (int)(screenWidth * 0.85) - landingZoneWidth), random.Next((int)(screenHeight * 0.2), (int)(screenHeight * 0.6)));
                    LZ22 = new Point(LZ21.X + landingZoneWidth, LZ21.Y);
                    if((LZ22.X > (LZ11.X - landingZoneWidth) && LZ22.X < (LZ12.X + landingZoneWidth)) || (LZ21.X > (LZ11.X - landingZoneWidth) && LZ21.X < (LZ12.X + landingZoneWidth)))
                    {
                        goodZone = false;
                    }
                    else
                    {
                        goodZone = true;
                    }
                }
                landingZone2.point1 = LZ21;
                landingZone2.point2 = LZ22;
            }
            int ruleOfThumb; //Rule of thumb: log base 2 of line width
            polygonPoints = new List<Point>();
            polygonPoints.Add(P1);
            polygonPoints.Add(P2);//add landing zones depending on screen
            polygonPoints.Add(landingZone1.point1);
            polygonPoints.Add(landingZone1.point2);
            if(curScreen == screen.LEVEL1)
            {
                polygonPoints.Add(landingZone2.point1);
                polygonPoints.Add(landingZone2.point2);
            }
            //calculate rule of thumb and run divide for each line between landing zones
            if(curScreen == screen.LEVEL1)
            {
                //determine which landingzone is closer to the left
                if(landingZone1.point1.X < landingZone2.point1.X)
                {
                    //landingZone1 is closer to left
                    //P1 to lz1.1, lz1.2 to lz2.1, lz2.2 to P2
                    Line leftToLanding1 = new Line();
                    leftToLanding1.point1 = P1;
                    leftToLanding1.point2 = landingZone1.point1;
                    ruleOfThumb = (int)Math.Log2(leftToLanding1.length);
                    Divide(leftToLanding1.point1, leftToLanding1.point2, ruleOfThumb);

                    Line landing1ToLanding2 = new Line();
                    landing1ToLanding2.point1 = landingZone1.point2;
                    landing1ToLanding2.point2 = landingZone2.point1;
                    ruleOfThumb = (int)Math.Log2(landing1ToLanding2.length);
                    Divide(landing1ToLanding2.point1, landing1ToLanding2.point2, ruleOfThumb);

                    Line landing2ToRight = new Line();
                    landing2ToRight.point1 = landingZone2.point2;
                    landing2ToRight.point2 = P2;
                    ruleOfThumb = (int)Math.Log2(landing2ToRight.length);
                    Divide(landing2ToRight.point1, landing2ToRight.point2, ruleOfThumb);
                }
                else
                {
                    //landingZone2 is closer to left
                    //P1 to lz2.1, lz2.2 to lz1.1, lz1.2 to P2
                    Line leftToLanding2 = new Line();
                    leftToLanding2.point1 = P1;
                    leftToLanding2.point2 = landingZone2.point1;
                    ruleOfThumb = (int)Math.Log2(leftToLanding2.length);
                    Divide(leftToLanding2.point1, leftToLanding2.point2, ruleOfThumb);

                    Line landing2ToLanding1 = new Line();
                    landing2ToLanding1.point1 = landingZone2.point2;
                    landing2ToLanding1.point2 = landingZone1.point1;
                    ruleOfThumb = (int)Math.Log2(landing2ToLanding1.length);
                    Divide(landing2ToLanding1.point1, landing2ToLanding1.point2, ruleOfThumb);

                    Line landing1ToRight = new Line();
                    landing1ToRight.point1 = landingZone1.point2;
                    landing1ToRight.point2 = P2;
                    ruleOfThumb = (int)Math.Log2(landing1ToRight.length);
                    Divide(landing1ToRight.point1, landing1ToRight.point2, ruleOfThumb);
                }
            }
            else
            {
                Line leftToLanding = new Line();
                leftToLanding.point1 = P1;
                leftToLanding.point2 = landingZone1.point1;
                ruleOfThumb = (int)Math.Log2(leftToLanding.length);
                Divide(leftToLanding.point1, leftToLanding.point2, ruleOfThumb);

                Line landingToRight = new Line();
                landingToRight.point1 = landingZone1.point2;
                landingToRight.point2 = P2;
                ruleOfThumb = (int)Math.Log2(landingToRight.length);
                Divide(landingToRight.point1, landingToRight.point2, ruleOfThumb);
            }
            
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
                if((int)((P1.Y + P2.Y) / 2 + (s * randNormal) * Math.Abs(P2.X - P1.X)) > (screenHeight * 0.6))
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
            font = Content.Load<SpriteFont>("font");
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
                                lander = new Lander();
                                lander.position = new Vector2(100, 100);
                                lander.velocity = 0;
                                lander.fuel = 100;
                                lander.angle = 1f;
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
                        if (!keyPressed)
                        {
                            prepNewTerrain();
                            createPolygon(new Point(0, (int)(screenHeight * (0.8))), new Point(screenWidth, (int)(screenHeight * (0.8))));
                            keyPressed = true;
                        }
                    }
                    else if (Keyboard.GetState().IsKeyDown(rotLeft))
                    {
                        lander.angle -= 0.04f;
                    }
                    else if (Keyboard.GetState().IsKeyDown(rotRight))
                    {
                        lander.angle += 0.04f;
                    }
                    else if (Keyboard.GetState().IsKeyDown(thrust))
                    {
                        Vector2 direction = new Vector2((float)Math.Cos(lander.angle - Math.PI/2), (float)Math.Sin(lander.angle - Math.PI/2));
                        lander.position += direction * 2;
                    }
                    break;
            }
            if (Keyboard.GetState().GetPressedKeyCount() == 0)
                keyPressed = false;

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
                    _spriteBatch.Draw(landerTexture, new Rectangle((int)lander.position.X, (int)lander.position.Y, 76, 76), null, Color.White, lander.angle, new Vector2(landerTexture.Width / 2f, landerTexture.Height / 2f), SpriteEffects.None, 0);
                    _spriteBatch.DrawString(font, "Angle: " + ((int)Math.Abs((lander.angle * (180/Math.PI)) % 360)).ToString(), new Vector2((float)(screenWidth * 0.85), (float)(screenHeight * 0.15)), Color.White);
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

//https://community.monogame.net/t/collision-detection-with-rotating-crosses-or-similar/7760/2