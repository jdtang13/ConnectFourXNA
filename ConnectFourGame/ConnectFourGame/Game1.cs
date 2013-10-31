using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace ConnectFourGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        const int BOARD_WIDTH = 7;
        const int BOARD_HEIGHT = 6;
        const int LINE_THICKNESS = 1;

        int SCREEN_WIDTH;
        int SCREEN_HEIGHT;

        int MENU_WIDTH = 175;

        int tileWidth;
        int tileHeight;

        bool gameOver;

        const int WIN_CONDITION = 4;

        char RED = 'r';
        char BLACK = 'b';
        char EMPTY = 'e';
        char NO_WINNER = 'n';

        int piecesRemaining;

        char playerTurn;

        string winMessage;
        string turnMessage;
        string debugMessage = "";

        string mouseMessage = "";
        string positionMessage = "";

        SpriteFont font;

        Texture2D line;
        Texture2D pieceTexture;
        Texture2D menuTexture;

        //  given an "x", returns the "y" value that the next piece should
        //  fall onto
        int[] topCoordinate;

        char[,] board;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {

            IsMouseVisible = true;

            board = new char[BOARD_WIDTH, BOARD_HEIGHT];
            SCREEN_WIDTH = graphics.GraphicsDevice.Viewport.Width - MENU_WIDTH;
            SCREEN_HEIGHT = graphics.GraphicsDevice.Viewport.Height;

            tileWidth = SCREEN_WIDTH / BOARD_WIDTH;
            tileHeight = SCREEN_HEIGHT / BOARD_HEIGHT;

            line = new Texture2D(graphics.GraphicsDevice, 1, 1);
            line.SetData(new Color[] { Color.Black });

            topCoordinate = new int[BOARD_WIDTH];
            for (int i = 0; i < BOARD_WIDTH; i++)
            {
                for (int j = 0; j < BOARD_HEIGHT; j++)
                {
                    board[i, j] = EMPTY;
                }
                topCoordinate[i] = 0;
            }

            pieceTexture = new Texture2D(graphics.GraphicsDevice, 1, 1);
            pieceTexture.SetData(new Color[] { Color.White });

            menuTexture = new Texture2D(graphics.GraphicsDevice, 1, 1);
            menuTexture.SetData(new Color[] { Color.CornflowerBlue });

            playerTurn = RED;

            winMessage = "Game ongoing.";
            turnMessage = "It is RED's turn.";
            piecesRemaining = BOARD_HEIGHT * BOARD_WIDTH;
            gameOver = false;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("menufont");
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        ///

        MouseState oldState;

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            MouseState mouse = Mouse.GetState();
            mouseMessage = "Screen: (" + mouse.X + ", " + mouse.Y + ")";

            Vector2 dropPosition = MousePositionToGrid(mouse.X, mouse.Y);
            positionMessage = "Board: (" + dropPosition.X + ", " + dropPosition.Y + ")";

            debugMessage = piecesRemaining + " pieces remaining.";

            if (!gameOver && mouse.LeftButton == ButtonState.Pressed && oldState.LeftButton != ButtonState.Pressed)
            {
                if (!dropPosition.Equals(new Vector2(-1, -1)) 
                    && board[(int)dropPosition.X, (int)dropPosition.Y] == EMPTY)
                {

                    dropPosition = DropPieceAt(dropPosition, playerTurn);
                    char winner = CheckWinner(dropPosition, playerTurn);

                    if (winner == RED)
                    {
                        winMessage = "RED has won!";
                        gameOver = true;
                    }
                    else if (winner == BLACK)
                    {
                        winMessage = "YELLOW has won!";
                        gameOver = true;
                    }
                    else if (winner == NO_WINNER)
                    {
                        winMessage = "It's a draw!";
                        gameOver = true;
                    }

                    //  if the new piece creates four in a row,
                    //  then return the winner. else return EMPTY

                    if (playerTurn == BLACK)
                    {
                        playerTurn = RED;
                        turnMessage = "It is RED's turn.";
                    }
                    else
                    {
                        playerTurn = BLACK;
                        turnMessage = "It is YELLOW's turn.";
                    }

                    if (gameOver) turnMessage = "Click to restart.";
                }

            }
            else if (gameOver && mouse.X >= SCREEN_WIDTH 
                && mouse.LeftButton == ButtonState.Pressed && oldState.LeftButton != ButtonState.Pressed)
            {
                Initialize();
            }
            oldState = mouse;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            Color bgColor = new Color(255, 200, 162); // light salmon color
            if (gameOver) bgColor = Color.DarkGray;   // change color when someone wins

            GraphicsDevice.Clear(bgColor);

            spriteBatch.Begin();

            for (int i = 0; i < BOARD_WIDTH; i++)
            {
                for (int j = 0; j < BOARD_HEIGHT; j++)
                {
                    if (board[i, j] != EMPTY)
                    {
                        Color pieceColor = Color.PaleGoldenrod;
                        if (board[i, j] == RED) pieceColor = Color.Crimson;

                        spriteBatch.Draw(pieceTexture, 
                            new Rectangle(i*tileWidth, (BOARD_HEIGHT-j-1)*tileHeight, tileWidth, tileHeight), pieceColor);
                    }
                }
            }

            for (int x = 1; x <= BOARD_WIDTH; x++)
            {
                //  draw vertical lines
                spriteBatch.Draw(line, new Rectangle(x * tileWidth, 0,
                    LINE_THICKNESS, SCREEN_HEIGHT), Color.White);
            }
            for (int y = 0; y < BOARD_HEIGHT; y++)
            {
                //  draw horizontal lines
                spriteBatch.Draw(line, new Rectangle(0, y * tileHeight,
                    SCREEN_WIDTH, LINE_THICKNESS), Color.White);
            }

            spriteBatch.Draw(menuTexture,
                new Rectangle(SCREEN_WIDTH-5, 0, MENU_WIDTH+5, SCREEN_HEIGHT), Color.White);

            spriteBatch.DrawString(font, winMessage, new Vector2(SCREEN_WIDTH + 5, 10), Color.White);
            spriteBatch.DrawString(font, turnMessage, new Vector2(SCREEN_WIDTH + 5, 40), Color.White);
            spriteBatch.DrawString(font, debugMessage, new Vector2(SCREEN_WIDTH + 5, 70), Color.White);
            spriteBatch.DrawString(font, mouseMessage, new Vector2(SCREEN_WIDTH + 5, 100), Color.White);
            spriteBatch.DrawString(font, positionMessage, new Vector2(SCREEN_WIDTH + 5, 130), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        Vector2 MousePositionToGrid(int x, int y)
        {
            int boardX = x / tileWidth; //  find the number of whole number tiles
                                        //  corresponding to this coordinate

            int boardY = BOARD_HEIGHT - 1 - (y / tileHeight); // inverted

            if (boardX >= BOARD_WIDTH || boardX < 0 || boardY >= BOARD_HEIGHT || boardY < 0)
                return new Vector2(-1, -1);

            return new Vector2(boardX, boardY);
        }

        Vector2 DropPieceAt(Vector2 position, char playerColor)
        {
            //  red or black
            //  return the position the piece was dropped at. if no piece as dropped
            //  then it returns vector[-1, -1]
            Vector2 pos = new Vector2(-1,-1);
            if (board[(int)position.X, (int)position.Y] == EMPTY)
            {
                piecesRemaining--;
                board[(int)position.X, topCoordinate[(int)position.X]] = playerColor;
                pos = new Vector2((int)position.X, topCoordinate[(int)position.X]);
                topCoordinate[(int)position.X]++;
            }
            return pos;
        }

        Vector2 GravitatedPosition(Vector2 position)
        {
            return new Vector2((int)position.X, topCoordinate[(int)position.X]);
        }

        char CheckWinner(Vector2 position, char turn)
        {
            if (piecesRemaining <= 0) return NO_WINNER;
  
            int x = (int) position.X;
            int y = (int) position.Y;
            char color = turn;

            int matchCount = 0;

            //  check horizontals
            for (int i = x; i < BOARD_WIDTH && i < x + WIN_CONDITION; i++)
            {
                if (board[i, y] == color) matchCount++;
            }
            for (int i = x - 1; i >= 0 && i > x - WIN_CONDITION; i--)
            {
                if (board[i, y] == color) matchCount++;
            }
            
            if (matchCount >= WIN_CONDITION) return color;
            else matchCount = 0;

            //  check verticals
            for (int i = y; i < BOARD_HEIGHT && i < y + WIN_CONDITION; i++)
            {
                if (board[x, i] == color) matchCount++;
                else
                {
                    break;
                }
            }
            for (int i = y - 1; i >= 0 && i > y - WIN_CONDITION; i--)
            {
                if (board[x, i] == color) matchCount++;
                else
                {
                    break;
                }
            }

            if (matchCount >= WIN_CONDITION) return color;
            else matchCount = 0;

            //  check diagonal from bottom-left to top-right
            for (int i = x, j = y; i < BOARD_WIDTH && j < BOARD_HEIGHT; i++, j++) {
                if (board[i, j] == color) matchCount++;
                else
                {
                    break;
                }
            }
            for (int i = x-1, j = y-1; i >=0 && j >=0; i--, j--)
            {
                if (board[i, j] == color) matchCount++;
                else
                {
                    break;
                }
            }
            //debugMessage = matchCount + " BL-TR diagonal matches.";

            if (matchCount >= WIN_CONDITION) return color;
            else matchCount = 0;

            //  check diagonal from bottom-right to top-left

            for (int i = x, j = y; i < BOARD_WIDTH && j >= 0; i++, j--)
            {
                if (board[i, j] == color) matchCount++;
                else
                {
                    break;
                }
            }
            for (int i = x - 1, j = y + 1; i >= 0 && j < BOARD_HEIGHT; i--, j++)
            {
                if (board[i, j] == color) matchCount++;
                else
                {
                    break;
                }
            }

            //debugMessage = matchCount + " BR-TL diagonal matches.";
            if (matchCount >= WIN_CONDITION) return color;
            else matchCount = 0;

            return EMPTY;
        }

    }
}
