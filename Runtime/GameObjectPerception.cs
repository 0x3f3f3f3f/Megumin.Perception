using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Megumin.Perception
{
    public class GameObjectPerception : Perception<GameObject>
    {
        protected override void ColloctTempInSensor()
        {
            tempInSensor.Clear();
            foreach (Collider c in inSensorColliders)
            {
                if (Filter.HasValue && Filter.Value.Check(c) == false)
                {
                    //过滤
                    continue;
                }

                if (ExcludeSelf && c.gameObject == gameObject)
                {
                    //过滤
                    continue;
                }

                tempInSensor.Add(c.gameObject);
            }
        }

        public override void OnFindTarget(GameObject target)
        {
            //Debug.Log($"感知模块 发现新目标");
            if (!AutoTarget && target)
            {
                AutoTarget = target;
            }
            //Debug.Log($"find {target}");
        }
    }
}
