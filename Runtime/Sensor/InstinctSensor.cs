using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Megumin.GameFramework.Perception
{
    /// <summary>
    /// 被警觉感知对象可以设置自己的响声，用于潜行靠近功能。
    /// </summary>
    public interface IInstinctSensorTarget
    {
        float SensorSound { get; }
    }

    /// <summary>
    /// 直觉，直感，本能
    /// </summary>
    public partial class InstinctSensor : Sensor
    {
        [ReadOnlyInInspector]
        public string Type = SensorType.Instinct;

        [Range(0, 30)]
        public float Radius = 7.5f;

        /// <summary>
        /// 每秒增加目标警觉值
        /// </summary>
        public float AddValueInRange = 0.1f;

        /// <summary>
        ///每秒减少目标警觉值
        /// </summary>
        public float RemoveValueOutRange = 0.1f;

        /// <summary>
        /// 触发被警觉的阈值
        /// </summary>
        public float TriggerValue = 1;

        /// <summary>
        /// 最大累计警觉值，这个值是为了脱离范围能很快消退被感知
        /// </summary>
        public float MaxSumValue = 2;

        static HashSet<Collider> inSensorColliders = new();
        static List<Component> list = new();

        /// <summary>
        /// 更新间隔
        /// </summary>
        [Space]
        [Range(0, 5)]
        public float checkDelta = 0.5f;
        protected float nextCheckStamp;

        public void Update()
        {
            if (Time.time < nextCheckStamp)
            {
                return;
            }
            nextCheckStamp = Time.time + checkDelta;

            inSensorColliders.Clear();
            Filter.TryPhysicsTest(transform.position, GetRadius(), inSensorColliders);
            foreach (var item in inSensorColliders)
            {
                Check(item);
            }

            list.Clear();
            list.AddRange(instinctdelta.Keys);

            foreach (var item in list)
            {
                var v = instinctdelta[item];

                var dis = Vector3.Distance(transform.position, item.transform.position);
                if (dis < Radius)
                {
                    if (item is IInstinctSensorTarget instinctSensorTarget)
                    {
                        v += instinctSensorTarget.SensorSound * checkDelta;
                    }
                    else
                    {
                        //每次在范围内就增加警觉值
                        v += AddValueInRange * checkDelta;
                    }

                    v = Mathf.Min(v, MaxSumValue);
                }
                else
                {
                    v -= RemoveValueOutRange * checkDelta;
                }

                instinctdelta[item] = v;

                if (v < 0)
                {
                    instinctdelta.Remove(item);
                }
            }

            //防止内存泄露
            list.Clear();
        }

        Dictionary<Component, float> instinctdelta = new Dictionary<Component, float>();
        public bool Check(Component target)
        {
            var current = 0f;
            if (instinctdelta.TryGetValue(target, out var delta))
            {
                current = delta;
            }
            else
            {
                instinctdelta[target] = current;
            }

            //在视警觉围内
            return current >= TriggerValue;
        }

        public override bool TryGetInSensor(HashSet<Collider> results, GameObjectFilter overrideFilter = null, int maxColliders = 10)
        {
            if (enabled == false)
            {
                return false;
            }

            foreach (var item in instinctdelta)
            {
                if (Check(item.Key))
                {
                    if (overrideFilter != null)
                    {
                        if (overrideFilter.Check(item.Key))
                        {
                            results.Add(item.Key as Collider);
                        }
                    }
                    else
                    {
                        results.Add(item.Key as Collider);
                    }
                }
            }

            return true;
        }

        [Header("Debug")]
        [Space]
        public bool DebugSolid = true;
        public Color DebugColor = new(1f, 0.25f, 0, 0.45f);
        [Range(0, 20)]
        public float DebugLineThickness = 2f;
    }
}


#if UNITY_EDITOR

namespace Megumin.GameFramework.Perception
{
    using UnityEditor;
    partial class InstinctSensor
    {
        private void OnDrawGizmosSelected()
        {
            if (!enabled || !GlobalDebugShow)
            {
                return;
            }

            //绘制警觉半径
            Gizmos.color = DebugColor;
            if (DebugSolid)
            {
                Gizmos.DrawSphere(transform.position, Radius);
            }

            var wireColor = Gizmos.color;
            wireColor.a = 1;
            Gizmos.color = wireColor;
            Gizmos.DrawWireSphere(transform.position, Radius);

            Handles.color = wireColor;
            foreach (var item in instinctdelta)
            {
                Handles.Label(item.Key.transform.position + Vector3.up, item.Value.ToString(),
                    GUI.skin.textField);
            }
        }
    }
}

#endif

