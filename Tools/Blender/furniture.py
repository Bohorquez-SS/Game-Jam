"""GOOD NIGHT — furniture & props. Run: python3 furniture.py <out_dir>
Each model: front faces -Y, origin at floor center. Movable parts are separate named objects
(Door_L, Drawer_0, Lid, Btn_5...) with their own pivots so Unity can animate them.
"""
import sys, math, bpy
from mathutils import Vector
sys.path.insert(0, __import__('os').path.dirname(__file__))
from gnlib import *

OUTD = sys.argv[-1]
MODELS = {}

def model(fn):
    MODELS[fn.__name__] = fn
    return fn

def set_origin(ob, point):
    cur = bpy.context.scene.cursor.location.copy()
    bpy.context.scene.cursor.location = Vector(point)
    for o in bpy.context.selected_objects: o.select_set(False)
    ob.select_set(True); bpy.context.view_layer.objects.active = ob
    bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
    bpy.context.scene.cursor.location = cur

def part(objs, name, origin=None, uv=1.0, smooth=False):
    ob = finish(objs, name, uv, smooth)
    if origin is not None: set_origin(ob, origin)
    return ob

# ------------------------------------------------------------------ BEDROOM
@model
def bed():
    L, W = 2.0, 1.0
    p = []
    # frame
    p.append(boxmm("rail_l", (-W/2, -L/2, 0.18), (-W/2 + 0.05, L/2, 0.34), "WoodMid", 0.012))
    p.append(boxmm("rail_r", (W/2 - 0.05, -L/2, 0.18), (W/2, L/2, 0.34), "WoodMid", 0.012))
    p.append(boxmm("rail_f", (-W/2, -L/2, 0.18), (W/2, -L/2 + 0.05, 0.34), "WoodMid", 0.012))
    for x in (-W/2 + 0.03, W/2 - 0.03):
        for y in (-L/2 + 0.03, L/2 - 0.03):
            p.append(box("leg", (0.07, 0.07, 0.2), (x, y, 0.1), "WoodMid", 0.01))
    p.append(boxmm("slats", (-W/2 + 0.05, -L/2 + 0.05, 0.27), (W/2 - 0.05, L/2, 0.30), "WoodLight", 0.0))
    # headboard with posts and slats
    for x in (-W/2 - 0.02, W/2 + 0.02):
        p.append(box("post", (0.08, 0.08, 1.0), (x, L/2, 0.5), "WoodMid", 0.015))
        p.append(sphere("knob", 0.05, (x, L/2, 1.03), "WoodMid", segs=10, rings=6))
    p.append(boxmm("hb_top", (-W/2, L/2 - 0.03, 0.88), (W/2, L/2 + 0.03, 0.96), "WoodMid", 0.01))
    p.append(boxmm("hb_mid", (-W/2, L/2 - 0.025, 0.42), (W/2, L/2 + 0.025, 0.48), "WoodMid", 0.01))
    for i in range(7):
        x = -W/2 + 0.12 + i * (W - 0.24) / 6
        p.append(box("hb_slat", (0.035, 0.03, 0.4), (x, L/2, 0.68), "WoodMid", 0.006))
    frame = part(p, "Frame")
    mattress = part([cushion("m", (W - 0.06, L - 0.06, 0.2), (0, 0, 0.4), "Sheet", 0.15)], "Mattress", smooth=True)
    pillow = part([cushion("p", (0.62, 0.36, 0.13), (0, L/2 - 0.3, 0.56), "Pillow", 0.6, rot=(math.radians(-8), 0, 0))], "Pillow", smooth=True)
    # rumpled blanket: subdivided plane with noise folds, draped over sides
    bm = bmesh.new()
    bmesh.ops.create_grid(bm, x_segments=18, y_segments=24, size=0.5)
    for v in bm.verts:
        x, y = v.co.x * 2 * (W / 2 + 0.06), v.co.y * 2 * 0.72 - 0.3
        z = 0.548 + 0.02 * math.sin(y * 9 + x * 3) + 0.012 * math.sin(x * 17)
        ax = abs(x) - (W / 2 - 0.02)
        if ax > 0: z -= ax * 2.8; x = math.copysign(W / 2 + 0.04 + ax * 0.15, x)
        if y < -L / 2 + 0.04: z -= (-L/2 + 0.04 - y) * 2.0
        v.co = Vector((x, y, z))
    bmesh.ops.solidify(bm, geom=bm.faces[:], thickness=0.02)
    blanket = mesh_from_bm(bm, "b", "Blanket")
    blanket = part([blanket], "Blanket", smooth=True)
    return [frame, mattress, pillow, blanket]

@model
def nightstand():
    p = [boxmm("body", (-0.22, -0.2, 0.06), (0.22, 0.2, 0.55), "WoodMid", 0.012),
         boxmm("top", (-0.24, -0.22, 0.55), (0.24, 0.22, 0.58), "WoodMid", 0.01)]
    for x in (-0.19, 0.19):
        for y in (-0.17, 0.17): p.append(box("leg", (0.04, 0.04, 0.06), (x, y, 0.03), "WoodMid", 0.005))
    body = part(p, "Body")
    d = part([boxmm("front", (-0.19, -0.215, 0.33), (0.19, -0.2, 0.5), "WoodLight", 0.008),
              sphere("knob", 0.018, (0, -0.225, 0.415), "Brass", segs=8, rings=6)], "Drawer_0", origin=(0, -0.2, 0.415))
    return [body, d]

@model
def desk():
    p = [boxmm("top", (-0.65, -0.32, 0.72), (0.65, 0.32, 0.76), "WoodLight", 0.01),
         boxmm("ped", (0.22, -0.3, 0.0), (0.62, 0.3, 0.72), "WoodLight", 0.01),
         boxmm("leg_l", (-0.62, -0.3, 0.0), (-0.58, 0.3, 0.72), "WoodLight", 0.008),
         boxmm("back", (-0.6, 0.27, 0.35), (0.22, 0.3, 0.72), "WoodLight", 0.005)]
    body = part(p, "Body")
    drs = []
    for i in range(3):
        z0 = 0.05 + i * 0.22
        drs.append(part([boxmm("f", (0.24, -0.315, z0), (0.6, -0.3, z0 + 0.2), "WoodMid", 0.008),
                         box("h", (0.12, 0.02, 0.018), (0.42, -0.325, z0 + 0.13), "Metal", 0.004)],
                        "Drawer_%d" % i, origin=(0.42, -0.3, z0 + 0.1)))
    return [body] + drs

@model
def chair():
    p = [boxmm("seat", (-0.21, -0.21, 0.43), (0.21, 0.21, 0.47), "WoodMid", 0.01)]
    for x in (-0.18, 0.18):
        for y in (-0.18, 0.18):
            p.append(box("leg", (0.035, 0.035, 0.43), (x, y, 0.215), "WoodMid", 0.006))
    for x in (-0.18, 0.18): p.append(box("post", (0.035, 0.035, 0.5), (x, 0.18, 0.72), "WoodMid", 0.006))
    for z in (0.62, 0.78, 0.92): p.append(boxmm("bar", (-0.18, 0.165, z), (0.18, 0.195, z + 0.05), "WoodMid", 0.006))
    return [part(p, "Chair")]

@model
def crt_monitor():
    p = [boxmm("front", (-0.21, -0.2, 0.0), (0.21, 0.02, 0.36), "PlasticBeige", 0.02, 2),
         boxmm("back", (-0.16, 0.02, 0.04), (0.16, 0.28, 0.32), "PlasticBeige", 0.03, 2),
         boxmm("bezel_in", (-0.17, -0.205, 0.05), (0.17, -0.19, 0.31), "PlasticDark", 0.005),
         box("led", (0.015, 0.006, 0.01), (0.17, -0.205, 0.025), "Red", 0)]
    body = part(p, "Body")
    scr = part([box("s", (0.32, 0.004, 0.24), (0, -0.208, 0.18), "Screen", 0)], "Screen", uv=0.32)
    return [body, scr]

