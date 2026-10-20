using UnityEngine;
using UnityEngine.SceneManagement;

namespace GNight
{
    /// <summary>Puntos de control de la historia (inicio de cada tramo peligroso).</summary>
    public enum Stage { None = -1, Act1 = 0, Act2Wake = 1, Act2Cameras = 2, Act2Survival = 3, Act3Search = 4, Act3Final = 5 }
}
