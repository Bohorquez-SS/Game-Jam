using UnityEngine;

namespace GNight
{
    /// <summary>
    /// Cuadro eléctrico físico: tapa que se abre, 5 interruptores 3D (A, C, L, I, 06), zócalo del fusible F2,
    /// display de consumo y nota técnica del padre. Superar 7 unidades = salta el diferencial.
    /// </summary>
    public class BreakerStation : FocusStation
    {
        public Transform door;
        public float doorAngle = -110f;
        public Lever[] switches = new Lever[5];
        public SocketSlot fuseSlot;
        public TextMesh display;
        public TextMesh label06;
        public GameObject burntFuse;      // fusible F2 quemado (visible mientras fuseOK == false)
        public TextMesh fuseLabel;
        bool fuseNoticeShown;
        Quaternion doorClosed; float doorT; bool open;
        bool syncing;

        void Start()
        {
            if (door) doorClosed = door.localRotation;
            for (int i = 0; i < switches.Length; i++) if (switches[i]) switches[i].Toggled = l => Changed();
            if (fuseSlot) fuseSlot.Installed = s =>
            {
                if (PowerPanel.I) PowerPanel.I.InstallFuse();
                CameraFX.Shake(0.05f);
                if (HUDView.I) HUDView.I.Hint(Settings.T("Fusible F2 cambiado. Ahora reparte la carga (máximo 7).", "Fuse F2 replaced. Now balance the load (max 7)."), 4f);
            };
            if (PowerPanel.I) PowerPanel.I.Changed += Sync;
            Sync();
        }

        bool NeedsFuse => PowerPanel.I != null && !PowerPanel.I.fuseOK;
        bool HasSpare => Inventory.Has("fuse");

        public override string Hint => NeedsFuse && HasSpare
            ? Settings.T("[Clic] en el fusible quemado F2 (parpadea) para cambiarlo · [Esc] alejarse", "[Click] the blown F2 fuse (blinking) to replace it · [Esc] step back")
            : Settings.T("[Clic] subir/bajar interruptores · [Esc] alejarse", "[Click] flip switches · [Esc] step back");
        public override string Instructions => NeedsFuse
            ? (HasSpare
                ? Settings.T("<b>CAMBIAR EL FUSIBLE</b>\nTienes el <b>fusible de repuesto</b>.\nHaz <b>clic en el fusible quemado F2</b> (abajo a la derecha, parpadea en amarillo) para sacarlo y poner el nuevo.\n\nDespués: la carga total no puede pasar de 7.",
                             "<b>REPLACE THE FUSE</b>\nYou have the <b>spare fuse</b>.\n<b>Click the blown F2 fuse</b> (bottom right, blinking yellow) to swap it for the new one.\n\nThen: the total load can't go over 7.")
                : Settings.T("<b>CUADRO ELÉCTRICO</b>\n<color=#ff8866>El fusible <b>F2</b> (cámaras) está <b>quemado</b>.</color> Necesito encontrar un repuesto: papá dejó una nota al lado.\n\nCada interruptor consume energía. La <b>carga total no puede pasar de 7</b>.",
                             "<b>BREAKER PANEL</b>\n<color=#ff8866>Fuse <b>F2</b> (cameras) is <b>blown</b>.</color> I need to find a spare: dad left a note next to it.\n\nEach switch uses power. The <b>total load can't go over 7</b>."))
            : Settings.T(
            "<b>CUADRO ELÉCTRICO</b>\nCada interruptor consume energía. La <b>carga total no puede pasar de 7</b>: si la superas, salta el diferencial y se apaga todo.\nSube los sistemas que necesites y baja los demás. Mira el medidor de carga.\nSi un zócalo de fusible está vacío, ese circuito no funciona: busca un fusible.",
            "<b>BREAKER PANEL</b>\nEach switch uses power. The <b>total load can't go over 7</b>: if it does, the breaker trips and everything goes off.\nTurn on what you need and turn off the rest. Watch the load meter.\nIf a fuse socket is empty that circuit won't work: find a fuse.");
        public override void OnEnter()
        {
            open = true; SyncFuse();
            if (AudioManager.I) AudioManager.I.At(AudioManager.I.handleRattle, transform.position, 0.6f);
            if (NeedsFuse && !HasSpare && !fuseNoticeShown && HUDView.I)
            { fuseNoticeShown = true; HUDView.I.Say(Settings.T("Tú", "You"), Settings.T("Uno de los fusibles está quemado. Necesito encontrar un repuesto.", "One of the fuses is blown. I need to find a spare."), 4f); }
        }

