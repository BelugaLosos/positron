using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace Positron.Client.Room
{
    public sealed class NetworkClock : IDisposable
    {
        private CancellationTokenSource _ctx;

        private readonly double _timeOffset;
        private readonly double _tickDuration;

        private double _recentServerTime = 0d;
        private double _clientTime = 0d;
        private double _timeScale = 1d;

        private bool _isInitialized;
        private bool _running;

        public double ClientTimeWithPastOffset => _clientTime;
        public double ClearClientTime => _clientTime + _timeOffset;

        private const double DRIFT_COEF = 0.2d;
        private const double MIN_TIME_SCALE = 0.9d;
        private const double MAX_TIME_SCALE = 1.1d;
        private const double TIME_TO_HARD_RESET = 1d;

        public NetworkClock(double offset, int tickrate)
        {
            _timeOffset = Math.Abs(offset) / ((double)tickrate);
            _tickDuration = 1d / (double)tickrate;
            _ctx = new();
        }

        public void Reset()
        {
            Dispose();
            _isInitialized = false;
        }

        public void Dispose()
        {
            if (_running == false)
            {
                return;
            }

            StopTime();
        }

        public void TryInitTime(uint currentTick)
        {
            if (_isInitialized)
            {
                return;
            }

            UpdateServerTime(currentTick);
            UpdateClientTimeToServer();

            _ctx = new();
            _running = true;
            UpdateLoopHook().Forget();

            Debug.Log("Network clock stopped");

            _isInitialized = true;
        }

        public void StopTime()
        {
            _ctx.Cancel();
            _running = false;
        }

        public void UpdateServerTime(uint tick)
        {
            _recentServerTime = TickToSeconds(tick);
        }

        public double TickToSeconds(uint tick) => ((double)tick) * _tickDuration;

        private async UniTask UpdateLoopHook()
        {
            Debug.Log("Network clock started");

            while (_running)
            {
                double delta = (double)Time.unscaledDeltaTime;

                _clientTime += delta * _timeScale;

                double diff = _recentServerTime - ClearClientTime;

                if (Math.Abs(diff) > TIME_TO_HARD_RESET)
                {
                    UpdateClientTimeToServer();
                    _timeScale = 1d;
                }
                else
                {
                    double targetScale = 1.0d + (diff * DRIFT_COEF);
                    _timeScale = MoveTowards(_timeScale, targetScale, delta * 0.5d);
                }

                _timeScale = Math.Clamp(_timeScale, MIN_TIME_SCALE, MAX_TIME_SCALE);
                
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: _ctx.Token);
            }
        }

        private void UpdateClientTimeToServer()
        {
            _clientTime = _recentServerTime - _timeOffset;
        }

        private double MoveTowards(double current, double target, double maxDelta)
        {
            if (Math.Abs(target - current) <= maxDelta) return target;
            return current + Math.Sign(target - current) * maxDelta;
        }
    }
}