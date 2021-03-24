using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using Microsoft.VisualBasic.CompilerServices;

namespace Lunar_Lander
{
    class Menu
    {
        Choice[] currentChoices;
        Choice[] mainChoices;
        Choice[] controlChoices;
        Choice[] quitChoices;
        Choice[] subscreenChoices;
        Choice[] lossChoices;
        Choice[] winChoices;
        float[] defaultPositionsY;
        float[] defaultPositionX;
        public Keys thrust { get; set; }
        public Keys rotLeft { get; set; }
        public Keys rotRight { get; set; }
        public Game1.screen destinationScreen { get; set; }
        Game1.screen curScreen;
        int screenWidth;
        int screenHeight;
        int iconPosition;
        string title;
        bool stopFunction;

        Texture2D Win1Texture;
        Texture2D Win2Texture;
        Texture2D LoseTexture;
        Texture2D iconTexture;
        Texture2D backgroundTexture;
        Texture2D menuTexture;
        SpriteFont menuFont;
        SpriteFont titleFont;

        public Menu(ContentManager content, int width, int height, Keys t, Keys r, Keys l)
        {
            screenWidth = width;
            screenHeight = height;
            iconPosition = 0;
            curScreen = Game1.screen.MAIN;
            title = "LUNAR LANDER";
            stopFunction = false;
            thrust = t;
            rotRight = r;
            rotLeft = l;

            backgroundTexture = content.Load<Texture2D>("space");
            menuTexture = content.Load<Texture2D>("menu");
            Win1Texture = content.Load<Texture2D>("Win1");
            Win2Texture = content.Load<Texture2D>("Win2");
            LoseTexture = content.Load<Texture2D>("Lose");
            iconTexture = content.Load<Texture2D>("icon");
            menuFont = content.Load<SpriteFont>("menuFont");
            titleFont = content.Load<SpriteFont>("titleFont");

            defaultPositionsY = new float[10] { screenHeight * 0.27f, screenHeight * 0.35f, screenHeight * 0.40f, screenHeight * 0.45f, screenHeight * 0.50f, screenHeight * 0.55f, screenHeight * 0.60f, screenHeight * 0.65f, screenHeight * 0.70f, screenHeight * 0.75f };
            defaultPositionX = new float[2] { screenWidth * 0.37f, screenWidth * 0.40f };
            mainChoices = new Choice[5]  {
                new Choice(Game1.action.PLAY, new Vector2(defaultPositionX[1], defaultPositionsY[1]), "PLAY", Game1.screen.LEVEL1),
                new Choice(Game1.action.HIGH_SCORES, new Vector2(defaultPositionX[1], defaultPositionsY[2]), "HIGH SCORES", Game1.screen.HIGH_SCORES),
                new Choice(Game1.action.CONTROLS, new Vector2(defaultPositionX[1], defaultPositionsY[3]), "CONTROLS", Game1.screen.CONTROLS),
                new Choice(Game1.action.CREDITS, new Vector2(defaultPositionX[1], defaultPositionsY[4]), "CREDITS", Game1.screen.CREDITS),
                new Choice(Game1.action.QUIT, new Vector2(defaultPositionX[1], defaultPositionsY[5]), "QUIT")
            };

            controlChoices = new Choice[4]  {
                new Choice(Game1.action.CHANGE_UP, new Vector2(defaultPositionX[1], defaultPositionsY[1]),    "THRUST               [ " + thrust.ToString() + " ]"),
                new Choice(Game1.action.CHANGE_LEFT, new Vector2(defaultPositionX[1], defaultPositionsY[2]),  "ROTATE LEFT      [ " + rotLeft.ToString() + " ]"),
                new Choice(Game1.action.CHANGE_RIGHT, new Vector2(defaultPositionX[1], defaultPositionsY[3]), "ROTATE RIGHT  [ " + rotRight.ToString() + " ]"),
                new Choice(Game1.action.BACK, new Vector2(defaultPositionX[1], defaultPositionsY[4]), "BACK", Game1.screen.MAIN)
            };

            subscreenChoices = new Choice[1]  {
                new Choice(Game1.action.BACK, new Vector2(defaultPositionX[1], defaultPositionsY[1]), "BACK", Game1.screen.MAIN)
            };

            quitChoices = new Choice[2]  {
                new Choice(Game1.action.CONTINUE, new Vector2(defaultPositionX[1], defaultPositionsY[1]), "CONTINUE"),
                new Choice(Game1.action.QUIT, new Vector2(defaultPositionX[1], defaultPositionsY[2]), "QUIT"),
            };

            lossChoices = new Choice[2]
            {
                new Choice(Game1.action.PLAY, new Vector2(defaultPositionX[1], defaultPositionsY[1]), "TRY AGAIN?", Game1.screen.LEVEL1),
                new Choice(Game1.action.BACK, new Vector2(defaultPositionX[1], defaultPositionsY[2]), "RETURN TO MAIN MENU", Game1.screen.MAIN)
            };

            winChoices = new Choice[2]
            {
                new Choice(Game1.action.PLAY, new Vector2(defaultPositionX[1], defaultPositionsY[1]), "PLAY AGAIN", Game1.screen.LEVEL1),
                new Choice(Game1.action.BACK, new Vector2(defaultPositionX[1], defaultPositionsY[2]), "RETURN TO MAIN MENU", Game1.screen.MAIN)
            };

            currentChoices = mainChoices; 
        }