@model
def keyboard():
    p = [boxmm("base", (-0.22, -0.08, 0.0), (0.22, 0.08, 0.025), "PlasticBeige", 0.006)]
    for r in range(4):
        for c in range(14):
            p.append(box("k", (0.022, 0.022, 0.012), (-0.2 + c * 0.0305, -0.055 + r * 0.033, 0.03), "PlasticBeige", 0.003))
    return [part(p, "Keyboard")]

@model
def pc_tower():
    p = [boxmm("b", (-0.09, -0.21, 0), (0.09, 0.21, 0.42), "PlasticBeige", 0.012),
         boxmm("bay", (-0.075, -0.215, 0.3), (0.075, -0.205, 0.36), "PlasticDark", 0.003),
         cyl("btn", 0.015, 0.01, (0, -0.212, 0.2), "PlasticDark", 10, rot=(math.radians(90), 0, 0))]
    return [part(p, "Tower")]

@model
def desk_fan():
    p = [cyl("base", 0.1, 0.03, (0, 0, 0.015), "PlasticDark", 16, bevel=0.008),
         cyl("neck", 0.015, 0.2, (0, 0, 0.13), "Metal", 8),
         cyl("motor", 0.05, 0.08, (0, 0.03, 0.25), "PlasticDark", 12, rot=(math.radians(90), 0, 0))]
    cage = []
    for i in range(10):
        a = i * math.pi / 10
        bm = bmesh.new()
        bmesh.ops.create_circle(bm, cap_ends=False, segments=20, radius=0.14)
        ob = mesh_from_bm(bm, "ring", "Metal")
        m = ob.modifiers.new("w", 'WIREFRAME'); m.thickness = 0.004
        ob.location = (0, -0.03, 0.25); ob.rotation_euler = (math.radians(90), a, 0)
        cage.append(ob)
    body = part(p + cage, "Body")
    bl = []
    for i in range(3):
        b = box("blade", (0.05, 0.004, 0.11), (0, 0, 0.065), "PlasticBeige", 0.002)
        b.rotation_euler = (0, i * 2 * math.pi / 3, 0)
        bl.append(b)
    blades = part(bl, "Blades")
    blades.location = (0, -0.035, 0.25)
    return [body, blades]

@model
def bookshelf_wall():
    p = [boxmm("board", (-0.55, -0.12, 0), (0.55, 0.12, 0.025), "WoodDark", 0.005)]
    for x in (-0.4, 0.4): p.append(boxmm("bracket", (x - 0.01, 0.05, -0.12), (x + 0.01, 0.12, 0.0), "Metal", 0.002))
    p += books_row(-0.5, 0.48, 0.025, 0.0)
    return [part(p, "Shelf")]

def books_row(x0, x1, z, y, seed=1, lean=True):
    import random
    rnd = random.Random(seed)
    mats = ["BookRed", "BookBlue", "BookGreen", "BookCream", "WoodLight"]
    out = []; x = x0
    while x < x1:
        w = rnd.uniform(0.025, 0.055); h = rnd.uniform(0.17, 0.26); d = rnd.uniform(0.14, 0.19)
        b = box("book", (w, d, h), (x + w / 2, y, z + h / 2), rnd.choice(mats), 0.003)
        if lean and rnd.random() < 0.08 and x + 0.1 < x1:
            b.rotation_euler = (0, math.radians(rnd.choice([-12, 12])), 0)
        out.append(b); x += w + 0.003
    return out

@model
def wardrobe_slatted():
    """Closet you can hide in. 1.1 wide x 0.55 deep x 2.0 high. Doors pivot at outer edges."""
    W, D, H = 1.1, 0.55, 2.0
    p = [boxmm("back", (-W/2, D/2 - 0.02, 0.05), (W/2, D/2, H), "WoodDark", 0.005),
         boxmm("side_l", (-W/2, -D/2, 0.05), (-W/2 + 0.025, D/2, H), "WoodDark", 0.006),
         boxmm("side_r", (W/2 - 0.025, -D/2, 0.05), (W/2, D/2, H), "WoodDark", 0.006),
         boxmm("top", (-W/2 - 0.02, -D/2 - 0.02, H), (W/2 + 0.02, D/2 + 0.01, H + 0.05), "WoodDark", 0.01),
         boxmm("base", (-W/2, -D/2, 0.0), (W/2, D/2, 0.08), "WoodDark", 0.008),
         cyl("rod", 0.012, W - 0.06, (0, 0.05, H - 0.2), "Metal", 8, rot=(0, math.radians(90), 0))]
    # hanging clothes
    import random; rnd = random.Random(4)
    for i in range(5):
        x = -0.35 + i * 0.17
        p.append(cushion("cloth", (0.06, 0.42, 0.75), (x, 0.05, H - 0.62), rnd.choice(["Hoodie", "Jeans", "Coat", "FabricGreen"]), 0.2))
    body = part(p, "Body")
    doors = []
    for side in (-1, 1):
        dw = W / 2 - 0.03
        x_in, x_out = (0.0 if side < 0 else 0.0), side * (W / 2 - 0.025)
        xa, xb = (x_out, x_out + -side * dw) if True else (0, 0)
        lo, hi = min(xa, xb), max(xa, xb)
        q = [boxmm("stile_o", (lo if side < 0 else hi - 0.05, -D/2 - 0.02, 0.09), (lo + 0.05 if side < 0 else hi, -D/2, H - 0.01), "WoodMid", 0.005),
             boxmm("stile_i", (hi - 0.05 if side < 0 else lo, -D/2 - 0.02, 0.09), (hi if side < 0 else lo + 0.05, -D/2, H - 0.01), "WoodMid", 0.005),
             boxmm("rail_b", (lo, -D/2 - 0.02, 0.09), (hi, -D/2, 0.2), "WoodMid", 0.005),
             boxmm("rail_t", (lo, -D/2 - 0.02, H - 0.12), (hi, -D/2, H - 0.01), "WoodMid", 0.005)]
        z = 0.24
        while z < H - 0.16:
            s = box("slat", (hi - lo - 0.1, 0.012, 0.07), ((lo + hi) / 2, -D/2 - 0.01, z), "WoodMid", 0.002)
            s.rotation_euler = (math.radians(-40), 0, 0); q.append(s); z += 0.075
        q.append(sphere("knob", 0.018, (hi - 0.07 if side < 0 else lo + 0.07, -D/2 - 0.035, 1.0), "Brass", segs=8, rings=6))
        doors.append(part(q, "Door_L" if side < 0 else "Door_R", origin=(x_out, -D/2 - 0.01, 0)))
    return [body] + doors

@model
def desk_lamp():
    p = [cyl("base", 0.08, 0.025, (0, 0, 0.012), "Metal", 16, bevel=0.006),
         cyl("stem", 0.012, 0.32, (0, 0, 0.18), "Brass", 8)]
    body = part(p, "Body")
    shade = part([cyl("shade", 0.15, 0.17, (0, 0, 0.4), "LampShade", 20, r2=0.09)], "Shade")
    bulb = part([sphere("bulb", 0.035, (0, 0, 0.36), "Bulb", segs=10, rings=6)], "Bulb")
    return [body, shade, bulb]

@model
def alarm_clock():
    p = [boxmm("b", (-0.075, -0.035, 0), (0.075, 0.035, 0.07), "PlasticDark", 0.012, 2)]
    return [part(p, "Clock")]

@model
def phone_on_table():
    return [part([boxmm("p", (-0.036, -0.075, 0), (0.036, 0.075, 0.009), "Phone", 0.004, 2)], "Phone")]

