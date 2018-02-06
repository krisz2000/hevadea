﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Maker.Rise.Components
{
    public class InputManager : GameComponent
    {
        private KeyboardState oldKeyState;
        private KeyboardState newKeyState;

        private MouseState _oldMouseState;
        private MouseState _mouseState;

        public MouseState GetMouseState()
        {
            return _mouseState;
        }

        public MouseState GetOldMouseState()
        {
            return _oldMouseState;
        }

        public InputManager(InternalGame game) : base(game)
        {
        }

        public Point MousePosition => _mouseState.Position;

        public bool MouseLeft => _mouseState.LeftButton == ButtonState.Pressed;
        public bool MouseRight => _mouseState.RightButton == ButtonState.Pressed;
        public bool MouseMiddle => _mouseState.MiddleButton == ButtonState.Pressed;

        public bool MouseLeftClick => _mouseState.LeftButton == ButtonState.Released
                                      && _oldMouseState.LeftButton == ButtonState.Pressed;

        public bool MouseRightClick => _mouseState.RightButton == ButtonState.Released
                                       && _oldMouseState.RightButton == ButtonState.Pressed;

        public bool MouseMiddleClick => _mouseState.MiddleButton == ButtonState.Released
                                        && _oldMouseState.MiddleButton == ButtonState.Pressed;

        public bool MouseScrollDown => _mouseState.ScrollWheelValue < _oldMouseState.ScrollWheelValue;
        public bool MouseScrollUp => _mouseState.ScrollWheelValue > _oldMouseState.ScrollWheelValue;

        public bool KeyDown(Keys key)
        {
            return newKeyState.IsKeyDown(key);
        }

        public bool KeyPress(Keys key)
        {
            return newKeyState.IsKeyUp(key) && oldKeyState.IsKeyDown(key);
        }

        public override void Initialize()
        {
            oldKeyState = newKeyState = Keyboard.GetState();
            _oldMouseState = _mouseState = Mouse.GetState(Game.Window);
        }

        public override void Draw(GameTime gameTime)
        {
        }

        public override void Update(GameTime gameTime)
        {
            oldKeyState = newKeyState;
            _oldMouseState = _mouseState;
            newKeyState = Keyboard.GetState();
            _mouseState = Mouse.GetState(Game.Window);
        }
    }
}