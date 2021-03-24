using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Media;
using System.IO;

namespace Lunar_Lander
{
    public struct Line
    {
        public Point point1;
        public Point point2;
        public double length { get { return Math.Sqrt(Math.Pow((point2.X - point1.X), 2) + Math.Pow((point2.Y - point1.Y), 2)); } }
    }

    public class Lander {
        public Vector2 position { get; set; }
        public float angle;
        public double fuel;
        public double velocity;
    }

    public class Game1 : Game
    {
        public static int[] HighScores { get; set; }
        public Keys[] KeyConfig { get; set; }
        public static int currentScore { get; set; }
        Random random;
        Menu myMenu;
        Lander lander;
        Keys rotRight;
        Keys rotLeft;
        Keys thrust;
        bool keyPressed;
        public static bool winner { get; set; }
        bool stop;
        bool changingKey;
        bool themePlaying;
        bool emitParticles;
        bool crashed;
        Line landingZone1;
        Line landingZone2;
        int landingZoneWidth;
        int transitionTime;
        Point collisionPoint;
        ParticleEmitter particleEmitterSmoke;
        ParticleEmitter particleEmitterFire;
        private GraphicsDeviceManager _graphics;
        private BasicEffect basicEffect;
        private List<VertexPositionColor> vertexList;
        private List<int> indexList;
        private List<Point> polygonPoints;
        private VertexPositionColor[] vertexTri;
        private int[] indexTri;
        private SpriteBatch _spriteBatch;
        private SpriteFont font;
        private List<SoundEffect> soundEffects;
        Song theme;
        int screenWidth;
        int screenHeight;
        double s;
        double mean = 0;
        double variance = 1;
        Texture2D landerTexture;

        float gravitationalForce;
        float horizontalMomentum;
        DateTime lastIteration;
        DateTime now;
        double millisecondsElapsed;
        DateTime themePlayTime;
        
        public enum screen { MAIN, LEVEL1, LEVEL2, WIN1, WIN2, LOSE, CREDITS, CONTROLS, HIGH_SCORES, NULL, QUIT }
        public enum action { PLAY, HIGH_SCORES, CREDITS, CONTROLS, BACK, QUIT, CONTINUE, CHANGE_UP, CHANGE_LEFT, CHANGE_RIGHT, NULL }
        public screen curScreen;
        Point mousePosition;
        public List<Point> lines;

        public Game1()
        {
            polygonPoints = new List<Point>();
            collisionPoint = new Point();
            keyPressed = false;
            winner = false;
            stop = false;
            emitParticles = false;
            themePlaying = false;
            crashed = false;
            changingKey = false;
            lander = new Lander();
            lines = new List<Point>();
            vertexList = new List<VertexPositionColor>();
            indexList = new List<int>();
            soundEffects = new List<SoundEffect>();
            _graphics = new GraphicsDeviceManager(this);
            _graphics.IsFullScreen = true;
            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;  
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;   
            _graphics.ApplyChanges();
            curScreen = screen.MAIN;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            s = 0.2;
            landingZoneWidth = 100;
            gravitationalForce = 0;
            horizontalMomentum = 0;
            screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            random = new Random();

            LoadKeys();
            LoadScores();
            rotLeft = KeyConfig[1];
            rotRight = KeyConfig[2];
            thrust = KeyConfig[0];

            myMenu = new Menu(Content, screenWidth, screenHeight, thrust, rotRight, rotLeft);
        }

        public void LoadKeys()
        {
            KeyConfig = new Keys[3];
            string line = "";
            int iterator = 0;
            System.IO.StreamReader reader = new System.IO.StreamReader(Content.RootDirectory + "/keyConfig.txt");
            while((line = reader.ReadLine()) != null)
            {
                Keys key = (Keys)Enum.Parse(typeof(Keys), line, true);
                KeyConfig[iterator] = key;
                iterator++;
            }
        }

        public void SaveKeys()
        {
            using (StreamWriter outputFile = new StreamWriter(Content.RootDirectory + "/keyConfig.txt"))
            {
                foreach (Keys key in KeyConfig)
                {
                    outputFile.WriteLine(key.ToString());
                }
            }
        }