# ------------------------------------------------------------------ LIVING
@model
def sofa():
    W = 1.9
    p = [boxmm("base", (-W/2, -0.42, 0.08), (W/2, 0.42, 0.38), "Fabric", 0.04, 2),
         boxmm("back", (-W/2, 0.22, 0.38), (W/2, 0.42, 0.85), "Fabric", 0.06, 2),
         boxmm("arm_l", (-W/2, -0.42, 0.38), (-W/2 + 0.18, 0.42, 0.62), "Fabric", 0.06, 2),
         boxmm("arm_r", (W/2 - 0.18, -0.42, 0.38), (W/2, 0.42, 0.62), "Fabric", 0.06, 2)]
    for x in (-W/2 + 0.06, W/2 - 0.06):
        for y in (-0.36, 0.36): p.append(cyl("leg", 0.025, 0.08, (x, y, 0.04), "WoodDark", 8))
    for i in range(3):
        x = -W/2 + 0.18 + (i + 0.5) * (W - 0.36) / 3
        p.append(cushion("seat", ((W - 0.36) / 3 - 0.02, 0.62, 0.14), (x, -0.08, 0.45), "Fabric", 0.4))
        p.append(cushion("backc", ((W - 0.36) / 3 - 0.03, 0.16, 0.42), (x, 0.16, 0.66), "Fabric", 0.4, rot=(math.radians(-10), 0, 0)))
    p.append(cushion("throw", (0.38, 0.12, 0.36), (-0.6, 0.0, 0.62), "Blanket", 0.5, rot=(math.radians(-20), 0, math.radians(15))))
    return [part(p, "Sofa", smooth=False)]

@model
def armchair():
    p = [boxmm("base", (-0.45, -0.42, 0.08), (0.45, 0.42, 0.38), "FabricGreen", 0.04, 2),
         boxmm("back", (-0.45, 0.22, 0.38), (0.45, 0.42, 0.9), "FabricGreen", 0.06, 2),
         boxmm("arm_l", (-0.45, -0.42, 0.38), (-0.3, 0.42, 0.62), "FabricGreen", 0.05, 2),
         boxmm("arm_r", (0.3, -0.42, 0.38), (0.45, 0.42, 0.62), "FabricGreen", 0.05, 2),
         cushion("seat", (0.58, 0.6, 0.13), (0, -0.08, 0.45), "FabricGreen", 0.4)]
    for x in (-0.4, 0.4):
        for y in (-0.36, 0.36): p.append(cyl("leg", 0.025, 0.08, (x, y, 0.04), "WoodDark", 8))
    return [part(p, "Armchair")]

@model
def coffee_table():
    p = [boxmm("top", (-0.55, -0.3, 0.38), (0.55, 0.3, 0.42), "WoodMid", 0.012),
         boxmm("shelf", (-0.5, -0.25, 0.1), (0.5, 0.25, 0.12), "WoodMid", 0.006)]
    for x in (-0.5, 0.5):
        for y in (-0.25, 0.25): p.append(box("leg", (0.04, 0.04, 0.38), (x, y, 0.19), "WoodMid", 0.006))
    p += [box("mag", (0.22, 0.3, 0.008), (0.2, 0.0, 0.425), "Paper", 0.001),
          cyl("mug", 0.04, 0.09, (-0.25, 0.05, 0.465), "Red", 12)]
    return [part(p, "Table")]

@model
def tv_stand():
    p = [boxmm("stand", (-0.6, -0.22, 0.0), (0.6, 0.22, 0.48), "WoodDark", 0.012),
         boxmm("tv", (-0.3, -0.25, 0.48), (0.3, 0.2, 0.95), "PlasticDark", 0.03, 2),
         boxmm("back", (-0.22, 0.2, 0.55), (0.22, 0.35, 0.9), "PlasticDark", 0.03),
         box("antenna_a", (0.006, 0.006, 0.35), (-0.05, 0.1, 1.1), "Chrome", 0, rot=(0, math.radians(-25), 0)),
         box("antenna_b", (0.006, 0.006, 0.35), (0.05, 0.1, 1.1), "Chrome", 0, rot=(0, math.radians(25), 0)),
         boxmm("vcr", (-0.22, -0.2, 0.32), (0.22, 0.12, 0.4), "PlasticDark", 0.006)]
    body = part(p, "Body")
    scr = part([box("s", (0.5, 0.004, 0.36), (0, -0.252, 0.72), "Screen", 0)], "Screen", uv=0.5)
    return [body, scr]

@model
def floor_lamp():
    p = [cyl("base", 0.16, 0.03, (0, 0, 0.015), "Metal", 16, bevel=0.008),
         cyl("pole", 0.015, 1.45, (0, 0, 0.74), "Brass", 8)]
    body = part(p, "Body")
    shade = part([cyl("shade", 0.22, 0.28, (0, 0, 1.55), "LampShade", 20, r2=0.15)], "Shade")
    bulb = part([sphere("bulb", 0.04, (0, 0, 1.5), "Bulb", segs=10, rings=6)], "Bulb")
    return [body, shade, bulb]

@model
def tall_bookcase():
    W, D, H = 1.0, 0.32, 1.9
    p = [boxmm("side_l", (-W/2, -D/2, 0), (-W/2 + 0.025, D/2, H), "WoodDark", 0.006),
         boxmm("side_r", (W/2 - 0.025, -D/2, 0), (W/2, D/2, H), "WoodDark", 0.006),
         boxmm("back", (-W/2, D/2 - 0.01, 0), (W/2, D/2, H), "WoodDark", 0.002)]
    for i, z in enumerate((0.0, 0.45, 0.9, 1.35, H - 0.025)):
        p.append(boxmm("sh", (-W/2, -D/2, z), (W/2, D/2, z + 0.025), "WoodDark", 0.004))
        if z < H - 0.1: p += books_row(-W/2 + 0.04, W/2 - 0.12 - (i % 2) * 0.25, z + 0.025, 0.0, seed=10 + i)
    p.append(box("box", (0.2, 0.2, 0.15), (W/2 - 0.15, 0, 0.9 + 0.1), "Cardboard", 0.005))
    return [part(p, "Bookcase")]

@model
def dresser():
    W, D, H = 1.0, 0.45, 0.85
    p = [boxmm("body", (-W/2, -D/2 + 0.02, 0.06), (W/2, D/2, H), "WoodDark", 0.01),
         boxmm("top", (-W/2 - 0.02, -D/2, H), (W/2 + 0.02, D/2 + 0.01, H + 0.03), "WoodDark", 0.01)]
    for x in (-W/2 + 0.04, W/2 - 0.04):
        for y in (-D/2 + 0.06, D/2 - 0.04): p.append(box("leg", (0.05, 0.05, 0.06), (x, y, 0.03), "WoodDark", 0.006))
    body = part(p, "Body")
    drs = []
    for i in range(3):
        z0 = 0.1 + i * 0.25
        drs.append(part([boxmm("f", (-W/2 + 0.03, -D/2 + 0.0, z0), (W/2 - 0.03, -D/2 + 0.02, z0 + 0.22), "WoodMid", 0.008),
                         box("h", (0.14, 0.02, 0.02), (0, -D/2 - 0.012, z0 + 0.11), "Brass", 0.004)],
                        "Drawer_%d" % i, origin=(0, -D/2 + 0.02, z0 + 0.11)))
    return [body] + drs

@model
def coat_rack():
    p = [cyl("base", 0.18, 0.03, (0, 0, 0.015), "WoodDark", 12),
         cyl("pole", 0.02, 1.75, (0, 0, 0.9), "WoodDark", 8)]
    for i in range(4):
        a = i * math.pi / 2
        p.append(box("hook", (0.012, 0.012, 0.16), (0.07 * math.cos(a), 0.07 * math.sin(a), 1.7), "WoodDark", 0, rot=(0, math.radians(35), a)))
    rack = part(p, "Rack")
    coat = part([cushion("coat", (0.45, 0.2, 0.95), (0, -0.12, 1.2), "Coat", 0.3),
                 cushion("sleeve", (0.12, 0.16, 0.6), (-0.2, -0.12, 1.15), "Coat", 0.3),
                 box("pocket", (0.14, 0.02, 0.12), (0.12, -0.225, 1.0), "Coat", 0.005)], "Coat", smooth=True)
    return [rack, coat]

