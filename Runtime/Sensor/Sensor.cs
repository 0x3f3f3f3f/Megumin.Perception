using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.ComponentModel;

namespace Megumin.GameFramework.Perception
{
    public class Sensor : MonoBehaviour
    {
        public GameObjectFilter Filter;


        public virtual bool TryPhysicsTest(HashSet<Collider> results,
                                           GameObjectFilter overrideFilter = null,
                                           int maxColliders = 10)
        {
            var filter = this.Filter;
            if (overrideFilter != null)
            {
                filter = overrideFilter;
            }

            return filter.TryPhysicsTest(transform.position, GetRadius(), results, CheckCollider);
        }

        public virtual float GetRadius()
        {
            return 0;
        }

        /// <summary>
        /// 用于子类重写
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        public virtual bool CheckCollider(Collider collider)
        {
            return true;
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
