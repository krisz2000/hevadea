﻿using System;

namespace Hevadea.Framework.Utils
{
    public class EasingManager
    {
        public bool Show { get; set; } = false;
        public float Speed = 1f;
        private const float DebugSpeed = 1f;

        private float _value = 0f;
        private bool _ended;

        public AnimationEndedHandler AnimationEnded;

        public delegate void AnimationEndedHandler(EasingManager sender);

        public void Reset()
        {
            _value = 0f;
            _ended = false;
        }

        public float GetValueInv(EasingFunctions show, EasingFunctions hide) => (1f - GetValue(show, hide));

        public float GetValue(EasingFunctions show, EasingFunctions hide)
        {
            return GetValue(Show ? show : hide);
        }

        public float GetValue(EasingFunctions function)
        {
            return Easing.Interpolate(_value, function);
        }

        public float GetValueInv(EasingFunctions function) => (1f - GetValue(function));

        public void Update(double deltaTime)
        {
            if (Show)
            {
                _value = Math.Min(1f, _value + (float)deltaTime * Speed * DebugSpeed);
            }
            else
            {
                _value = Math.Max(0f, _value - (float)deltaTime * Speed * DebugSpeed);
            }

            if (_value == 1f && !_ended)
            {
                _ended = true;
                AnimationEnded?.Invoke(this);
            }
        }
    }
}