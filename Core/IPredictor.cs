using System.Collections.Generic;
using UnityEngine;

namespace PofyTools
{
    public interface IPredictor
    {
        Vector3 GetPredictedOffset(float inTime);
        void Stamp();
        Transform Target { get; }
    }

    public struct SimplePredictionData : IPredictor
    {
        public Transform Target { get; private set; }
        private Vector3 _cachedPosition;
        private float _timestamp;

        public SimplePredictionData(Transform target)
        {
            this.Target = target;
            this._cachedPosition = target.position;
            this._timestamp = Time.time;
        }

        /// <summary>
        /// Predicts target offset for given time based on state's snapshots.
        /// </summary>
        /// <param name="inTime"> Time offset for predition.</param>
        /// <returns> Target offset prediction for Time.time + inTime.</returns>
        public Vector3 GetPredictedOffset(float inTime = 0)
        {
            if (!this.Target || inTime <= 0)
                return default;

            Vector3 result;
            Vector3 velocity;

            Vector3 deltaPosition = this.Target.position - this._cachedPosition;
            float elapsedTime = Time.time - this._timestamp;

            if (elapsedTime > 0f)
                velocity = deltaPosition / elapsedTime;
            else
                velocity = deltaPosition;

            result = (velocity * inTime);

            return result;
        }

        public void Stamp()
        {
            if (this.Target)
            {
                this._cachedPosition = this.Target.position;
                this._timestamp = Time.time;
            }
        }
    }

    public class MultiPointPositionPredictor : IPredictor
    {
        public struct StampData
        {
            public Vector3 position;
            public float timestamp;

            public StampData(Vector3 position, float timestamp)
            {
                this.position = position;
                this.timestamp = timestamp;
            }
        }

        private Transform _target;
        public Transform Target => this._target;
        public void SetTarget(Transform target)
        {
            if (this._target != target)
                this._target = target;
            {
                this._stamps.Clear();
                Stamp();
            }
        }

        public int pointLimit;

        private List<StampData> _stamps;

        private Vector3 _lastResult;

        public MultiPointPositionPredictor(Transform target, int pointLimit = 1)
        {
            this.pointLimit = pointLimit + 1;
            this._stamps = new List<StampData>();

            SetTarget(target);
        }

        /// <summary>
        /// Predicts target offset for given time based on state's snapshots.
        /// </summary>
        /// <param name="inTime"> Time offset for predition.</param>
        /// <returns> Target offset prediction for Time.time + inTime.</returns>
        public Vector3 GetPredictedOffset(float inTime = 0)
        {
            int count = this._stamps.Count;
            if (!this.Target || inTime <= 0 || count < 1)
                return default;

            Stamp();

            Vector3 final = Vector3.zero;

            Vector3 velocity;
            Vector3 deltaPosition;
            float elapsedTime;

            int elements = 0;

            for (int i = 1; i < count; i++)
            {
                for (int j = i - 1; j >= 0; --j)
                {
                    deltaPosition = this._stamps[i].position - this._stamps[j].position;
                    elapsedTime = this._stamps[i].timestamp - this._stamps[j].timestamp;

                    if (elapsedTime > 0f)
                        velocity = deltaPosition / elapsedTime;
                    else
                        velocity = deltaPosition;


                    final += (velocity * inTime);
                    elements++;

                }
            }

            return this._lastResult = final / elements;
        }

        public void Stamp()
        {
            if (this.Target)
            {
                if (this._stamps.Count >= this.pointLimit)
                    this._stamps.RemoveAt(0);

                this._stamps.Add(new StampData(this.Target.position, Time.time));
            }
        }

        public void Draw()
        {
#if UNITY_EDITOR
            if (this.Target)
            {
                Gizmos.color = new Color(1, 0.5f, 1, 1);
                foreach (var stamp in this._stamps)
                {
                    Gizmos.DrawWireSphere(stamp.position, 0.1f);
                }

                Gizmos.color = new Color(0.5f, 1f, 0.5f, 1f);
                Gizmos.DrawWireSphere(this.Target.position + this._lastResult, 0.1f);
            }
#endif
        }

    }
}
