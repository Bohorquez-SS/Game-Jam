using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNight
{
    [System.Serializable]
    public class DocumentData
    {
        public string id;
        public string titleEs, titleEn;
        [TextArea(3, 12)] public string bodyEs, bodyEn;
        public Texture2D image;
        public string Title => Settings.T(titleEs, titleEn);
        public string Body => Settings.T(bodyEs, bodyEn);
    }
}
