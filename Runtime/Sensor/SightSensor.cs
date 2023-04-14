using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Megumin.GameFramework.Perception
{
    [Serializable]
    public class SensorLevel
    {
        public string Name = "Custom";
        [Range(0, 100)]
        public float Radius = 15;

        [Header("Debug")]
        [Space]
        public bool DebugSolid = true;
        [Range(0, 10)]
        public float DebugLineThickness = 2;
    }

    [Serializable]
    public class SightLevel : SensorLevel
    {
        public Color DebugColor = new(0.38f, 0.85f, 0.25f, 0.07f);

        [Header("Config")]
        [Range(0, 360)]
        public float HorizontalAngle = 120;
        [Range(0, 360)]
        public float VerticalAngle = 160;
    }

    public partial class SightSensor : Sensor
    {
        [ReadOnlyInInspector]
        public string Type = SensorType.Sight;

        public List<SightLevel> Level = new() { new SightLevel() };

        public override float GetRadius()
        {
            var maxR = Level.Max(x => x.Radius);
            return maxR;
        }

        public override bool CheckCollider(Collider collider)
        {
            if (!enabled)
            {
                return false;
            }

            return CheckInLevel(collider);
        }

        public bool CheckInLevel(Collider collider)
        {
            foreach (var level in Level)
            {
                if (CheckLevel(collider, level))
                {
                    return true;
                }
            }

            return false;
        }

        bool CheckLevel(Collider collider, SightLevel level)
        {
            //一个对象可能有多个碰撞盒，每个碰撞盒都要检测
            var dis = Vector3.Distance(collider.transform.position, transform.position);
            if (dis < level.Radius)
            {
                var dir = collider.transform.position - transform.position;
                var hAngle = Vector3.SignedAngle(dir, transform.forward, transform.up);
                hAngle = Mathf.Abs(hAngle);
                if (hAngle > level.HorizontalAngle / 2)
                {
                    return false;
                }

                var vAngle = Vector3.SignedAngle(dir, transform.forward, transform.right);
                vAngle = Mathf.Abs(vAngle);
                if (vAngle > level.VerticalAngle / 2)
                {
                    return false;
                }

                return true;
            }
            return false;
        }
    }
}


#if UNITY_EDITOR

namespace Megumin.GameFramework.Perception
{
    using UnityEditor;
    partial class SightSensor
    {
        private void OnDrawGizmosSelected()
        {
            if (!enabled || !GlobalDebugShow)
            {
                return;
            }

            foreach (var level in Level)
            {
                DrawSightLevel(level);
            }
        }

        private void DrawSightLevel(SightLevel level)
        {
            DrawArc(transform.up, transform.forward, level.HorizontalAngle, level.Radius, level);
            DrawArc(transform.right, transform.forward, level.VerticalAngle, level.Radius, level);
        }

        public void DrawArc(Vector3 axis, Vector3 forward, float angle, float radius, SightLevel level)
        {
            Handles.color = level.DebugColor;
            if (level.DebugSolid)
            {
                Handles.DrawSolidArc(transform.position, axis,
                                forward,
                                angle / 2, radius);
                Handles.DrawSolidArc(transform.position, axis * -1,
                    forward,
                    angle / 2, radius);
            }

            var wireColor = Handles.color;
            wireColor.a = 1;
            Handles.color = wireColor;

            var leftDir = Quaternion.AngleAxis(-angle / 2, axis);
            var left = leftDir * forward * radius;
            Handles.DrawLine(transform.position, transform.position + left, level.DebugLineThickness);

            var rightDir = Quaternion.AngleAxis(angle / 2, axis);
            var right = rightDir * forward * radius;
            Handles.DrawLine(transform.position, transform.position + right, level.DebugLineThickness);

            Handles.DrawWireArc(transform.position, axis,
                forward,
                angle / 2, radius, level.DebugLineThickness);
            Handles.DrawWireArc(transform.position, axis * -1,
                forward,
                angle / 2, radius, level.DebugLineThickness);
        }
    }
}

#endif


