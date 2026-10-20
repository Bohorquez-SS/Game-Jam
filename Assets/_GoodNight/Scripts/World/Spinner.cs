using System.Collections;
using UnityEngine;

namespace GNight
{
    public class Spinner : MonoBehaviour
    {
        public Vector3 axis = Vector3.right; public float speed = 900f; public bool on = true; float cur;
        void Update() { cur = Mathf.MoveTowards(cur, on ? speed : 0f, Time.deltaTime * speed * 0.6f); transform.Rotate(axis, cur * Time.deltaTime, Space.Self); }
    }
}
