using UnityEngine;

namespace GNight
{
    /// <summary>Ranura que acepta un objeto del inventario (fusible, cinta, llave, casete).</summary>
    public class SocketSlot : Clickable
    {
        public string itemId;
        public string missingEs = "Falta algo aquí", missingEn = "Something is missing here";
        public string installEs = "Colocar", installEn = "Insert";
        public GameObject installedVisual;
        public bool installed;
        public bool consume = true;
        public System.Action<SocketSlot> Installed;
        public override string Label => installed ? "" : (Inventory.Has(itemId) ? Settings.T(installEs, installEn) : Settings.T(missingEs, missingEn));
        void Start() { if (installedVisual) installedVisual.SetActive(installed); }
        public override void Click()
        {
            if (installed) return;
            if (!Inventory.Has(itemId))
            {
                if (AudioManager.I) AudioManager.I.UI(AudioManager.I.error, 0.4f);
                return;
            }
            installed = true;
            if (consume) Inventory.Remove(itemId);
            if (installedVisual) installedVisual.SetActive(true);
            if (AudioManager.I) AudioManager.I.UI(AudioManager.I.lockClunk, 0.8f);
            if (Installed != null) Installed(this);
            GameEvents.Raise("installed:" + itemId);
        }
        public void SetInstalled(bool v) { installed = v; if (installedVisual) installedVisual.SetActive(v); }
    }
}