        public void SaveHighScores()
        {
            using (StreamWriter outputFile = new StreamWriter(Content.RootDirectory + "/HighScores.txt"))
            {
                foreach(int score in HighScores)
                {
                    outputFile.WriteLine(score.ToString());
                }
            }
        }

        public void LoadScores()
        {
            HighScores = new int[5];
            string line = "";
            int iterator = 0;
            System.IO.StreamReader reader = new System.IO.StreamReader(Content.RootDirectory + "/HighScores.txt");
            while ((line = reader.ReadLine()) != null)
            {
                HighScores[iterator] = Int32.Parse(line);
                iterator++;
            }
        }

        public void generateScore()
        {
            bool replaced = false;
            if (curScreen == screen.WIN1)
            {
                currentScore += (int)lander.fuel * 5;
            }
            else if (curScreen == screen.WIN2)
            {
                currentScore += (int)lander.fuel * 50;
            }
            for(int i = 0; i < HighScores.Length; i++)
            {
                if(currentScore > HighScores[i] && !replaced)
                {
                    HighScores[i] = currentScore;
                    replaced = true;
                }
            }
        }

        public void Quit()
        {
            SaveHighScores();
            SaveKeys();
            Exit();
        }

        public void prepNewTerrain(screen scr = screen.LEVEL1)
        {
            Random random = new Random();
            stop = false;
            curScreen = scr;
            lines.Clear();
            vertexList.Clear();
            indexList.Clear();
            polygonPoints.Clear();
            vertexTri = new VertexPositionColor[] { };
            indexTri = new int[] { };
            transitionTime = 0;
            landingZone1.point1 = new Point();
            landingZone1.point2 = new Point();
            landingZone2.point1 = new Point();
            landingZone2.point2 = new Point();
            lander = new Lander();
            lander.position = new Vector2(random.Next((int)(screenWidth * 0.15), (int)(screenWidth * 0.85)), (float)(screenHeight * 0.1));
            lander.velocity = 0;
            lander.fuel = scr == screen.LEVEL1 ? 100 : 50;
            lander.angle = (float)random.Next(0, 6);
            winner = false;
            crashed = false;
            gravitationalForce = 0;
            horizontalMomentum = 0;
            lastIteration = DateTime.Now;
            particleEmitterSmoke = new ParticleEmitter(
            Content, 
            new TimeSpan(0, 0, 0, 0, 2), 
            (int)lander.position.X, 
            (int)lander.position.Y, 
            20, 
            200, 
            new TimeSpan(0, 0, 2), 
            new TimeSpan(0, 0, 0, 3000));
            particleEmitterFire = new ParticleEmitter(
            Content,
            new TimeSpan(0, 0, 0, 0, 10),
            (int)lander.position.X, 
            (int)lander.position.Y, 
            40,
            300,
            new TimeSpan(0, 0, 0, 0, 1000),
            new TimeSpan(0, 0, 0, 0, 800));
            
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
                vpc.Color = Color.LightGray;
                vpcY.Color = Color.LightGray;
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

            //Include point in landing zone for win collision
            if(curScreen == screen.LEVEL1)
            {
                polygonPoints.Add(new Point(landingZone1.point1.X + (landingZoneWidth / 2), landingZone1.point1.Y));
                polygonPoints.Add(new Point(landingZone2.point1.X + (landingZoneWidth / 2), landingZone2.point1.Y));
            }
            else
            {
                polygonPoints.Add(new Point(landingZone1.point1.X + (landingZoneWidth / 2), landingZone1.point1.Y));
            }
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

        public bool detectCollision()
        {
            bool collided = false;
            double distance = 0;
            float landerRadius = (screenWidth * 0.03958f) / 2;
            Point origin = new Point((int)lander.position.X, (int)lander.position.Y);

            //find closest terrain point
            List<Point> applicablePoints = polygonPoints.FindAll(x => ((x.X > origin.X - landerRadius) && (x.X < origin.X + landerRadius)));
            List<Point> applicableYPoints = polygonPoints.FindAll(x => ((x.Y > origin.Y - landerRadius) && (x.Y < origin.Y + landerRadius)));
            applicablePoints.AddRange(applicableYPoints);

            //For each applicable point, is it within the bounding circle?
            foreach(Point p in applicablePoints)
            {
                //distance formula: d=Sqrt((x2 - x1)^2 + (y2 - y1)^2)
                distance = Math.Sqrt(Math.Pow(origin.X - p.X, 2) + Math.Pow(origin.Y - p.Y, 2));
                if(distance < landerRadius)
                {
                    collided = true;
                    collisionPoint = p;
                }
            }

            return collided;
        }

        public bool isWin()
        {
            bool win = false;
            if ((collisionPoint.X > landingZone1.point1.X && collisionPoint.X < landingZone1.point2.X)
                || (curScreen == screen.LEVEL1 && (collisionPoint.X > landingZone2.point1.X && collisionPoint.X < landingZone2.point2.X)))
            {
                if ((((int)Math.Abs((lander.angle * (180 / Math.PI)) % 360)) < 5 && ((int)Math.Abs((lander.angle * (180 / Math.PI)) % 360)) >= 0)
                        || ((int)Math.Abs((lander.angle * (180 / Math.PI)) % 360)) > 355 && ((int)Math.Abs((lander.angle * (180 / Math.PI)) % 360)) <= 360
                        && gravitationalForce < 0.4f)
                {
                    win = true;
                }
            }
            return win;
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
            landerTexture = Content.Load<Texture2D>("lunarLander");
            soundEffects.Add(Content.Load<SoundEffect>("435413__v-ktor__explosion12"));
            soundEffects.Add(Content.Load<SoundEffect>("success"));
            theme = Content.Load<Song>("bensound-slowmotion");

        }

        protected override void Update(GameTime gameTime)
        {
            now = DateTime.Now;
            millisecondsElapsed = (now - lastIteration).TotalMilliseconds;
            action act = myMenu.Update(curScreen);
            if (changingKey)
            {
                if(Keyboard.GetState().GetPressedKeyCount() == 1 && Keyboard.GetState().IsKeyUp(Keys.Enter))
                {
                    if (thrust == Keys.None)
                    {
                        thrust = Keyboard.GetState().GetPressedKeys()[0];
                        KeyConfig[0] = thrust;
                        myMenu.updateThrust(this.thrust);
                    }
                    else if(rotLeft == Keys.None)
                    {
                        rotLeft = Keyboard.GetState().GetPressedKeys()[0];
                        KeyConfig[1] = rotLeft;
                        myMenu.updateRotLeft(this.rotLeft);
                    }
                    else if(rotRight == Keys.None)
                    {
                        rotRight = Keyboard.GetState().GetPressedKeys()[0];
                        KeyConfig[2] = rotRight;
                        myMenu.updateRotRight(this.rotRight);
                    }
                    changingKey = false;
                }
            }
            else
            {


                if (act != action.NULL)
                {
                    switch (act)
                    {
                        case action.PLAY:
                            prepNewTerrain(screen.LEVEL1);
                            createPolygon(new Point(0, (int)(screenHeight * (0.8))), new Point(screenWidth, (int)(screenHeight * (0.8))));
                            break;
                        case action.HIGH_SCORES:
                            curScreen = screen.HIGH_SCORES;
                            break;
                        case action.CONTROLS:
                            curScreen = screen.CONTROLS;
                            break;
                        case action.CREDITS:
                            curScreen = screen.CREDITS;
                            break;
                        case action.BACK:
                            curScreen = myMenu.destinationScreen;
                            break;
                        case action.QUIT://Generic quit vs. Terminate?
                            Quit();
                            break;
                        case action.CONTINUE://Quit menu
                            break;
                        case action.CHANGE_UP:
                            thrust = Keys.None;
                            myMenu.updateThrust(Keys.None);
                            changingKey = true;
                            break;
                        case action.CHANGE_LEFT:
                            rotLeft = Keys.None;
                            myMenu.updateRotLeft(Keys.None);
                            changingKey = true;
                            break;
                        case action.CHANGE_RIGHT:
                            rotRight = Keys.None;
                            myMenu.updateRotRight(Keys.None);
                            changingKey = true;
                            break;

                    }
                }
                switch (curScreen)
                {
                    case screen.WIN1:
                        System.Threading.Thread.Sleep(3000);
                        if (curScreen == screen.WIN1)
                        {
                            prepNewTerrain(screen.LEVEL2);

                            createPolygon(new Point(0, (int)(screenHeight * (0.8))), new Point(screenWidth, (int)(screenHeight * (0.8)))); //TODO: Random doubles in range for start points
                        }
                        break;
                    case screen.LEVEL1:
                    case screen.LEVEL2:
                        if (Keyboard.GetState().IsKeyDown(Keys.F1))
                        {
                            if (!keyPressed)
                            {
                                prepNewTerrain();
                                createPolygon(new Point(0, (int)(screenHeight * (0.8))), new Point(screenWidth, (int)(screenHeight * (0.8))));
                                keyPressed = true;
                            }
                        }
                        if (Keyboard.GetState().IsKeyDown(rotLeft))
                        {
                            lander.angle -= 0.04f;
                        }
                        if (Keyboard.GetState().IsKeyDown(rotRight))
                        {
                            lander.angle += 0.04f;
                        }
                        if (Keyboard.GetState().IsKeyDown(thrust))
                        {

                            if (lander.fuel > 0)
                            {
                                emitParticles = true;
                                Vector2 direction = new Vector2((float)Math.Cos(lander.angle - Math.PI / 2), (float)Math.Sin(lander.angle - Math.PI / 2));
                                horizontalMomentum += direction.X * 0.04f;
                                gravitationalForce += direction.Y * 0.04f;
                                lander.fuel -= 0.1f;
                            }
                            else
                                emitParticles = false;
                        }
                        else
                        {
                            emitParticles = false;
                        }

                        if (crashed)
                        {
                            transitionTime += (int)millisecondsElapsed;
                            if (transitionTime < 500)
                            {
                                particleEmitterFire.shipCrash(lander.position);
                                //particleEmitterSmoke.shipCrash(lander.position);
                                particleEmitterFire.shipThrust(gameTime, lander.position, true, -1);
                                particleEmitterSmoke.shipThrust(gameTime, lander.position, false, -1);
                            }
                            else if (transitionTime < 2000)
                            {
                                particleEmitterFire.shipThrust(gameTime, lander.position, false, -1);
                                particleEmitterSmoke.shipThrust(gameTime, lander.position, false, -1);
                            }
                            else
                            {
                                crashed = false;
                                curScreen = screen.LOSE;

                            }
                        }
                        else
                        {
                            float landerRadius = (screenWidth * 0.03958f) / 2;
                            particleEmitterSmoke.shipThrust(gameTime, lander.position + new Vector2(((float)Math.Cos(lander.angle + Math.PI / 2) * landerRadius), ((float)Math.Sin(lander.angle + Math.PI / 2) * landerRadius)), emitParticles, lander.angle, 0.3);
                            particleEmitterFire.shipThrust(gameTime, lander.position + new Vector2(((float)Math.Cos(lander.angle + Math.PI / 2) * landerRadius), ((float)Math.Sin(lander.angle + Math.PI / 2) * landerRadius)), emitParticles, lander.angle, 0.2);
                        }

                        if (detectCollision() && !stop)
                        {
                            //detect win/loss condition
                            if (isWin())
                            {
                                //1. Wait while sound plays
                                //2. play sound
                                //3. Switch to WIN1 screen
                                MediaPlayer.Stop();
                                soundEffects[1].CreateInstance().Play();
                                stop = true;
                                if (curScreen == screen.LEVEL1)
                                {
                                    curScreen = screen.WIN1;
                                }
                                else
                                {
                                    curScreen = screen.WIN2;
                                }
                                myMenu.Update(curScreen);
                                System.Threading.Thread.Sleep(1000);


                                themePlaying = false;
                                generateScore();
                            }
                            else
                            {
                                //1. Wait while ship explodes
                                //2. play sound
                                //3. Switch to lose screen
                                MediaPlayer.Stop();
                                soundEffects[0].CreateInstance().Play();
                                stop = true;
                                crashed = true;
                                lander.fuel = 0;
                                lander.angle = 0;
                                gravitationalForce = 0;
                                themePlaying = false;
                                currentScore = 0;
                            }
                        }


                        if (!stop)
                        {
                            //Move Lander
                            //Lander position.X += horizontalmomentum
                            //Lander Position.Y += gravitationalForce
                            //Given direction of lander, increment/decrement gravity and momentum given thrust_YN
                            Vector2 movement = new Vector2(horizontalMomentum, gravitationalForce);
                            lander.position += movement * (int)millisecondsElapsed * 0.2f;
                            gravitationalForce += 0.01f;
                            if (horizontalMomentum != 0)
                            {
                                if (horizontalMomentum > 0)
                                    horizontalMomentum -= 0.01f;
                                else
                                    horizontalMomentum += 0.01f;
                            }
                        }

                        break;
                }
            }
            if (Keyboard.GetState().GetPressedKeyCount() == 0)
                keyPressed = false;
            
            lastIteration = now;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (!themePlaying)
            {
                MediaPlayer.Play(theme);
                themePlaying = true;
                themePlayTime = DateTime.Now;
            }
            else
            {
                if(themePlayTime.AddMilliseconds(theme.Duration.TotalMilliseconds) < now)
                {
                    themePlaying = false;
                }
            }
            GraphicsDevice.Clear(Color.CornflowerBlue);

            
            myMenu.Draw(_spriteBatch);
            _spriteBatch.Begin();
            switch (curScreen) 
            {
                case screen.LEVEL1:
                case screen.LEVEL2:
                    if(!crashed)
                        _spriteBatch.Draw(landerTexture, new Rectangle((int)lander.position.X, (int)lander.position.Y, 76, 76), null, Color.White, lander.angle, new Vector2(landerTexture.Width / 2f, landerTexture.Height / 2f), SpriteEffects.None, 0);
                   
                    if((((int)Math.Abs((lander.angle * (180 / Math.PI)) % 360)) < 5 && ((int)Math.Abs((lander.angle * (180 / Math.PI)) % 360)) >= 0)
                        || ((int)Math.Abs((lander.angle * (180 / Math.PI)) % 360)) > 355 && ((int)Math.Abs((lander.angle * (180 / Math.PI)) % 360)) <=360)
                        _spriteBatch.DrawString(font, "Angle: " + ((int)Math.Abs((lander.angle * (180/Math.PI)) % 360)).ToString(), new Vector2((float)(screenWidth * 0.85), (float)(screenHeight * 0.15)), Color.LightGreen);
                    else
                        _spriteBatch.DrawString(font, "Angle: " + ((int)Math.Abs((lander.angle * (180 / Math.PI)) % 360)).ToString(), new Vector2((float)(screenWidth * 0.85), (float)(screenHeight * 0.15)), Color.White);
                    if(lander.fuel > 0)
                    {
                        _spriteBatch.DrawString(font, "Fuel: " + (Math.Truncate(lander.fuel * 100) / 100).ToString(), new Vector2((float)(screenWidth * 0.85), (float)(screenHeight * 0.20)), Color.LightGreen);
                    }
                    else
                    {
                        _spriteBatch.DrawString(font, "Fuel: 0", new Vector2((float)(screenWidth * 0.85), (float)(screenHeight * 0.20)), Color.White);
                    }
                    if(gravitationalForce > 0.4f)
                    {
                        _spriteBatch.DrawString(font, "Velocity: " + (Math.Abs(Math.Truncate(gravitationalForce * 10))).ToString(), new Vector2((float)(screenWidth * 0.85), (float)(screenHeight * 0.25)), Color.White);
                    }
                    else
                    {
                        _spriteBatch.DrawString(font, "Velocity: " + (Math.Abs(Math.Truncate(gravitationalForce * 10))).ToString(), new Vector2((float)(screenWidth * 0.85), (float)(screenHeight * 0.25)), Color.LightGreen);
                    }

                    //if thrust is engaged, draw particles

                    particleEmitterSmoke.Draw(_spriteBatch);
                    particleEmitterFire.Draw(_spriteBatch);
                    
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
                default:
                    _spriteBatch.End();
                    break;
            }

            

            
            base.Draw(gameTime);
        }
    }
}