@model
def wall_clock():
    p = [cyl("rim", 0.17, 0.04, (0, 0, 0), "WoodDark", 24, rot=(math.radians(90), 0, 0), bevel=0.008),
         cyl("face", 0.15, 0.01, (0, -0.022, 0), "Paper", 24, rot=(math.radians(90), 0, 0))]
    for i in range(12):
        a = i * math.pi / 6
        p.append(box("tick", (0.008, 0.004, 0.02), (0.13 * math.sin(a), -0.028, 0.13 * math.cos(a)), "PlasticDark", 0, rot=(0, -a, 0)))
    body = part(p, "Body")
    hh = part([box("h", (0.012, 0.004, 0.07), (0, -0.032, 0.03), "PlasticDark", 0)], "Hand_H", origin=(0, -0.032, 0))
    mh = part([box("m", (0.008, 0.004, 0.11), (0, -0.034, 0.05), "PlasticDark", 0)], "Hand_M", origin=(0, -0.034, 0))
    return [body, hh, mh]

@model
def plant():
    p = [cyl("pot", 0.13, 0.25, (0, 0, 0.125), "Ceramic", 14, r2=0.16, bevel=0.01),
         cyl("soil", 0.145, 0.01, (0, 0, 0.245), "Soil", 14)]
    import random; rnd = random.Random(2)
    for i in range(9):
        a = i * 2 * math.pi / 9 + rnd.uniform(-0.2, 0.2)
        l = box("leaf", (0.06, 0.008, rnd.uniform(0.35, 0.55)), (0.06 * math.cos(a), 0.06 * math.sin(a), 0.45), "Plant", 0)
        l.rotation_euler = (math.radians(rnd.uniform(15, 35)) * math.sin(a), -math.radians(rnd.uniform(15, 35)) * math.cos(a), a)
        p.append(l)
    return [part(p, "Plant")]

@model
def photo_frame_stand():
    p = [boxmm("frame", (-0.1, -0.012, 0.0), (0.1, 0.0, 0.15), "WoodDark", 0.004),
         box("leg", (0.02, 0.08, 0.12), (0, 0.035, 0.06), "WoodDark", 0, rot=(math.radians(-25), 0, 0))]
    body = part(p, "Frame")
    pic = part([box("pic", (0.17, 0.002, 0.12), (0, -0.013, 0.075), "Paper", 0)], "Picture", uv=0.17)
    return [body, pic]

@model
def photo_frame_wall():
    p = [boxmm("frame", (-0.22, -0.02, -0.17), (0.22, 0.0, 0.17), "WoodDark", 0.006)]
    body = part(p, "Frame")
    pic = part([box("pic", (0.38, 0.002, 0.28), (0, -0.021, 0), "Paper", 0)], "Picture", uv=0.38)
    return [body, pic]

@model
def security_desk():
    p = [boxmm("top", (-0.7, -0.3, 0.72), (0.7, 0.3, 0.76), "WoodMid", 0.01),
         boxmm("l", (-0.68, -0.28, 0), (-0.64, 0.28, 0.72), "WoodMid", 0.006),
         boxmm("r", (0.64, -0.28, 0), (0.68, 0.28, 0.72), "WoodMid", 0.006),
         boxmm("back", (-0.64, 0.25, 0.3), (0.64, 0.28, 0.72), "WoodMid", 0.004),
         boxmm("dvr", (-0.55, -0.15, 0.76), (-0.15, 0.2, 0.84), "PlasticDark", 0.006),
         box("dvr_led", (0.02, 0.005, 0.008), (-0.2, -0.153, 0.8), "Red", 0)]
    return [part(p, "Desk")]

@model
def camera_switcher():
    """Physical camera selector with 5 buttons."""
    body = part([boxmm("b", (-0.17, -0.06, 0), (0.17, 0.06, 0.05), "PlasticDark", 0.008)], "Body")
    btns = []
    for i in range(5):
        x = -0.12 + i * 0.06
        btns.append(part([boxmm("btn", (x - 0.02, -0.03, 0.05), (x + 0.02, 0.01, 0.07), "PlasticBeige", 0.004)], "Btn_%d" % i, origin=(x, -0.01, 0.06)))
    return [body] + btns

# ------------------------------------------------------------------ KITCHEN
@model
def kitchen_base():
    """Base cabinet run 2.4 m with 3 drawers + 3 doors + sink."""
    W, D, H = 2.4, 0.6, 0.88
    p = [boxmm("carc", (-W/2, -D/2 + 0.02, 0.1), (W/2, D/2, H), "WoodLight", 0.005),
         boxmm("kick", (-W/2 + 0.03, -D/2 + 0.07, 0), (W/2 - 0.03, D/2, 0.1), "PlasticDark", 0),
         boxmm("ctop", (-W/2 - 0.02, -D/2 - 0.02, H), (W/2 + 0.02, D/2, H + 0.04), "Counter", 0.006),
         boxmm("sink", (0.35, -0.2, H + 0.01), (0.85, 0.2, H + 0.045), "Chrome", 0.01),
         cyl("faucet", 0.015, 0.25, (0.6, 0.24, H + 0.16), "Chrome", 10),
         box("spout", (0.02, 0.18, 0.02), (0.6, 0.16, H + 0.28), "Chrome", 0.005)]
    body = part(p, "Body")
    parts = []
    xs = [-W/2 + 0.4 + i * 0.8 for i in range(3)]
    for i, x in enumerate(xs):
        parts.append(part([boxmm("f", (x - 0.38, -D/2, 0.68), (x + 0.38, -D/2 + 0.02, 0.85), "WoodMid", 0.006),
                           box("h", (0.16, 0.02, 0.02), (x, -D/2 - 0.012, 0.77), "Chrome", 0.004)], "Drawer_%d" % i, origin=(x, -D/2 + 0.02, 0.77)))
        parts.append(part([boxmm("d", (x - 0.38, -D/2, 0.12), (x + 0.38, -D/2 + 0.02, 0.65), "WoodMid", 0.006),
                           box("h", (0.02, 0.02, 0.12), (x + 0.3, -D/2 - 0.012, 0.52), "Chrome", 0.004)], "Door_%d" % i, origin=(x - 0.38, -D/2 + 0.01, 0.0)))
    return [body] + parts

@model
def kitchen_wall():
    W = 2.4
    p = [boxmm("carc", (-W/2, -0.17, 0), (W/2, 0.17, 0.7), "WoodLight", 0.006)]
    for i in range(4):
        x = -W/2 + 0.3 + i * 0.6
        p += [boxmm("d", (x - 0.29, -0.19, 0.02), (x + 0.29, -0.17, 0.68), "WoodMid", 0.005),
              box("h", (0.02, 0.02, 0.1), (x + (0.22 if i % 2 else -0.22), -0.2, 0.12), "Chrome", 0.004)]
    p.append(box("ledlight", (W - 0.1, 0.06, 0.012), (0, -0.05, -0.006), "Bulb", 0))
    return [part(p, "Cabinets")]

@model
def stove():
    p = [boxmm("b", (-0.3, -0.3, 0), (0.3, 0.3, 0.9), "Fridge", 0.01),
         boxmm("ov", (-0.25, -0.31, 0.15), (0.25, -0.3, 0.6), "PlasticDark", 0.005),
         box("oh", (0.4, 0.03, 0.025), (0, -0.33, 0.65), "Chrome", 0.006),
         boxmm("bp", (-0.3, 0.22, 0.9), (0.3, 0.3, 1.05), "Fridge", 0.008)]
    for x in (-0.15, 0.15):
        for y in (-0.12, 0.1): p.append(cyl("burner", 0.07, 0.015, (x, y, 0.91), "PlasticDark", 14))
    for i in range(4): p.append(cyl("knob", 0.02, 0.025, (-0.18 + i * 0.12, -0.31, 0.78), "PlasticDark", 10, rot=(math.radians(90), 0, 0)))
    return [part(p, "Stove")]

@model
def fridge():
    p = [boxmm("b", (-0.33, -0.3, 0), (0.33, 0.33, 1.8), "Fridge", 0.03, 2),
         boxmm("seam", (-0.33, -0.305, 1.18), (0.33, -0.3, 1.2), "Metal", 0),
         box("h1", (0.025, 0.03, 0.3), (0.26, -0.33, 1.42), "Chrome", 0.008),
         box("h2", (0.025, 0.03, 0.45), (0.26, -0.33, 0.85), "Chrome", 0.008)]
    import random; rnd = random.Random(7)
    for i in range(5):  # magnets & notes
        p.append(box("note", (rnd.uniform(0.07, 0.12), 0.002, rnd.uniform(0.07, 0.12)), (rnd.uniform(-0.22, 0.1), -0.303, rnd.uniform(1.25, 1.65)), "Paper", 0))
    return [part(p, "Fridge")]

