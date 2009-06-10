﻿#region File Description & Usings
//-----------------------------------------------------------------------------
// GameState.cs
//
// Created by Poplicola
//-----------------------------------------------------------------------------
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion

namespace UndeadClient
{
    public interface IGameState
    {
        bool InWorld { get; set; }
    }

    public class GameState : GameComponent, IGameState
    {
        // Input service
        Input.IInputHandler m_InputService;
        TileEngine.ITileEngine m_TileEngineService;
        GameObjects.IGameObjects m_GameObjectsService;
        TileEngine.IWorld m_WorldService;
        GUI.IGUI m_GUIService;
        Network.IGameClient m_GameClientService;

        // Debug message
        public string DebugMessage { get { return m_DebugMessage(); } }
        public bool TakeScreenshot { get; protected set; }

        public bool InWorld { get; set; }

        public GameState(Game game)
            : base(game)
        {
            game.Services.AddService(typeof(IGameState), this);
            InWorld = false;
        }

        public override void Initialize()
        {
            base.Initialize();
            m_InputService = (Input.IInputHandler)Game.Services.GetService(typeof(Input.IInputHandler));
            m_TileEngineService = (TileEngine.ITileEngine)Game.Services.GetService(typeof(TileEngine.ITileEngine));
            m_GameObjectsService = (GameObjects.IGameObjects)Game.Services.GetService(typeof(GameObjects.IGameObjects));
            m_WorldService = (TileEngine.IWorld)Game.Services.GetService(typeof(TileEngine.IWorld));
            m_GUIService = (GUI.IGUI)Game.Services.GetService(typeof(GUI.IGUI));
            m_GameClientService = (Network.IGameClient)Game.Services.GetService(typeof(Network.IGameClient));
        }

        public void LoadContent()
        {
            // Don't do anything...
        }

        public override void Update(GameTime gameTime)
        {
            this.TakeScreenshot = false;

            base.Update(gameTime);

            // Get a pick type
            if (m_GUIService.IsMouseOverGUI(m_InputService.Mouse.Position))
            {
                m_TileEngineService.PickType = UndeadClient.TileEngine.PickTypes.PickNothing;
            }
            else
            {
                if (m_InputService.Mouse.Buttons[0].IsDown)
                {
                    m_TileEngineService.PickType = TileEngine.PickTypes.PickStatics | TileEngine.PickTypes.PickObjects | TileEngine.PickTypes.PickGroundTiles;
                }
                else
                {
                    m_TileEngineService.PickType = TileEngine.PickTypes.PickStatics | TileEngine.PickTypes.PickObjects;
                }
            }

            mParseKeyboard(m_InputService.Keyboard);

            mUpdateFPS(gameTime);
        }

        public void UpdateAfter()
        {
            if (m_InputService.Mouse.Buttons[0].Press)
            {
                TileEngine.IMapObject iGroundTile = m_TileEngineService.MouseOverGroundTile;
                if (iGroundTile != null)
                {
                    ((GameObjects.Unit)m_GameObjectsService.GetObject(m_GameObjectsService.MyGUID)).Move(
                        (int)iGroundTile.Position.X,
                        (int)iGroundTile.Position.Y,
                        (int)iGroundTile.Z);
                }
            }

            // Check for a move event from the player ...
            try
            {
                int iDirection = 0, iSequence = 0, iKey = 0;
                bool iMoveEvent = m_GameObjectsService.GetObject(m_GameObjectsService.MyGUID).Movement.GetMoveEvent(ref iDirection, ref iSequence, ref iKey);
                if (iMoveEvent)
                {
                    m_GameClientService.Send_MoveRequest(iDirection, iSequence, iKey);
                }
            }
            catch
            {
                // The player has not yet been loaded
            }
        }

        private Vector3 m_LightDirection = new Vector3(0f, 0f, 1f);
        private double m_LightRadians = 0d;

