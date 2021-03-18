using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
//using System.Windows.Forms;

namespace Lunar_Lander
{
    public struct Line
    {
        Point point1;
        Point point2;
    }
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        int screenWidth;
        int screenHeight;
        Texture2D backgroundTexture;
        Texture2D menuTexture;
        Texture2D landerTexture;
        enum screen { MAIN, LEVEL1, LEVEL2, WIN, LOSE, CREDITS, CONTROLS, HIGH_SCORES, NULL, QUIT }
        screen curScreen;
        Point mousePosition;
        public List<Point> polygonPoints;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.IsFullScreen = false;
            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;  
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;   
            _graphics.ApplyChanges();
            curScreen = screen.MAIN;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            polygonPoints = new List<Point>();
            screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            
        }

        public List<Point> createPolygon(Point P1, Point P2)
        {
            polygonPoints.Add(P1);
            polygonPoints.Add(P2);
            Divide(P1, P2);
            polygonPoints.Add(new Point(0, screenHeight));
            polygonPoints.Add(new Point(screenWidth, screenHeight));

            return polygonPoints;
        }

        public void Divide(Point P1, Point P2)
        {
            Point midPoint = new Point((P1.X - P2.X), (P1.Y - P2.Y));
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
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
                        mousePosition = Mouse.GetState().Position;
                        if (mousePosition.X > (screenWidth * 0.4) && mousePosition.X < screenWidth * 0.63)
                        {
                            if (mousePosition.Y > (screenHeight * 0.36) && mousePosition.Y < (screenHeight * 0.40))
                            {
                                //PLAY
                                lines = createPolygon(new Point(0, (int)(screenHeight * 0.2)), new Point(screenWidth, (int)(screenHeight * 0.2)));
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
                    break;
                case screen.LEVEL1:
                    break;
            }

            

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
