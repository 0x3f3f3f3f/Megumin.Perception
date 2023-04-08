using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Megumin.GameFramework
{
    /// <summary>
    /// 被听觉感知对象可以设置自己的响声，用于潜行靠近功能。
    /// </summary>
    public interface IHearingSensorTarget
    {
        float SensorSound { get; }
    }
}


