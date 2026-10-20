using System.Collections;
using UnityEngine;

namespace GNight
{
    public class TextureScroll : MonoBehaviour
    {
        public Vector2 speed = new Vector2(0f, 0.5f); Material mat; Vector2 off;
        void Start() { mat = GetComponent<Renderer>().material; }
        void Update() { off += speed * Time.deltaTime; off.x %= 1f; off.y %= 1f; mat.mainTextureOffset = off; }
    }
}