        private void mParseKeyboard(Input.KeyboardHandler nKeyboard)
        {
            if (nKeyboard.IsKeyDown(Keys.Q))
            {
                this.TakeScreenshot = true;
            }

            if (InWorld)
            {
                if (nKeyboard.IsKeyDown(Keys.I))
                    m_LightRadians += .01f;
                if (nKeyboard.IsKeyDown(Keys.K))
                    m_LightRadians -= .01f;
                if (nKeyboard.IsKeyDown(Keys.O))
                    m_LightDirection.Z += .001f;
                if (nKeyboard.IsKeyDown(Keys.L))
                    m_LightDirection.Z -= .001f;

                m_LightDirection.X = (float)Math.Sin(m_LightRadians);
                m_LightDirection.Y = (float)Math.Cos(m_LightRadians);

                m_TileEngineService.SetLightDirection(m_LightDirection);
                #region KeyboardMovement
                /*
                GameObjects.Movement iMovement = m_GameObjectsService.GetObject(m_GameObjectsService.MyGUID).Movement;
                if (iMovement.IsMoving == false)
                {
                    if (nKeyboard.IsKeyDown(Keys.W))
                    {
                        if (nKeyboard.IsKeyDown(Keys.A))
                        {
                            iMovement.SetGoalTile(
                                iMovement.TileX - 1,
                                iMovement.TileY, 0);
                        }
                        else if (nKeyboard.IsKeyDown(Keys.D))
                        {
                            iMovement.SetGoalTile(
                                iMovement.TileX,
                                iMovement.TileY - 1, 0);
                        }
                        else
                        {
                            iMovement.SetGoalTile(
                                iMovement.TileX - 1,
                                iMovement.TileY - 1, 0);
                        }
                    }
                    else if (nKeyboard.IsKeyDown(Keys.S))
                    {
                        if (nKeyboard.IsKeyDown(Keys.A))
                        {
                            iMovement.SetGoalTile(
                                iMovement.TileX,
                                iMovement.TileY + 1, 0);
                        }
                        else if (nKeyboard.IsKeyDown(Keys.D))
                        {
                            iMovement.SetGoalTile(
                                iMovement.TileX + 1,
                                iMovement.TileY, 0);
                        }
                        else
                        {
                            iMovement.SetGoalTile(
                                iMovement.TileX + 1,
                                iMovement.TileY + 1, 0);
                        }
                    }
                    else
                    {
                        if (nKeyboard.IsKeyDown(Keys.A))
                        {
                            iMovement.SetGoalTile(
                                iMovement.TileX - 1,
                                iMovement.TileY + 1, 0);

                        }
                        if (nKeyboard.IsKeyDown(Keys.D))
                        {
                            iMovement.SetGoalTile(
                                iMovement.TileX + 1,
                                iMovement.TileY - 1, 0);
                        }
                    }
                }
                */
                #endregion
            }
        }

        // Poplicola 5/9/2009
        private float FPS; private float m_frames = 0; private float m_elapsedSeconds = 0;
        private bool mUpdateFPS(GameTime gameTime)
        {
            m_frames++;
            m_elapsedSeconds += (float)gameTime.ElapsedRealTime.TotalSeconds;
            if (m_elapsedSeconds >= 1)
            {
                FPS = m_frames / m_elapsedSeconds;
                m_elapsedSeconds -= 1;
                m_frames = 0;
                return true;
            }
            return false;
        }

        private string m_DebugMessage()
        {
            String iDebug = "FPS: " + FPS.ToString() + Environment.NewLine;
            iDebug += "Objects on screen: " + m_TileEngineService.ObjectsRendered.ToString() + Environment.NewLine;
            if (m_TileEngineService.MouseOverObject != null)
            {
                iDebug += "OBJECT: " + m_TileEngineService.MouseOverObject.ToString() + Environment.NewLine;
                if (m_TileEngineService.MouseOverObject.Type == TileEngine.MapObjectTypes.StaticTile)
                {
                    iDebug += "ArtID: " + ((TileEngine.StaticItem)m_TileEngineService.MouseOverObject).ID;
                }
                else if (m_TileEngineService.MouseOverObject.Type == TileEngine.MapObjectTypes.MobileTile)
                {
                    iDebug += 
                        "AnimID: " + ((TileEngine.MobileTile)m_TileEngineService.MouseOverObject).ID + Environment.NewLine +
                        "GUID: " + m_TileEngineService.MouseOverObject.OwnerGUID;
                }
                else if (m_TileEngineService.MouseOverObject.Type == TileEngine.MapObjectTypes.GameObjectTile)
                {
                    iDebug +=
                        "ArtID: " + ((TileEngine.GameObjectTile)m_TileEngineService.MouseOverObject).ID + Environment.NewLine +
                        "GUID: " + m_TileEngineService.MouseOverObject.OwnerGUID;
                }
                iDebug += " Z: " + m_TileEngineService.MouseOverObject.Z;
            }
            else
            {
                iDebug += "OVER: " + "null";
            }
            if (m_TileEngineService.MouseOverGroundTile != null)
            {
                iDebug += Environment.NewLine + "GROUND: " + m_TileEngineService.MouseOverGroundTile.Position.ToString();
            }
            else
            {
                iDebug += Environment.NewLine + "GROUND: null";
            }

            // iDebug += Environment.NewLine + m_GameObjectsService.GetObject(m_GameObjectsService.MyGUID).Movement.MoveSequence.ToString();
            // iDebug += Environment.NewLine + m_LightDirection.ToString();
            return iDebug;
        }
    }
}