@model
def kitchen_table():
    p = [boxmm("top", (-0.55, -0.4, 0.72), (0.55, 0.4, 0.76), "WoodLight", 0.012)]
    for x in (-0.5, 0.5):
        for y in (-0.35, 0.35): p.append(box("leg", (0.05, 0.05, 0.72), (x, y, 0.36), "WoodLight", 0.008))
    for i, (x, y) in enumerate([(-0.25, -0.15), (0.25, 0.15)]):
        p.append(cyl("plate", 0.12, 0.015, (x, y, 0.768), "Ceramic", 20, bevel=0.004))
    p += [cyl("cup", 0.04, 0.1, (0.3, -0.2, 0.81), "Ceramic", 12),
          box("napkin", (0.18, 0.18, 0.003), (-0.05, 0.2, 0.762), "Paper", 0),
          cyl("bottle", 0.035, 0.25, (-0.4, 0.25, 0.885), "Glass", 10)]
    return [part(p, "Table")]

@model
def toolbox():
    body = part([boxmm("b", (-0.22, -0.11, 0), (0.22, 0.11, 0.16), "Red", 0.01),
                 boxmm("tray", (-0.2, -0.09, 0.14), (0.2, 0.09, 0.16), "Metal", 0.002),
                 box("screwdriver", (0.22, 0.02, 0.02), (-0.05, -0.04, 0.17), "Brass", 0.005)], "Body")
    lid = part([boxmm("l", (-0.22, -0.11, 0.16), (0.22, 0.11, 0.2), "Red", 0.01),
                box("handle", (0.2, 0.025, 0.04), (0, 0, 0.225), "PlasticDark", 0.008),
                box("latch", (0.04, 0.012, 0.04), (0, -0.115, 0.16), "Chrome", 0.003)], "Lid", origin=(0, 0.11, 0.16))
    return [body, lid]

@model
def fuse():
    p = [cyl("glass", 0.012, 0.06, (0, 0, 0), "Glass", 10, rot=(0, math.radians(90), 0)),
         cyl("cap_a", 0.014, 0.012, (-0.032, 0, 0), "Brass", 10, rot=(0, math.radians(90), 0)),
         cyl("cap_b", 0.014, 0.012, (0.032, 0, 0), "Brass", 10, rot=(0, math.radians(90), 0))]
    return [part(p, "Fuse")]

@model
def breaker_box():
    """Wall breaker panel: door (pivot left), 5 switches, fuse socket."""
    body = part([boxmm("box", (-0.3, 0, -0.4), (0.3, 0.1, 0.4), "Metal", 0.01),
                 boxmm("in", (-0.27, -0.002, -0.37), (0.27, 0.0, 0.37), "PlasticDark", 0),
                 boxmm("socket", (0.08, -0.03, -0.3), (0.24, 0.0, -0.18), "PlasticBeige", 0.004)], "Body")
    door = part([boxmm("d", (-0.3, -0.025, -0.4), (0.3, -0.005, 0.4), "Metal", 0.008),
                 box("h", (0.03, 0.02, 0.08), (0.24, -0.035, 0), "Chrome", 0.005),
                 box("lbl", (0.25, 0.002, 0.08), (0, -0.026, 0.25), "Label", 0)], "Door", origin=(-0.3, -0.015, 0))
    sws = []
    for i in range(5):
        x = -0.22 + i * 0.11
        sws.append(part([boxmm("base", (x - 0.035, -0.02, 0.05), (x + 0.035, 0.0, 0.25), "PlasticDark" if i < 4 else "Red", 0.004),
                         box("lever", (0.03, 0.05, 0.05), (x, -0.045, 0.15), "PlasticBeige", 0.005)], "Switch_%d" % i, origin=(x, -0.02, 0.15)))
    return [body, door] + sws

@model
def pantry():
    """Tall pantry you can hide in (doors at front -Y)."""
    W, D, H = 0.7, 0.6, 2.05
    p = [boxmm("back", (-W/2, D/2 - 0.02, 0), (W/2, D/2, H), "WoodLight", 0.004),
         boxmm("l", (-W/2, -D/2, 0), (-W/2 + 0.02, D/2, H), "WoodLight", 0.004),
         boxmm("r", (W/2 - 0.02, -D/2, 0), (W/2, D/2, H), "WoodLight", 0.004),
         boxmm("t", (-W/2, -D/2, H - 0.02), (W/2, D/2, H), "WoodLight", 0.004),
         boxmm("shelf", (-W/2, 0.05, 1.6), (W/2, D/2, 1.62), "WoodLight", 0.003)]
    for i in range(4): p.append(box("can", (0.08, 0.08, 0.12), (-0.25 + i * 0.16, 0.18, 1.68), ["Red", "BookCream", "BookGreen", "Cardboard"][i], 0.005))
    body = part(p, "Body")
    q = [boxmm("st_l", (-W/2, -D/2 - 0.02, 0.02), (-W/2 + 0.05, -D/2, H - 0.02), "WoodMid", 0.004),
         boxmm("st_r", (W/2 - 0.05, -D/2 - 0.02, 0.02), (W/2, -D/2, H - 0.02), "WoodMid", 0.004),
         boxmm("rb", (-W/2, -D/2 - 0.02, 0.02), (W/2, -D/2, 0.12), "WoodMid", 0.004),
         boxmm("rt", (-W/2, -D/2 - 0.02, H - 0.12), (W/2, -D/2, H - 0.02), "WoodMid", 0.004)]
    z = 0.18
    while z < H - 0.16:
        s = box("slat", (W - 0.1, 0.012, 0.07), (0, -D/2 - 0.01, z), "WoodMid", 0.002); s.rotation_euler = (math.radians(-40), 0, 0); q.append(s); z += 0.075
    door = part(q, "Door", origin=(-W/2, -D/2 - 0.01, 0))
    return [body, door]

# ------------------------------------------------------------------ BATHROOM
@model
def toilet():
    p = [cyl("bowl", 0.2, 0.38, (0, -0.05, 0.19), "Ceramic", 18, r2=0.17, bevel=0.02),
         cyl("seat", 0.21, 0.03, (0, -0.07, 0.395), "Ceramic", 18, bevel=0.01),
         boxmm("tank", (-0.2, 0.12, 0.38), (0.2, 0.3, 0.8), "Ceramic", 0.025, 2),
         box("flush", (0.05, 0.02, 0.02), (0.13, 0.115, 0.74), "Chrome", 0.004)]
    return [part(p, "Toilet")]

@model
def bath_sink():
    p = [cyl("ped", 0.08, 0.75, (0, 0.05, 0.375), "Ceramic", 14, r2=0.06),
         boxmm("basin", (-0.28, -0.22, 0.75), (0.28, 0.22, 0.88), "Ceramic", 0.04, 2),
         cyl("tap", 0.012, 0.12, (0, 0.16, 0.94), "Chrome", 8),
         box("spout", (0.02, 0.09, 0.02), (0, 0.12, 0.99), "Chrome", 0.004)]
    return [part(p, "Sink")]

@model
def bathtub():
    p = [boxmm("outer", (-0.38, -0.85, 0), (0.38, 0.85, 0.55), "Ceramic", 0.05, 2),
         boxmm("inner", (-0.32, -0.79, 0.15), (0.32, 0.79, 0.56), "Screen", 0.03),  # dark water/shadow inside
         cyl("tap", 0.015, 0.2, (0, 0.82, 0.68), "Chrome", 8),
         cyl("rod", 0.012, 1.7, (-0.38, 0, 2.0), "Chrome", 8, rot=(math.radians(90), 0, 0))]
    body = part(p, "Tub")
    import random; rnd = random.Random(3)
    bm = bmesh.new()
    bmesh.ops.create_grid(bm, x_segments=20, y_segments=8, size=0.5)
    for v in bm.verts:
        y = v.co.x * 1.7; z = 2.0 - (v.co.y + 0.5) * 1.6
        x = -0.38 + 0.03 * math.sin(y * 18)
        v.co = Vector((x, y, z))
    cur = mesh_from_bm(bm, "curtain", "Towel")
    curtain = part([cur], "Curtain")
    return [body, curtain]

