namespace GNight
{
    /// <summary>Puntos de control (inicio de cada tramo). El orden importa: ApplyWorld es acumulativo.</summary>
    public enum Stage
    {
        None = -1,
        C1_Noche = 0,        // 23:47 una noche normal
        C2_Despertar = 1,    // 00:18 cerrojo y voz falsa
        C2_Camaras = 2,      // cámaras, fusible, energía
        C2_Persecucion = 3,  // primer encuentro
        C3_Cintas = 4,       // mensajes imposibles, cintas VHS
        C3_Persecucion = 5,  // segunda persecución
        C4_Protocolo = 6,    // llave, armario, habitación 06, grabación
        C5_Final = 7         // aislamiento final y epílogo
    }
}