        /// <summary>El zócalo refleja SIEMPRE el estado real: quemado (sin repuesto no se puede cambiar) o instalado.</summary>
        void SyncFuse()
        {
            var p = PowerPanel.I; if (p == null || fuseSlot == null) return;
            if (fuseSlot.installed != p.fuseOK) fuseSlot.SetInstalled(p.fuseOK);
            if (fuseSlot.installedVisual) fuseSlot.installedVisual.SetActive(p.fuseOK);
            if (burntFuse) burntFuse.SetActive(!p.fuseOK);
            fuseSlot.missingEs = "F2 · fusible QUEMADO (necesitas un repuesto)"; fuseSlot.missingEn = "F2 · BLOWN fuse (you need a spare)";
            fuseSlot.installEs = "Cambiar el fusible quemado por el repuesto"; fuseSlot.installEn = "Swap the blown fuse for the spare";
        }
        public override void OnExit() { open = false; }

        void Sync()
        {
            var p = PowerPanel.I; if (p == null) return;
            syncing = true;
            for (int i = 0; i < switches.Length; i++)
                if (switches[i])
                {
                    switches[i].Set(p.state[i] && !p.Tripped, i == 1 && !p.fuseOK);
                    switches[i].interactable = i < 4 || p.c06Known;
                }
            SyncFuse();
            syncing = false;
        }

        void Changed()
        {
            if (syncing) return;
            var p = PowerPanel.I; if (p == null) return;
            var s = new bool[5];
            for (int i = 0; i < 5; i++) s[i] = switches[i] != null && switches[i].on;
            if (!p.Apply(s))
            {
                CameraFX.Shake(0.25f);
                if (HUDView.I) HUDView.I.Flash(new Color(1f, 0.9f, 0.6f, 0.5f), 0.25f);
                Sync();
            }
            else if (AudioManager.I) AudioManager.I.UI(AudioManager.I.lockClunk, 0.5f);
        }

        void Update()
        {
            SyncFuse();
            // resaltar el fusible quemado solo cuando el jugador YA tiene el repuesto
            if (fuseLabel != null)
            {
                bool cue = NeedsFuse && HasSpare && Mathf.FloorToInt(Time.time * 3f) % 2 == 0;
                fuseLabel.text = NeedsFuse ? (HasSpare ? Settings.T("▼ F2 · CAMBIAR ▼", "▼ F2 · REPLACE ▼") : Settings.T("F2 · QUEMADO", "F2 · BLOWN")) : "F2 · OK";
                fuseLabel.color = NeedsFuse ? (cue ? new Color(1f, 0.95f, 0.2f) : new Color(1f, 0.4f, 0.25f)) : new Color(0.4f, 1f, 0.5f);
                if (burntFuse) burntFuse.transform.localScale = Vector3.one * (cue ? 1.15f : 1f);
            }
            doorT = Mathf.MoveTowards(doorT, open ? 1f : 0f, Time.deltaTime * 3f);
            if (door) door.localRotation = doorClosed * Quaternion.Euler(0, doorAngle * Ease.InOut(doorT), 0);
            if (label06 != null && PowerPanel.I != null) label06.text = PowerPanel.I.c06Known ? "CIRCUITO 06\n2 u." : "???\n2 u.";
            if (display == null || PowerPanel.I == null) return;
            var p = PowerPanel.I;
            int load = 0; for (int i = 0; i < 5; i++) if (p.state[i] && !p.Tripped) load += p.cost[i];
            string bar = ""; for (int i = 0; i < p.capacity; i++) bar += i < load ? "█" : "·";
            display.text = p.Tripped ? Settings.T("DIFERENCIAL\nDISPARADO", "BREAKER\nTRIPPED")
                                     : Settings.T("CARGA ", "LOAD ") + load + "/" + p.capacity + "\n" + bar + (p.fuseOK ? "" : "\nF2 " + Settings.T("FUNDIDO", "BLOWN"));
            display.color = p.Tripped || !p.fuseOK ? new Color(1f, 0.35f, 0.2f) : (load == p.capacity ? new Color(1f, 0.85f, 0.4f) : new Color(0.4f, 1f, 0.5f));
        }
    }
}