        public void updateThrust(Keys key)
        {
            thrust = key;
            controlChoices[0] = new Choice(Game1.action.CHANGE_UP, new Vector2(defaultPositionX[1], defaultPositionsY[1]), "THRUST               [ " + thrust.ToString() + " ]");
        }
        public void updateRotLeft(Keys key)
        {
            rotLeft = key;
            controlChoices[1] = new Choice(Game1.action.CHANGE_LEFT, new Vector2(defaultPositionX[1], defaultPositionsY[2]), "ROTATE LEFT      [ " + rotLeft.ToString() + " ]");
        }
        public void updateRotRight(Keys key)
        {
            rotRight = key;
            controlChoices[2] = new Choice(Game1.action.CHANGE_RIGHT, new Vector2(defaultPositionX[1], defaultPositionsY[3]), "ROTATE RIGHT  [ " + rotRight.ToString() + " ]");
        }

        public Game1.action Update(Game1.screen scr)
        {
            Game1.action action = Game1.action.NULL;
            bool preventInput = false;
            if(curScreen != scr)
            {
                preventInput = true;
                iconPosition = 0;
                curScreen = scr;
                switch (scr)
                {
                    case Game1.screen.MAIN:
                        title = "LUNAR LANDER";
                        currentChoices = mainChoices;
                        break;
                    case Game1.screen.HIGH_SCORES:
                        title = "HIGH SCORES";
                        currentChoices = subscreenChoices;
                        break;
                    case Game1.screen.CONTROLS:
                        title = "CONTROL CONFIGURATION";
                        currentChoices = controlChoices;
                        break;
                    case Game1.screen.CREDITS:
                        title = "CREDITS";
                        currentChoices = subscreenChoices;
                        break;
                    case Game1.screen.LEVEL1:
                        title = "";
                        currentChoices = new Choice[0];
                        break;
                    case Game1.screen.LEVEL2:
                        title = "";
                        currentChoices = new Choice[0];
                        break;
                    case Game1.screen.QUIT:
                        title = "ARE YOU SURE YOU WANT TO QUIT?";
                        currentChoices = quitChoices;
                        break;
                    case Game1.screen.LOSE:
                        title = "MISSION FAILED";
                        currentChoices = lossChoices;
                        break;
                    case Game1.screen.WIN1:
                        title = "LEVEL 2";
                        currentChoices = new Choice[0];
                        break;
                    case Game1.screen.WIN2:
                        title = "MISSION COMPLETE";
                        currentChoices = winChoices;
                        break;
                }
            }
            else
            {
                preventInput = false;
            }

            if (!preventInput && !stopFunction)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    action = Game1.action.QUIT;
                }
                else if (Keyboard.GetState().IsKeyDown(Keys.Enter))
                {
                    action = currentChoices[iconPosition].action;
                    stopFunction = true;
                }
                else if (Keyboard.GetState().IsKeyDown(Keys.Down))
                {
                    iconPosition += (iconPosition + 1) > currentChoices.Length-1 ? 0 : 1;
                    stopFunction = true;
                }
                else if (Keyboard.GetState().IsKeyDown(Keys.Up))
                {
                    iconPosition -= (iconPosition - 1) < 0 ? 0 : 1;
                    stopFunction = true;
                }
            }
            if (Keyboard.GetState().GetPressedKeyCount() == 0)
            {
                stopFunction = false;
            }

