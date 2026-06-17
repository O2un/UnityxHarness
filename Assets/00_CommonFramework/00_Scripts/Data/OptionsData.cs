using System;
using UnityEngine;

namespace O2un.Data 
{
    [Serializable]
    public sealed class OptionsData
    {
        public float MusicVolume = 1f;
        public float SfxVolume   = 1f;
        public string Language   = "ko";
    }
}