using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNight
{
    /// <summary>Una cinta VHS: qué cámara muestra, qué "actores" del pasado aparecen y a qué hora fue grabada.</summary>
    [System.Serializable]
    public class TapeData
    {
        public string id;              // tape_a / tape_b / tape_c
        public string letter;          // etiqueta escrita: A, B, C
        public string stamp;           // hora grabada en la imagen
        public string dateLabel = "SÁB 23:59";
        public Camera cam;             // cámara del pasado (capa GN_Tape incluida)
        public GameObject actors;      // actores en capa GN_Tape (solo los ve esta cámara)
        public TapeActor actor;        // animación del doble durante la reproducción
        public string clueEs, clueEn;
        public float duration = 14f;
        public GameObject box;         // caja de la cinta sobre el mueble (para ordenarlas)
    }
}
