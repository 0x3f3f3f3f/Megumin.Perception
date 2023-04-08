using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Megumin.GameFramework.Sensor
{
    /// <summary>
    /// Mask这里设置了，HearingSensor，SightSensor就不用设置了
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class AISensor<T> : Sensor
        where T : class
    {
        [ProtectedInInspector]
        public HearingSensor HearingSensor;
        [ProtectedInInspector]
        public SightSensor SightSensor;

        public void Awake()
        {
            FindComponent();
        }

        private void Reset()
        {
            FindComponent();
        }

        void FindComponent()
        {
            if (!HearingSensor)
            {
                HearingSensor = GetComponentInChildren<HearingSensor>();
            }

            if (!SightSensor)
            {
                SightSensor = GetComponentInChildren<SightSensor>();
            }
        }

        [ReadOnlyInInspector]
        public List<T> InSensor = new List<T>();

        HashSet<Collider> inSensorColliders = new();
        HashSet<T> tempInSensor = new();
        private void Update()
        {
            if (Time.time < nextCheckStamp)
            {
                return;
            }
            nextCheckStamp = Time.time + checkDelta;

            inSensorColliders.Clear();
            if (SightSensor)
            {
                SightSensor.TryPhysicsTest(inSensorColliders, Filter);
            }

            if (HearingSensor)
            {
                HearingSensor.TryPhysicsTest(inSensorColliders, Filter);
            }

            tempInSensor.Clear();
            foreach (Collider c in inSensorColliders)
            {
                var tV = c.GetComponentInParent<T>();
                if (tV != null)
                {
                    tempInSensor.Add(tV);
                }
            }

            foreach (var item in InSensor)
            {
                if (tempInSensor.Contains(item))
                {

                }
                else
                {
                    //失去感知
                    OnLostTarget(item);
                }
            }

            foreach (var item in tempInSensor)
            {
                if (InSensor.Contains(item))
                {

                }
                else
                {
                    //新感知
                    OnFindTarget(item);
                }
            }

            InSensor.Clear();
            InSensor.AddRange(tempInSensor);
        }

        [ReadOnlyInInspector]
        public T AutoTarget;

        public virtual void OnFindTarget(T target)
        {
            //Debug.Log($"感知模块 发现新目标");
            if (AutoTarget == null)
            {
                AutoTarget = target;
            }
        }

        public virtual void OnLostTarget(T target)
        {
            //Debug.Log($"感知模块 失去目标");
            if (target == AutoTarget)
            {
                AutoTarget = InSensor.FirstOrDefault();
            }
        }

        [Header("Debug")]
        [Space]
        public bool DebugSolid = false;
        public Color DebugColor = Color.red;
        [Range(0, 10)]
        public float DebugLineThickness = 2;
    }
}


#if UNITY_EDITOR

namespace Megumin.GameFramework.Sensor
{
    using UnityEditor;
    partial class AISensor<T>
    {
        public void OnDrawGizmosSelected()
        {
            if (AutoTarget is MonoBehaviour behaviour)
            {
                Handles.color = DebugColor;
                Handles.DrawLine(transform.position, behaviour.transform.position, DebugLineThickness);
            }
        }
    }
}

#endif
