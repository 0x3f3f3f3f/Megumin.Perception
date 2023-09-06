using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Megumin.Perception
{
    public class GameObjectPerception : Perception<GameObject>
    {
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