            return action;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(backgroundTexture, new Rectangle(0, 0, screenWidth, screenHeight), Color.White);
            
            switch (curScreen)
            {
                case Game1.screen.MAIN:
                case Game1.screen.WIN1:
                case Game1.screen.WIN2:
                case Game1.screen.LOSE:
                case Game1.screen.HIGH_SCORES:
                case Game1.screen.CREDITS:
                case Game1.screen.CONTROLS:
                case Game1.screen.QUIT:
                    if(currentChoices.Length > 0)
                        spriteBatch.Draw(iconTexture, new Rectangle((int)defaultPositionX[1] - (int)(screenHeight * 0.04), (int)currentChoices[iconPosition].position.Y + 20, (int)(screenWidth * 0.02f), (int)(screenWidth * 0.02f)), Color.White);
                    spriteBatch.DrawString(titleFont, title, new Vector2(defaultPositionX[0], defaultPositionsY[0]), Color.White);
                    foreach (Choice choice in currentChoices)
                    {
                        spriteBatch.DrawString(menuFont, choice.key, choice.position, Color.White);
                    }

                    if(curScreen == Game1.screen.HIGH_SCORES)
                    {
                        spriteBatch.DrawString(menuFont, "1. " + Game1.HighScores[0].ToString(), new Vector2(defaultPositionX[1], defaultPositionsY[3]), Color.White);
                        spriteBatch.DrawString(menuFont, "2. " + Game1.HighScores[1].ToString(), new Vector2(defaultPositionX[1], defaultPositionsY[4]), Color.White);
                        spriteBatch.DrawString(menuFont, "3. " + Game1.HighScores[2].ToString(), new Vector2(defaultPositionX[1], defaultPositionsY[5]), Color.White);
                        spriteBatch.DrawString(menuFont, "4. " + Game1.HighScores[3].ToString(), new Vector2(defaultPositionX[1], defaultPositionsY[6]), Color.White);
                        spriteBatch.DrawString(menuFont, "5. " + Game1.HighScores[4].ToString(), new Vector2(defaultPositionX[1], defaultPositionsY[7]), Color.White);
                    }
                    if(curScreen == Game1.screen.CREDITS)
                    {
                        spriteBatch.DrawString(menuFont, "Game Developed By: ", new Vector2(defaultPositionX[1], defaultPositionsY[3]), Color.White);
                        spriteBatch.DrawString(menuFont, "Abraham Gunther", new Vector2(defaultPositionX[1], defaultPositionsY[4]), Color.White);
                    }
                    if(curScreen == Game1.screen.LOSE)
                    {
                        spriteBatch.DrawString(menuFont, "Your Score: 0... Because you died...", new Vector2(defaultPositionX[1] - (int)(screenHeight * 0.1), defaultPositionsY[4]), Color.White);
                    }
                    if (curScreen == Game1.screen.WIN1 || curScreen == Game1.screen.WIN2)
                    {
                        spriteBatch.DrawString(menuFont, "Your Score: " + Game1.currentScore.ToString(), new Vector2(defaultPositionX[1] - (int)(screenHeight * 0.1), defaultPositionsY[4]), Color.White);
                    }
                    break;
            }
            spriteBatch.End();
        }
    }
    class Choice 
    {
        public Game1.action action;
        public Vector2 position;
        public string key;

        public Choice(Game1.action act, Vector2 pos, string k, Game1.screen destination = Game1.screen.NULL)
        {
            action = act;
            position = pos;
            key = k;
        }
    }
}