@model
def mirror_cabinet():
    body = part([boxmm("b", (-0.3, 0, -0.35), (0.3, 0.12, 0.35), "Paint", 0.01)], "Body")
    door = part([boxmm("f", (-0.3, -0.02, -0.35), (0.3, 0.0, 0.35), "Paint", 0.006)], "Door", origin=(-0.3, -0.01, 0))
    glass = part([box("g", (0.52, 0.004, 0.62), (0, -0.022, 0), "Glass", 0)], "Mirror", uv=0.52)
    glass.parent = door
    return [body, door, glass]

@model
def towel_rack():
    p = [cyl("bar", 0.012, 0.6, (0, 0, 0), "Chrome", 8, rot=(0, math.radians(90), 0)),
         box("b1", (0.02, 0.08, 0.02), (-0.3, 0.04, 0), "Chrome", 0.004), box("b2", (0.02, 0.08, 0.02), (0.3, 0.04, 0), "Chrome", 0.004),
         cushion("towel", (0.45, 0.04, 0.55), (0, -0.01, -0.25), "Towel", 0.2)]
    return [part(p, "Rack")]

# ------------------------------------------------------------------ HALL / DOORS / WINDOWS
@model
def interior_door():
    """Door leaf 0.88 x 2.03, pivot at hinge (left, x=0), extends to +X. Panelled."""
    W, H, T = 0.88, 2.03, 0.045
    p = [boxmm("slab", (0, -T/2, 0), (W, T/2, H), "Paint", 0.006)]
    for (z0, z1) in ((0.12, 0.95), (1.08, 1.9)):
        for s in (-1, 1):
            p.append(boxmm("panel", (0.1, s * T/2 - (0.006 if s > 0 else 0) , z0), (W - 0.1, s * T/2 + (0.006 if s < 0 else 0), z1), "Paint", 0.004))
    leaf = part(p, "Leaf")
    hw = []
    for s in (-1, 1):
        hw += [cyl("rose", 0.03, 0.012, (W - 0.08, s * (T/2 + 0.006), 1.0), "Brass", 12, rot=(math.radians(90), 0, 0)),
               box("lever", (0.12, 0.02, 0.02), (W - 0.12, s * (T/2 + 0.03), 1.0), "Brass", 0.006)]
    handle = part(hw, "Handle")
    return [leaf, handle]

@model
def door_frame():
    W, H = 0.9, 2.05
    p = []
    for s in (-1, 1):
        p += [boxmm("jl", (-W/2 - 0.07, s * 0.07 - 0.015, 0), (-W/2, s * 0.07 + 0.015, H + 0.07), "Paint", 0.004),
              boxmm("jr", (W/2, s * 0.07 - 0.015, 0), (W/2 + 0.07, s * 0.07 + 0.015, H + 0.07), "Paint", 0.004),
              boxmm("jt", (-W/2 - 0.07, s * 0.07 - 0.015, H), (W/2 + 0.07, s * 0.07 + 0.015, H + 0.07), "Paint", 0.004)]
    return [part(p, "Frame")]

@model
def front_door():
    W, H, T = 0.95, 2.1, 0.05
    p = [boxmm("slab", (0, -T/2, 0), (W, T/2, H), "WoodMid", 0.006)]
    for z0, z1 in ((0.15, 0.8), (0.9, 1.45)):
        for s in (-1, 1): p.append(boxmm("panel", (0.12, s * T/2 - 0.006 * (s > 0), z0), (W - 0.12, s * T/2 + 0.006 * (s < 0), z1), "WoodDark", 0.004))
    p.append(boxmm("win", (0.2, -T/2 - 0.003, 1.58), (W - 0.2, T/2 + 0.003, 1.92), "Glass", 0.004))
    p.append(boxmm("peep", (W/2 - 0.015, -T/2 - 0.01, 1.52), (W/2 + 0.015, T/2 + 0.01, 1.55), "Brass", 0.003))
    leaf = part(p, "Leaf")
    hw = []
    for s in (-1, 1): hw += [cyl("knob", 0.03, 0.05, (W - 0.09, s * 0.05, 1.0), "Brass", 12, rot=(math.radians(90), 0, 0))]
    hw.append(boxmm("lock", (W - 0.12, -0.04, 1.12), (W - 0.06, -0.025, 1.2), "Brass", 0.004))
    handle = part(hw, "Handle")
    bolt_base = part([boxmm("bb", (W - 0.2, -0.045, 1.28), (W - 0.06, -0.025, 1.33), "Chrome", 0.004)], "BoltBase")
    bolt_bar = part([boxmm("bar", (W - 0.18, -0.06, 1.295), (W - 0.06, -0.045, 1.315), "Chrome", 0.003),
                     box("knob", (0.015, 0.02, 0.03), (W - 0.12, -0.07, 1.305), "Chrome", 0.003)], "BoltBar")
    return [leaf, handle, bolt_base, bolt_bar]

@model
def metal_door():
    W, H, T = 0.9, 2.03, 0.06
    p = [boxmm("slab", (0, -T/2, 0), (W, T/2, H), "Metal", 0.008)]
    for z in (0.3, 1.0, 1.7): p.append(boxmm("rib", (0.05, -T/2 - 0.01, z), (W - 0.05, -T/2, z + 0.06), "Metal", 0.004))
    for (x, z) in ((0.06, 0.06), (W - 0.06, 0.06), (0.06, H - 0.06), (W - 0.06, H - 0.06)):
        p.append(sphere("rivet", 0.012, (x, -T/2 - 0.005, z), "Chrome", segs=6, rings=4))
    p.append(box("wheel", (0.2, 0.03, 0.2), (W - 0.2, -T/2 - 0.03, 1.0), "Red", 0.02))
    return [part(p, "Leaf")]

@model
def sash_window():
    """1.2 x 1.1 window, bottom at z=0. Frame static; Sash_Low moves up."""
    W, Hh = 1.2, 1.1
    h = Hh / 2
    p = [boxmm("sill", (-W/2 - 0.1, -0.2, -0.04), (W/2 + 0.1, 0.02, 0.0), "Paint", 0.006),
         boxmm("ml", (-W/2 - 0.08, -0.09, 0), (-W/2, -0.05, Hh + 0.08), "Paint", 0.004),
         boxmm("mr", (W/2, -0.09, 0), (W/2 + 0.08, -0.05, Hh + 0.08), "Paint", 0.004),
         boxmm("mt", (-W/2 - 0.08, -0.09, Hh), (W/2 + 0.08, -0.05, Hh + 0.08), "Paint", 0.004)]
    p += sash_parts(0, h + h / 2, 0.025, W, h)
    frame = part(p, "Frame")
    low = part(sash_parts(0, h / 2, -0.02, W, h), "Sash_Low")
    lock = part([boxmm("lk", (-0.04, -0.05, h - 0.02), (0.04, -0.02, h + 0.0), "Brass", 0.004)], "Latch")
    return [frame, low, lock]

def sash_parts(x, cz, y, W, h):
    out = [boxmm("rb", (-W/2, y - 0.02, cz - h/2), (W/2, y + 0.02, cz - h/2 + 0.05), "Paint", 0.004),
           boxmm("rt", (-W/2, y - 0.02, cz + h/2 - 0.05), (W/2, y + 0.02, cz + h/2), "Paint", 0.004),
           boxmm("sl", (-W/2, y - 0.02, cz - h/2), (-W/2 + 0.05, y + 0.02, cz + h/2), "Paint", 0.004),
           boxmm("sr", (W/2 - 0.05, y - 0.02, cz - h/2), (W/2, y + 0.02, cz + h/2), "Paint", 0.004),
           boxmm("mu", (-0.015, y - 0.015, cz - h/2), (0.015, y + 0.015, cz + h/2), "Paint", 0.003)]
    return out

