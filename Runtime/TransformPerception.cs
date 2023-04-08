using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Megumin.GameFramework.Perception
{
    public class TransformPerception : Perception<Transform>
    {
        public override void OnFindTarget(Transform target)
        {
            //Debug.Log($"感知模块 发现新目标");
            if (!AutoTarget && target)
            {
                AutoTarget = target;
            }
            Debug.Log($"find {target}");
        }

        public override void OnLostTarget(Transform target)
        {
            base.OnLostTarget(target);
        }
    }
}
