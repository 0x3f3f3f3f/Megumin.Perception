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
        /// <summary>
        /// 接近检测，实现检测夹角不应该过大，但是如果对象非常近，夹角很小的时候又看不见，就不是很合理
        /// 所有使用一个视线检测，距离很近，但夹角很大的额外检测
        /// 将不同Level放到Sight里去
        /// </summary>
        [ProtectedInInspector]
        public SightSensor NearSensor;

        [ProtectedInInspector]
        public List<T> Ignore = new List<T>();

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

            for (int i = Ignore.Count - 1; i >= 0; i--)
            {
                if (Ignore[i] == null)
                {
                    Ignore.RemoveAt(i);
                }
            }

            var self = GetComponentInParent<T>();
            if (self != null)
            {
                if (!Ignore.Contains(self))
                {
                    Ignore.Add(self);
                }
            }
        }

        [ReadOnlyInInspector]
        public List<T> SightTarget = new List<T>();
        [ReadOnlyInInspector]
        public List<T> HearingTarget = new List<T>();
        [ReadOnlyInInspector]
        public List<T> InSensor = new List<T>();
        List<T> OldInSensor = new List<T>();

        private void Update()
        {
            if (Time.time < nextCheckStamp)
            {
                return;
            }
            nextCheckStamp = Time.time + checkDelta;

            var mixR = Mathf.Max(HearingSensor.Radius, SightSensor.Level.Max(x=>x.Radius));
            var collidersInRadius = PhysicsTest(mixR);

            SightTarget.Clear();
            HearingTarget.Clear();

            ///交换引用
            (InSensor, OldInSensor) = (OldInSensor, InSensor);

            InSensor.Clear();
            foreach (var item in collidersInRadius)
            {
                var tarC = item.GetComponentInParent<T>();
                if (tarC == null)
                {
                    continue;
                }

                if (Ignore.Contains(tarC))
                {
                    continue;
                }

                if (tarC is MonoBehaviour behaviour)
                {
                    if (SightSensor.Check(behaviour, item))
                    {
                        //在视觉范围内
                        SightTarget.Add(tarC);
                        if (!InSensor.Contains(tarC))
                        {
                            InSensor.Add(tarC);
                        }
                    }

                    if (HearingSensor.Check(behaviour))
                    {
                        HearingTarget.Add(tarC);
                        if (!InSensor.Contains(tarC))
                        {
                            InSensor.Add(tarC);
                        }
                    }
                }
            }

            ///检测已经在感知范围内的，看看是不是离开感知范围
            foreach (var item in InSensor)
            {
                if (item is MonoBehaviour behaviour)
                {
                    if (SightSensor.Check(behaviour, null))
                    {
                        //在视觉范围内
                        SightTarget.Add(item);
                        if (!InSensor.Contains(item))
                        {
                            InSensor.Add(item);
                        }
                    }

                    if (HearingSensor.Check(behaviour))
                    {
                        HearingTarget.Add(item);
                        if (!InSensor.Contains(item))
                        {
                            InSensor.Add(item);
                        }
                    }
                }
            }

            foreach (var item in OldInSensor)
            {
                if (InSensor.Contains(item))
                {

                }
                else
                {
                    //失去感知
                    OnLostTarget(item);
                }
            }

            foreach (var item in InSensor)
            {
                if (OldInSensor.Contains(item))
                {

                }
                else
                {
                    //新感知
                    OnFindTarget(item);
                }
            }

            OldInSensor.Clear();
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