@model
def alarm_panel():
    body = part([boxmm("b", (-0.15, 0, -0.21), (0.15, 0.04, 0.21), "PlasticBeige", 0.012, 2),
                 boxmm("scr", (-0.11, -0.002, 0.06), (0.11, 0.0, 0.16), "Screen", 0.003),
                 box("led_r", (0.012, 0.006, 0.012), (0.1, -0.003, 0.185), "Red", 0)], "Body")
    btns = []
    labels = "123456789*0#"
    for i in range(12):
        r, c = i // 3, i % 3
        x, z = -0.075 + c * 0.075, 0.0 - r * 0.048
        btns.append(part([boxmm("k", (x - 0.028, -0.018, z - 0.018), (x + 0.028, 0.0, z + 0.018), "PlasticDark", 0.005)], "Btn_%s" % ("s" if labels[i] == "*" else ("h" if labels[i] == "#" else labels[i])), origin=(x, -0.009, z)))
    return [body] + btns

@model
def hall_wardrobe():
    W, D, H = 1.15, 0.5, 2.1
    p = [boxmm("body", (-W/2, -D/2 + 0.02, 0.08), (W/2, D/2, H), "WoodDark", 0.008),
         boxmm("base", (-W/2 - 0.02, -D/2, 0), (W/2 + 0.02, D/2, 0.08), "WoodDark", 0.006),
         boxmm("crown", (-W/2 - 0.04, -D/2 - 0.03, H), (W/2 + 0.04, D/2, H + 0.08), "WoodDark", 0.015),
         boxmm("dl", (-W/2 + 0.02, -D/2, 0.12), (-0.01, -D/2 + 0.02, H - 0.05), "WoodMid", 0.006),
         boxmm("dr", (0.01, -D/2, 0.12), (W/2 - 0.02, -D/2 + 0.02, H - 0.05), "WoodMid", 0.006),
         sphere("k1", 0.02, (-0.06, -D/2 - 0.012, 1.05), "Brass", segs=8, rings=6),
         sphere("k2", 0.02, (0.06, -D/2 - 0.012, 1.05), "Brass", segs=8, rings=6),
         # scratches on the floor (hint it moves)
         box("scratch", (0.9, 0.003, 0.002), (-0.6, -D/2 - 0.05, 0.001), "WoodLight", 0)]
    # hidden keyhole on side
    p.append(boxmm("keyplate", (W/2, -0.05, 0.9), (W/2 + 0.005, 0.05, 1.05), "Brass", 0.003))
    return [part(p, "Wardrobe")]

@model
def side_table():
    p = [boxmm("top", (-0.3, -0.18, 0.75), (0.3, 0.18, 0.78), "WoodDark", 0.008)]
    for x in (-0.26, 0.26):
        for y in (-0.14, 0.14): p.append(box("leg", (0.035, 0.035, 0.75), (x, y, 0.375), "WoodDark", 0.006))
    p += [cyl("bowl", 0.08, 0.04, (-0.12, 0, 0.8), "Ceramic", 12, r2=0.06), box("keys_dish", (0.12, 0.08, 0.01), (0.15, 0, 0.785), "Brass", 0.002)]
    return [part(p, "Table")]

# ------------------------------------------------------------------ SECRET ROOM
@model
def reel_recorder():
    body = part([boxmm("b", (-0.2, -0.15, 0), (0.2, 0.15, 0.1), "PlasticDark", 0.01),
                 boxmm("panel", (-0.18, -0.13, 0.1), (0.18, 0.13, 0.105), "Metal", 0),
                 cyl("hub_a", 0.012, 0.03, (-0.09, 0.03, 0.12), "Chrome", 8),
                 cyl("hub_b", 0.012, 0.03, (0.09, 0.03, 0.12), "Chrome", 8)] +
                [box("key", (0.03, 0.025, 0.02), (-0.08 + i * 0.045, -0.12, 0.115), "PlasticBeige", 0.003) for i in range(4)], "Body")
    reels = []
    for i, x in enumerate((-0.09, 0.09)):
        reels.append(part([cyl("r", 0.075, 0.012, (x, 0.03, 0.12), "Tape", 20), cyl("rr", 0.077, 0.004, (x, 0.03, 0.128), "Chrome", 20)],
                          "Reel_%d" % i, origin=(x, 0.03, 0.12)))
    return [body] + reels

@model
def tape_reel():
    p = [cyl("r", 0.075, 0.012, (0, 0, 0.006), "Tape", 20), cyl("rr", 0.077, 0.004, (0, 0, 0.013), "Chrome", 20)]
    return [part(p, "Reel")]

@model
def vhs_tape():
    p = [boxmm("b", (-0.095, -0.0125, 0), (0.095, 0.0125, 0.188), "Tape", 0.003),
         box("label", (0.15, 0.002, 0.05), (0, -0.0135, 0.13), "Label", 0),
         box("win", (0.08, 0.002, 0.03), (0, -0.0135, 0.07), "Glass", 0)]
    return [part(p, "Tape")]

@model
def vcr_deck():
    body = part([boxmm("b", (-0.21, -0.17, 0), (0.21, 0.17, 0.09), "PlasticDark", 0.008),
                 boxmm("slot", (-0.1, -0.172, 0.05), (0.1, -0.168, 0.07), "Rubber", 0),
                 box("clk", (0.07, 0.003, 0.02), (0.13, -0.171, 0.035), "Screen", 0)], "Body")
    return [body]

@model
def cork_board():
    p = [boxmm("frame", (-0.55, -0.02, -0.38), (0.55, 0.0, 0.38), "WoodLight", 0.006),
         boxmm("cork", (-0.52, -0.022, -0.35), (0.52, -0.019, 0.35), "Cardboard", 0)]
    import random; rnd = random.Random(9)
    for i in range(9):
        x, z = rnd.uniform(-0.42, 0.42), rnd.uniform(-0.25, 0.25)
        p.append(box("note", (rnd.uniform(0.09, 0.16), 0.002, rnd.uniform(0.08, 0.14)), (x, -0.024, z), "Paper", 0, rot=(0, rnd.uniform(-0.15, 0.15), 0)))
        p.append(sphere("pin", 0.008, (x, -0.028, z + 0.05), "Red", segs=6, rings=4))
    # red string
    for i in range(4):
        a, b = Vector((rnd.uniform(-0.4, 0.4), -0.03, rnd.uniform(-0.25, 0.25))), Vector((rnd.uniform(-0.4, 0.4), -0.03, rnd.uniform(-0.25, 0.25)))
        d = b - a
        s = box("string", (d.length, 0.002, 0.003), ((a + b) / 2), "Red", 0)
        s.rotation_euler = (0, -math.atan2(d.z, d.x), 0)
        p.append(s)
    return [part(p, "Board")]

@model
def filing_cabinet():
    body = part([boxmm("b", (-0.24, -0.3, 0), (0.24, 0.3, 1.3), "Metal", 0.008)], "Body")
    drs = []
    for i in range(4):
        z0 = 0.04 + i * 0.315
        drs.append(part([boxmm("f", (-0.22, -0.31, z0), (0.22, -0.3, z0 + 0.29), "Metal", 0.006),
                         box("h", (0.12, 0.02, 0.025), (0, -0.322, z0 + 0.22), "Chrome", 0.004),
                         box("lbl", (0.07, 0.002, 0.04), (0, -0.311, z0 + 0.26), "Label", 0)], "Drawer_%d" % i, origin=(0, -0.3, z0 + 0.15)))
    return [body] + drs

@model
def generator_lever():
    body = part([boxmm("b", (-0.18, 0, -0.25), (0.18, 0.12, 0.25), "Metal", 0.01),
                 boxmm("slot", (-0.02, -0.002, -0.15), (0.02, 0.0, 0.15), "PlasticDark", 0),
                 box("warn", (0.2, 0.002, 0.06), (0, -0.002, 0.2), "Red", 0)], "Body")
    lever = part([box("arm", (0.03, 0.03, 0.22), (0, -0.04, 0.11), "Metal", 0.005),
                  cyl("grip", 0.025, 0.1, (0, -0.04, 0.24), "Red", 10, rot=(0, math.radians(90), 0))], "Lever", origin=(0, -0.02, 0))
    return [body, lever]

