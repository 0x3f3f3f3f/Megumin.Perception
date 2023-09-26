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
                if (ExcludeSelf)
                {
                    //忽略自身
                    if (c.gameObject != gameObject)
                    {
                        tempInSensor.Add(c.gameObject);
                    }
                }
                else
                {
                    tempInSensor.Add(c.gameObject);
                }
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
