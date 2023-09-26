using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Megumin.Perception
{
    /// <summary>
    /// 感知组件
    /// 与<see cref="Sensor"/>一起使用
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class Perception<T> : MonoBehaviour
        where T : class
    {
        public Enable<GameObjectFilter> Filter;

        /// <summary>
        /// 感知忽略自身对象
        /// </summary>
        public bool ExcludeSelf = true;

        [ProtectedInInspector]
        public List<Sensor> InChildrenSensors;

        public void Awake()
        {
            var sensor2 = gameObject.GetComponentsInChildren<Sensor>(true);
            foreach (var sensor in sensor2)
            {
                if (!InChildrenSensors.Contains(sensor))
                {
                    InChildrenSensors.Add(sensor);
                }
            }
        }

        private void Reset()
        {
            if (InChildrenSensors != null)
            {
                InChildrenSensors.Clear();
                gameObject.GetComponentsInChildren(true, InChildrenSensors);
            }
        }

        [ReadOnlyInInspector]
        public List<T> InSensor = new();

        /// <summary>
        /// 更新间隔
        /// </summary>
        [Space]
        [Range(0, 5)]
        public float checkDelta = 0.5f;
        protected float nextCheckStamp;

        protected HashSet<Collider> inSensorColliders { get; } = new();
        protected HashSet<T> tempInSensor { get; } = new();

        private void Update()
        {
            if (Time.time < nextCheckStamp)
            {
                return;
            }
            nextCheckStamp = Time.time + checkDelta;
            CheckSensor();
        }

        protected virtual void CheckSensor()
        {
            inSensorColliders.Clear();

            if (InChildrenSensors != null)
            {
                foreach (var item in InChildrenSensors)
                {
                    if (item != null)
                    {
                        item.TryGetInSensor(inSensorColliders);
                    }
                }
            }

            ColloctTempInSensor();

            foreach (var item in InSensor)
            {
                if (tempInSensor.Contains(item))
                {

                }
                else
                {
                    //失去感知
                    OnLostTarget(item);
                    LostTarget?.Invoke(item);
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
                    FindTarget?.Invoke(item);
                }
            }

            InSensor.Clear();
            InSensor.AddRange(tempInSensor);
        }

        protected virtual void ColloctTempInSensor()
        {
            tempInSensor.Clear();

            foreach (Collider c in inSensorColliders)
            {
                var tV = c.GetComponentInParent<T>();
                if (tV is Component component && component)
                {
                    if (Filter.HasValue && Filter.Value.Check(component) == false)
                    {
                        //过滤
                        continue;
                    }

                    if (ExcludeSelf && component.gameObject == gameObject)
                    {
                        //过滤
                        continue;
                    }

                    tempInSensor.Add(tV);
                }
            }
        }

        [ReadOnlyInInspector]
        public T AutoTarget;

        public UnityEngine.Events.UnityEvent<T> FindTarget;
        public UnityEngine.Events.UnityEvent<T> LostTarget;

        public virtual void OnFindTarget(T target)
        {
            //TODO，根据Sensor类型区分。

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
                AutoTarget = default;
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

namespace Megumin.Perception
{
    using UnityEditor;
    partial class Perception<T>
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