@model
def isolation_console():
    body = part([boxmm("b", (-0.5, -0.25, 0), (0.5, 0.25, 0.85), "Metal", 0.01),
                 boxmm("slope", (-0.5, -0.25, 0.85), (0.5, 0.05, 0.95), "Metal", 0.01),
                 boxmm("screen", (-0.32, -0.12, 0.951), (0.32, 0.05, 0.955), "Screen", 0)], "Body")
    btns = []
    for i, m in enumerate(["Red", "Red", "BookGreen"]):
        x = -0.3 + i * 0.3
        btns.append(part([cyl("b", 0.045, 0.04, (x, -0.2, 0.9), m, 14)], "Btn_%d" % i, origin=(x, -0.2, 0.88)))
    return [body] + btns

@model
def locker():
    W, D, H = 0.55, 0.55, 1.95
    p = [boxmm("back", (-W/2, D/2 - 0.02, 0), (W/2, D/2, H), "Metal", 0.004),
         boxmm("l", (-W/2, -D/2, 0), (-W/2 + 0.02, D/2, H), "Metal", 0.004),
         boxmm("r", (W/2 - 0.02, -D/2, 0), (W/2, D/2, H), "Metal", 0.004),
         boxmm("t", (-W/2, -D/2, H - 0.02), (W/2, D/2, H), "Metal", 0.004),
         boxmm("bo", (-W/2, -D/2, 0), (W/2, D/2, 0.04), "Metal", 0.004)]
    body = part(p, "Body")
    q = [boxmm("d", (-W/2, -D/2 - 0.02, 0.02), (W/2, -D/2, H - 0.02), "Metal", 0.004)]
    for i in range(6): q.append(boxmm("vent", (-0.15, -D/2 - 0.025, 1.45 + i * 0.05), (0.15, -D/2 - 0.019, 1.47 + i * 0.05), "PlasticDark", 0))
    q.append(box("h", (0.02, 0.03, 0.12), (W/2 - 0.07, -D/2 - 0.035, 1.0), "Chrome", 0.004))
    door = part(q, "Door", origin=(-W/2, -D/2 - 0.01, 0))
    return [body, door]

@model
def secret_desk():
    p = [boxmm("top", (-0.95, -0.3, 0.72), (0.95, 0.3, 0.76), "WoodDark", 0.01)]
    for x in (-0.9, 0.9):
        for y in (-0.25, 0.25): p.append(box("leg", (0.05, 0.05, 0.72), (x, y, 0.36), "Metal", 0.006))
    p += [boxmm("shelf", (-0.95, 0.0, 1.2), (0.95, 0.3, 1.24), "WoodDark", 0.006),
          box("sup_l", (0.03, 0.3, 0.48), (-0.9, 0.15, 0.98), "Metal", 0.004), box("sup_r", (0.03, 0.3, 0.48), (0.9, 0.15, 0.98), "Metal", 0.004)]
    import random; rnd = random.Random(5)
    for i in range(6):
        p.append(box("paper", (0.21, 0.29, 0.002), (rnd.uniform(-0.8, 0.8), rnd.uniform(-0.25, -0.05), 0.762 + i * 0.002), "Paper", 0, rot=(0, 0, rnd.uniform(-0.4, 0.4))))
    p.append(cyl("mug", 0.04, 0.1, (0.7, -0.15, 0.81), "Ceramic", 12))
    return [part(p, "Desk")]

@model
def cable_bundle():
    p = []
    import random; rnd = random.Random(12)
    for i in range(7):
        bm = bmesh.new()
        pts = [Vector((rnd.uniform(-0.1, 0.1), -1.0 + k * 0.25, 0.0 + abs(math.sin(k + i)) * 0.02)) for k in range(9)]
        verts = [bm.verts.new(pt) for pt in pts]
        for a, b in zip(verts, verts[1:]): bm.edges.new((a, b))
        ob = mesh_from_bm(bm, "cable", "Rubber" if i % 2 else "PlasticDark")
        sk = ob.modifiers.new("skin", 'SKIN')
        for v in ob.data.skin_vertices[0].data: v.radius = (0.008, 0.008)
        p.append(ob)
    return [part(p, "Cables")]

# ------------------------------------------------------------------ EXTERIOR
@model
def tree():
    import random; rnd = random.Random(21)
    p = [cyl("trunk", 0.16, 2.4, (0, 0, 1.2), "WoodDark", 8, r2=0.1)]
    for i in range(4):
        z = 2.0 + i * 0.8; r = 1.3 - i * 0.25
        bm = bmesh.new(); bmesh.ops.create_icosphere(bm, subdivisions=1, radius=r)
        for v in bm.verts: v.co += Vector((rnd.uniform(-0.15, 0.15), rnd.uniform(-0.15, 0.15), rnd.uniform(-0.15, 0.15))) * r
        ob = mesh_from_bm(bm, "canopy", "Plant"); ob.location = (rnd.uniform(-0.2, 0.2), rnd.uniform(-0.2, 0.2), z)
        p.append(ob)
    return [part(p, "Tree")]

@model
def fence_segment():
    p = [boxmm("rail_a", (-1.0, -0.02, 0.3), (1.0, 0.02, 0.38), "WoodMid", 0.005),
         boxmm("rail_b", (-1.0, -0.02, 0.9), (1.0, 0.02, 0.98), "WoodMid", 0.005)]
    for i in range(13):
        x = -0.96 + i * 0.16
        p.append(boxmm("picket", (x - 0.05, -0.045, 0), (x + 0.05, -0.02, 1.15), "WoodMid", 0.004))
        p.append(box("tip", (0.07, 0.025, 0.07), (x, -0.0325, 1.16), "WoodMid", 0, rot=(0, math.radians(45), 0)))
    return [part(p, "Fence")]

@model
def street_lamp():
    p = [cyl("pole", 0.07, 4.4, (0, 0, 2.2), "Metal", 10, r2=0.05),
         box("arm", (0.05, 0.8, 0.05), (0, -0.4, 4.35), "Metal", 0.005),
         boxmm("head", (-0.14, -0.95, 4.2), (0.14, -0.6, 4.35), "Metal", 0.02)]
    body = part(p, "Lamp")
    bulb = part([boxmm("glass", (-0.11, -0.92, 4.18), (0.11, -0.63, 4.21), "Bulb", 0.005)], "Bulb")
    return [body, bulb]

@model
def car():
    p = [boxmm("body", (-0.9, -2.1, 0.3), (0.9, 2.1, 0.95), "BookBlue", 0.12, 2),
         boxmm("cabin", (-0.8, -1.0, 0.95), (0.8, 0.9, 1.45), "BookBlue", 0.12, 2),
         boxmm("ws", (-0.75, -1.02, 1.0), (0.75, -0.98, 1.4), "Glass", 0.02),
         boxmm("rw", (-0.75, 0.88, 1.0), (0.75, 0.92, 1.4), "Glass", 0.02),
         box("hl_l", (0.3, 0.03, 0.12), (-0.55, -2.11, 0.75), "Bulb", 0.01), box("hl_r", (0.3, 0.03, 0.12), (0.55, -2.11, 0.75), "Bulb", 0.01),
         box("tl_l", (0.3, 0.03, 0.1), (-0.6, 2.11, 0.8), "Red", 0.01), box("tl_r", (0.3, 0.03, 0.1), (0.6, 2.11, 0.8), "Red", 0.01)]
    for x in (-0.85, 0.85):
        for y in (-1.35, 1.35): p.append(cyl("wheel", 0.33, 0.25, (x, y, 0.33), "Rubber", 16, rot=(0, math.radians(90), 0)))
    return [part(p, "Car")]

if __name__ == "__main__":
    only = None
    for a in sys.argv:
        if a.startswith("--only="): only = a[7:].split(",")
    for name, fn in MODELS.items():
        if only and name not in only: continue
        clear_scene_objects()
        objs = fn()
        export(objs, name, OUTD)
        print("exported", name, sum(len(o.data.polygons) for o in objs if o.type == 'MESH'), "faces")
