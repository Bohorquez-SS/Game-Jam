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
        Quaternion doorClosed; float doorT; bool open;
        bool syncing;

        void Start()
        {
            if (door) doorClosed = door.localRotation;
            for (int i = 0; i < switches.Length; i++) if (switches[i]) switches[i].Toggled = l => Changed();
            if (fuseSlot) fuseSlot.Installed = s => { if (PowerPanel.I) PowerPanel.I.InstallFuse(); };
            if (PowerPanel.I) PowerPanel.I.Changed += Sync;
            Sync();
        }

        public override string Hint => Settings.T("[Clic] subir/bajar interruptores · [Esc] alejarse", "[Click] flip switches · [Esc] step back");
        public override void OnEnter() { open = true; if (AudioManager.I) AudioManager.I.At(AudioManager.I.handleRattle, transform.position, 0.6f); }
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
            if (fuseSlot) fuseSlot.SetInstalled(p.fuseOK);
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
            doorT = Mathf.MoveTowards(doorT, open ? 1f : 0f, Time.deltaTime * 3f);
            if (door) door.localRotation = doorClosed * Quaternion.Euler(0, doorAngle * Ease.InOut(doorT), 0);
            if (label06 != null && PowerPanel.I != null) label06.text = PowerPanel.I.c06Known ? "5 · 06 (2)" : "5 · ??? (2)";
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
