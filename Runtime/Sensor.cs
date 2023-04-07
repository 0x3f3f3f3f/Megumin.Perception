using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.ComponentModel;

namespace Megumin.GameFramework.Sensor
{
    public class Sensor : MonoBehaviour
    {
        /// <summary>
        /// 探测器是不是自己进行物理检测,可能由高级复合感知模块统一进行物理测试
        /// </summary>
        public bool PhysicsTestRadiusSelf = false;
        public GameObjectFilter Filter;

        public virtual List<Collider> PhysicsTest(float maxR,GameObjectFilter overrideFilter = null)
        {
            var filter = this.Filter;
            if (overrideFilter != null)
            {
                filter = overrideFilter;
            }

            Collider[] res = Array.Empty<Collider>();
            if (filter.LayerMask.Enabled)
            {
                res = Physics.OverlapSphere(transform.position, maxR, filter.LayerMask.Value);
            }
            else
            {
                res = Physics.OverlapSphere(transform.position, maxR);
            }

            List<Collider> colliders = new List<Collider>();
            foreach (var item in res)
            {
                if (filter.CheckTag(item.gameObject))
                {
                    colliders.Add(item);
                }
            }

            return colliders;
        }

        /// <summary>
        /// 更新间隔
        /// </summary>
        [Space]
        [Range(0, 5)]
        public float checkDelta = 0.5f;
        protected float nextCheckStamp;


        static bool globalDebugshow = true;
        /// <summary>
        /// 全局显示开关
        /// </summary>
        public static bool GlobalDebugShow
        {
            get
            {
                //if (globalDebugshow == null)
                //{
                //    globalDebugshow = new Pref<bool>(nameof(Sensor), true);
                //}
                return globalDebugshow;
            }
        }

        [Editor]
        public void SwitchGlobalToggle()
        {
            globalDebugshow = !GlobalDebugShow;
        }
    }

    public class SensorType
    {
        public const string Sight = nameof(Sight);
        public const string Hearing = nameof(Hearing);
    }
}
