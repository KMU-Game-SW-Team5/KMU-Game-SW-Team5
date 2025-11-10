using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 스킬들의 여러가지 운동을 포괄하는 추상 클래스
public abstract class Motion : ScriptableObject
{
    // 현재 운동 방식에서 1/60초 후의 속도 벡터를 리턴함.
    abstract public Vector3 GetNextVelocity(Transform target, Vector3 velocity, Vector3 acceleration);
}
