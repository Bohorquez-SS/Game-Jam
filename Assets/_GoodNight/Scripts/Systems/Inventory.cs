using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNight
{
    /// <summary>Inventario simple de objetos recogidos.</summary>
    public static class Inventory
    {
        public class Item { public string id, nameEs, nameEn; public Inspectable source; public string Name => Settings.T(nameEs, nameEn); }
        public static readonly List<Item> Items = new List<Item>();
        public static event System.Action Changed;
        public static void Clear() { Items.Clear(); }
        public static bool Has(string id) { foreach (var i in Items) if (i.id == id) return true; return false; }
        public static void Add(string id, string es, string en, Inspectable src)
        {
            if (Has(id)) return;
            Items.Add(new Item { id = id, nameEs = es, nameEn = en, source = src });
            if (Changed != null) Changed();
        }
        public static void Remove(string id)
        {
            Items.RemoveAll(i => i.id == id);
            if (Changed != null) Changed();
        }
    }
}
