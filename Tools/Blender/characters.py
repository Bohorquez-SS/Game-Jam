"""GOOD NIGHT — rigged characters (entity + double) and first-person hands/phone.
python3 characters.py <out_dir>
Characters: skin-modifier body + hand-built armature + automatic weights + named actions
(Idle, Walk, Run, LookAround, Stare, Attack, Crawl, LieIdle, SitUp). Front faces -Y.
"""
import sys, os, math, bpy, bmesh
from mathutils import Vector, Euler, Quaternion
sys.path.insert(0, os.path.dirname(__file__))
from gnlib import *

OUTD = sys.argv[-1]

EDGES = []
# ----------------------------------------------------------------------------- body graph
def body_graph(P):
    """P = proportions dict. Returns (verts {name:(pos,radius)}, edges[(a,b)])."""
    H = P["height"]; s = H / 1.75
    leg = P.get("leg", 1.0); arm = P.get("arm", 1.0); neck = P.get("neck", 1.0); thin = P.get("thin", 1.0)
    v = {}
    def add(n, x, y, z, r): v[n] = (Vector((x, y, z)), r)
    hip_z = 0.92 * s * leg
    add("pelvis", 0, 0, hip_z, 0.13 * s * thin)
    add("spine", 0, 0.0, hip_z + 0.22 * s, 0.12 * s * thin)
    add("chest", 0, -0.01, hip_z + 0.44 * s, 0.15 * s * thin)
    add("neck", 0, 0.0, hip_z + 0.6 * s, 0.05 * s)
    add("neck2", 0, -0.01, hip_z + 0.6 * s + 0.07 * s * neck, 0.048 * s)
    hz = hip_z + 0.6 * s + 0.07 * s * neck
    add("head", 0, -0.02, hz + 0.1 * s, 0.1 * s * P.get("head", 1.0))
    add("headtop", 0, -0.01, hz + 0.21 * s * P.get("headlong", 1.0), 0.085 * s * P.get("head", 1.0))
    for side, sx in (("L", -1), ("R", 1)):
        sh_z = hip_z + 0.52 * s
        add("clav" + side, sx * 0.09 * s, 0, sh_z, 0.06 * s * thin)
        add("shoulder" + side, sx * 0.2 * s, 0, sh_z, 0.06 * s * max(thin, 0.85))
        add("elbow" + side, sx * 0.25 * s, 0.01, sh_z - 0.29 * s * arm, 0.042 * s * max(thin, 0.8))
        add("wrist" + side, sx * 0.29 * s, -0.01, sh_z - 0.55 * s * arm, 0.032 * s)
        add("hand" + side, sx * 0.3 * s, -0.02, sh_z - 0.63 * s * arm, 0.04 * s * P.get("handsize", 1.0))
        add("fing" + side, sx * 0.31 * s, -0.025, sh_z - 0.63 * s * arm - 0.09 * s * P.get("finger", 1.0), 0.02 * s)
        add("hip" + side, sx * 0.09 * s, 0, hip_z - 0.04 * s, 0.085 * s * thin)
        add("knee" + side, sx * 0.1 * s, -0.01, hip_z * 0.52, 0.055 * s * thin)
        add("ankle" + side, sx * 0.1 * s, 0.01, 0.08 * s, 0.04 * s)
        add("toe" + side, sx * 0.1 * s, -0.14 * s, 0.035 * s, 0.035 * s)
    e = [("pelvis", "spine"), ("spine", "chest"), ("chest", "neck"), ("neck", "neck2"), ("neck2", "head"), ("head", "headtop")]
    for side in "LR":
        e += [("chest", "clav" + side), ("clav" + side, "shoulder" + side), ("shoulder" + side, "elbow" + side), ("elbow" + side, "wrist" + side),
              ("wrist" + side, "hand" + side), ("hand" + side, "fing" + side),
              ("pelvis", "hip" + side), ("hip" + side, "knee" + side), ("knee" + side, "ankle" + side), ("ankle" + side, "toe" + side)]
    EDGES[:] = e
    return v, e

def build_body(name, P, region_mats):
    v, e = body_graph(P)
    names = list(v.keys())
    bm = bmesh.new()
    bv = {n: bm.verts.new(v[n][0]) for n in names}
    for a, b in e: bm.edges.new((bv[a], bv[b]))
    me = bpy.data.meshes.new(name); bm.to_mesh(me); bm.free()
    ob = bpy.data.objects.new(name, me); bpy.context.scene.collection.objects.link(ob)
    sk = ob.modifiers.new("skin", 'SKIN')
    sk.use_smooth_shade = True
    for i, n in enumerate(names):
        r = v[n][1]
        sv = me.skin_vertices[0].data[i]
        sv.radius = (r, r * P.get("depth", 0.85))
        if n == "pelvis": sv.use_root = True
    sub = ob.modifiers.new("sub", 'SUBSURF'); sub.levels = 1
    for o in bpy.context.selected_objects: o.select_set(False)
    ob.select_set(True); bpy.context.view_layer.objects.active = ob
    bpy.ops.object.modifier_apply(modifier="skin"); bpy.ops.object.modifier_apply(modifier="sub")
    # materials by region
    mats = {}
    for key in set(region_mats.values()):
        ob.data.materials.append(mat(key)); mats[key] = len(ob.data.materials) - 1
    hip_z = 0.92 * P["height"] / 1.75 * P.get("leg", 1.0)
    for poly in ob.data.polygons:
        c = poly.center
        r = region_of(c, v, hip_z, P)
        poly.material_index = mats[region_mats.get(r, region_mats["torso"])]
        poly.use_smooth = True
    return ob, v

def region_of(c, v, hip_z, P, edges=None):
    best, bd = "torso", 1e9
    for a, b in EDGES:
        pa, ra = v[a]; pb, rb = v[b]
        ab = pb - pa; t = max(0.0, min(1.0, (c - pa).dot(ab) / max(ab.length_squared, 1e-9)))
        d = (c - (pa + ab * t)).length - (ra + (rb - ra) * t)
        if d < bd: bd, best = d, (b if t > 0.5 else a)
    if best in ("head", "headtop", "neck2"): return "head"
    if best == "neck": return "neck"
    if best.startswith(("hand", "fing")) or (best.startswith("wrist")): return "hand"
    if best.startswith(("toe", "ankle")): return "foot"
    if best.startswith(("knee", "hip")): return "leg"
    if best.startswith(("elbow", "shoulder", "clav")): return "arm"
    if best == "pelvis": return "pelvis"
    return "torso"

# ----------------------------------------------------------------------------- armature
BONES = [  # name, head-node, tail-node, parent
    ("Hips", "pelvis", "spine", None), ("Spine", "spine", "chest", "Hips"), ("Chest", "chest", "neck", "Spine"),
    ("Neck", "neck", "neck2", "Chest"), ("Head", "neck2", "headtop", "Neck"),
]
for s in "LR":
    BONES += [("Shoulder." + s, "clav" + s, "shoulder" + s, "Chest"), ("UpperArm." + s, "shoulder" + s, "elbow" + s, "Shoulder." + s),
              ("LowerArm." + s, "elbow" + s, "wrist" + s, "UpperArm." + s), ("Hand." + s, "wrist" + s, "fing" + s, "LowerArm." + s),
              ("UpperLeg." + s, "hip" + s, "knee" + s, "Hips"), ("LowerLeg." + s, "knee" + s, "ankle" + s, "UpperLeg." + s),
              ("Foot." + s, "ankle" + s, "toe" + s, "LowerLeg." + s)]

def build_armature(name, v):
    arm = bpy.data.armatures.new(name + "_Arm")
    ob = bpy.data.objects.new(name + "_Rig", arm); bpy.context.scene.collection.objects.link(ob)
    for o in bpy.context.selected_objects: o.select_set(False)
    ob.select_set(True); bpy.context.view_layer.objects.active = ob
    bpy.ops.object.mode_set(mode='EDIT')
    root = arm.edit_bones.new("Root"); root.head = (0, 0, 0); root.tail = (0, 0.25, 0)
    for n, h, t, p in BONES:
        b = arm.edit_bones.new(n)
        b.head = v[h][0]; b.tail = v[t][0]
        b.parent = arm.edit_bones[p] if p else root
        if p and n not in ("UpperLeg.L", "UpperLeg.R", "Shoulder.L", "Shoulder.R"): b.use_connect = (arm.edit_bones[p].tail - b.head).length < 1e-4
    bpy.ops.object.mode_set(mode='OBJECT')
    return ob

def bind(body, rig):
    for o in bpy.context.selected_objects: o.select_set(False)
    body.select_set(True); rig.select_set(True); bpy.context.view_layer.objects.active = rig
    bpy.ops.object.parent_set(type='ARMATURE_AUTO')
    ok = any(len(vg.name) for vg in body.vertex_groups) and any(len(v.groups) for v in body.data.vertices)
    if not ok:
        bpy.ops.object.parent_set(type='ARMATURE_ENVELOPE')

def attach(obj, rig, bone):
    """Rigidly attach a mesh to a bone (face features, hair, accessories)."""
    obj.parent = rig; obj.parent_type = 'BONE'; obj.parent_bone = bone
    pb = rig.pose.bones[bone]
    mw = obj.matrix_world.copy()
    obj.matrix_parent_inverse = (rig.matrix_world @ pb.matrix @ __import__('mathutils').Matrix.Translation((0, pb.length, 0))).inverted()

# ----------------------------------------------------------------------------- animation
def R(x=0, y=0, z=0):
    return Quaternion(Euler((math.radians(x), math.radians(y), math.radians(z)), 'XYZ'))

def make_action(rig, name, frames, fn, loop=True):
    act = bpy.data.actions.new(name); act.use_fake_user = True
    if rig.animation_data is None: rig.animation_data_create()
    rig.animation_data.action = act
    for pb in rig.pose.bones: pb.rotation_mode = 'QUATERNION'
    for f in range(0, frames + 1, 2 if frames > 40 else 1):
        t = f / frames
        pose = fn(t)
        for pb in rig.pose.bones:
            q, loc = pose.get(pb.name, (Quaternion(), None)) if isinstance(pose.get(pb.name), tuple) else (pose.get(pb.name, Quaternion()), None)
            pb.rotation_quaternion = q
            pb.keyframe_insert("rotation_quaternion", frame=f)
            if pb.name in ("Hips", "Root"):
                pb.location = loc if loc is not None else Vector((0, 0, 0))
                pb.keyframe_insert("location", frame=f)
    return act

def S(t, k=1.0, ph=0.0): return math.sin((t * k + ph) * 2 * math.pi)

def actions(rig, creepy):
    j = 1.0 if creepy else 0.0  # jerky offsets for the entity
    def idle(t):
        b = S(t)
        return {"Chest": R(2 * b), "Neck": R(-1 * b + 6 * j, 0, 4 * j), "Head": R(3 * j, 0, 10 * j + 2 * S(t, 1, 0.3)),
                "UpperArm.L": R(0, 0, -4 - 2 * b), "UpperArm.R": R(0, 0, 4 + 2 * b), "Spine": R(1.5 * b + 8 * j)}
    def walk(t):
        a = S(t); c = math.cos(t * 2 * math.pi)
        lean = 6 + 6 * j
        return {"UpperLeg.L": R(28 * a), "UpperLeg.R": R(-28 * a), "LowerLeg.L": R(-max(0, -c) * 40 - 5), "LowerLeg.R": R(-max(0, c) * 40 - 5),
                "Foot.L": R(-8 * a), "Foot.R": R(8 * a),
                "UpperArm.L": R(-18 * a * (1 - 0.6 * j), 0, -4), "UpperArm.R": R(18 * a * (1 - 0.6 * j), 0, 4),
                "LowerArm.L": R(-10 - 8 * j), "LowerArm.R": R(-10 - 8 * j),
                "Spine": R(lean, 4 * a), "Chest": R(2, -6 * a), "Neck": R(4 * j), "Head": R(0, 3 * a, 14 * j * (1 if S(t, 0.5) > 0 else -0.3)),
                "Hips": (R(0, 0, 0), Vector((0, 0.03 * abs(a), 0)))}
    def run(t):
        a = S(t); c = math.cos(t * 2 * math.pi)
        return {"UpperLeg.L": R(45 * a), "UpperLeg.R": R(-45 * a), "LowerLeg.L": R(-max(0, -c) * 75 - 10), "LowerLeg.R": R(-max(0, c) * 75 - 10),
                "UpperArm.L": R(-40 * a, 0, -10 - 20 * j), "UpperArm.R": R(40 * a, 0, 10 + 20 * j), "LowerArm.L": R(-60 + 30 * j), "LowerArm.R": R(-60 + 30 * j),
                "Spine": R(18 + 10 * j), "Chest": R(4, -10 * a), "Head": R(-10 - 8 * j), "Hips": (R(), Vector((0, 0.06 * abs(a), 0)))}
    def look(t):
        a = S(t)
        return {"Head": R(0, 0, 55 * a), "Neck": R(0, 0, 20 * a), "Chest": R(0, 0, 10 * a), "Spine": R(4 * j)}
    def stare(t):
        # slow unnatural head tilt and hold
        k = min(1.0, t * 1.6)
        return {"Head": R(-5, 0, 35 * k * (1 if creepy else 0.4)), "Neck": R(8 * k), "Spine": R(10 * j), "Chest": R(4 * S(t, 2) * j),
                "UpperArm.L": R(0, 0, -3), "UpperArm.R": R(0, 0, 3), "Hand.L": R(0, 0, 10 * S(t, 6) * j)}
    def attack(t):
        k = min(1.0, t * 3.0)
        return {"UpperArm.L": R(-110 * k, 0, -15), "UpperArm.R": R(-110 * k, 0, 15), "LowerArm.L": R(-20 * k), "LowerArm.R": R(-20 * k),
                "Spine": R(25 * k), "Chest": R(10 * k), "Neck": R(-20 * k), "Head": R(-25 * k, 0, 15 * j * S(t, 4)),
                "Hips": (R(), Vector((0, 0.0, -0.25 * k)))}
    def crawl_peek(t):
        k = math.sin(min(1.0, t) * math.pi)
        return {"Spine": R(10 + 25 * k), "Chest": R(10 * k, 0, 25 * k), "Neck": R(15 * k, 0, 20 * k), "Head": R(0, 0, 55 * k),
                "UpperArm.L": R(-20 * k, 0, -6), "UpperArm.R": R(-60 * k, 0, 30 * k)}
    def lie(t):
        b = S(t)
        return {"Hips": (R(-90), Vector((0, 0, 0))), "Chest": R(1.5 * b), "Head": R(0, 0, 8 * S(t, 0.5)),
                "UpperArm.L": R(0, 0, -6), "UpperArm.R": R(0, 0, 6)}
    def situp(t):
        k = 0.5 - 0.5 * math.cos(min(1.0, t) * math.pi)
        return {"Hips": (R(-90), Vector((0, 0, 0))), "UpperLeg.L": R(0), "UpperLeg.R": R(0),
                "Spine": R(65 * k), "Chest": R(20 * k), "Neck": R(5 * k), "Head": R(0, 0, 0),
                "UpperArm.L": R(-30 * k, 0, -10), "UpperArm.R": R(-30 * k, 0, 10), "LowerArm.L": R(-40 * k), "LowerArm.R": R(-40 * k)}
    acts = [("Idle", 60, idle), ("Walk", 30 if not creepy else 36, walk), ("Run", 18, run), ("LookAround", 120, look),
            ("Stare", 90, stare), ("Attack", 24, attack), ("Peek", 72, crawl_peek), ("LieIdle", 80, lie), ("SitUp", 90, situp)]
    out = []
    for n, fr, fn in acts: out.append(make_action(rig, n, fr, fn))
    rig.animation_data.action = out[0]
    return out

# ----------------------------------------------------------------------------- faces / hair
def face_entity(rig, v, s):
    hp = v["head"][0]; r = v["head"][1]
    parts = []
    for sx in (-1, 1):
        sock = sphere("socket", r * 0.33, hp + Vector((sx * r * 0.42, -r * 0.82, r * 0.18)), "EyeDark", scale=(1.1, 0.5, 1.35))
        pup = sphere("pupil", r * 0.07, hp + Vector((sx * r * 0.4, -r * 0.98, r * 0.16)), "Eye", segs=8, rings=6)
        parts += [sock, pup]
    mouth = box("mouth", (r * 1.25, r * 0.15, r * 0.12), hp + Vector((0, -r * 0.86, -r * 0.42)), "Mouth", r * 0.05, rot=(0, 0, 0))
    mouth.rotation_euler = (0, math.radians(4), 0)
    teeth = box("teeth", (r * 1.05, r * 0.05, r * 0.05), hp + Vector((0, -r * 0.93, -r * 0.38)), "Eye", 0.0)
    parts += [mouth, teeth]
    # long wet black hair hanging in strands
    import random; rnd = random.Random(8)
    for i in range(26):
        a = math.radians(rnd.uniform(40, 320)); rr = r * 0.95
        top = hp + Vector((math.sin(a) * rr * 0.8, math.cos(a) * rr * 0.8 * 0.9 + 0.01, r * 0.75))
        front = math.cos(a) < -0.3
        if math.cos(a) < -0.55: continue
        L = rnd.uniform(0.35, 0.6) * s * (0.7 if front else 1.0)
        st = box("hair", (0.035, 0.02, L), top + Vector((math.sin(a) * 0.03, math.cos(a) * 0.03, -L / 2)), "Hair", 0.004)
        st.rotation_euler = (math.radians(rnd.uniform(-8, 8)), math.radians(rnd.uniform(-6, 6)), a)
        parts.append(st)
    cap = sphere("scalp", r * 1.06, hp + Vector((0, 0.03, r * 0.3)), "Hair", scale=(1.02, 1, 1.0))
    parts.append(cap)
    f = join(parts, "Face")
    for p in f.data.polygons: p.use_smooth = True
    attach(f, rig, "Head")
    return f

def face_double(rig, v, s):
    hp = v["head"][0]; r = v["head"][1]
    parts = []
    for sx in (-1, 1):
        parts.append(sphere("eyew", r * 0.15, hp + Vector((sx * r * 0.36, -r * 0.86, r * 0.12)), "Eye", scale=(1, 0.5, 0.75), segs=8, rings=6))
        parts.append(sphere("iris", r * 0.07, hp + Vector((sx * r * 0.36, -r * 0.94, r * 0.12)), "EyeDark", segs=8, rings=6))
        parts.append(box("brow", (r * 0.36, r * 0.06, r * 0.06), hp + Vector((sx * r * 0.36, -r * 0.9, r * 0.36)), "Hair", r * 0.02))
    parts.append(box("nose", (r * 0.14, r * 0.25, r * 0.3), hp + Vector((0, -r * 0.98, -r * 0.05)), "Skin", r * 0.05))
    parts.append(box("mouth", (r * 0.5, r * 0.05, r * 0.05), hp + Vector((0, -r * 0.9, -r * 0.42)), "Mouth", 0.0))
    hair = sphere("hair", r * 0.98, hp + Vector((0, r * 0.38, r * 0.42)), "Hair", scale=(1.04, 0.95, 0.85), segs=14, rings=8)
    fringe = box("fringe", (r * 1.5, r * 0.35, r * 0.22), hp + Vector((0, -r * 0.7, r * 0.72)), "Hair", r * 0.08)
    parts += [hair, fringe]
    for sx in (-1, 1): parts.append(sphere("ear", r * 0.18, hp + Vector((sx * r * 0.98, 0, 0)), "Skin", scale=(0.5, 1, 1.2), segs=8, rings=6))
    # hood
    parts.append(sphere("hood", r * 1.1, v["neck"][0] + Vector((0, r * 0.8, -r * 0.2)), "Hoodie", scale=(1.3, 0.7, 0.55)))
    f = join(parts, "Face")
    for p in f.data.polygons: p.use_smooth = True
    attach(f, rig, "Head")
    return f

def build_character(name, P, region_mats, creepy, face_fn):
    clear_scene_objects()
    for a in list(bpy.data.actions): bpy.data.actions.remove(a)
    for a in list(bpy.data.armatures): bpy.data.armatures.remove(a)
    body, v = build_body(name + "_Body", P, region_mats)
    if creepy:  # dress: a cone skirt over the legs
        hip = v["pelvis"][0]
        skirt = cyl("dress", 0.25 * P["height"] / 1.75, hip.z - 0.12, (0, 0.0, (hip.z + 0.12) / 2 + 0.05), "EntityCloth", 14, r2=0.15 * P["height"] / 1.75)
        for p in skirt.data.polygons: p.use_smooth = True
    rig = build_armature(name, v)
    if creepy:
        bpy.ops.object.select_all(action='DESELECT')
        body.select_set(True); skirt.select_set(True); bpy.context.view_layer.objects.active = body
        bpy.ops.object.join(); body = bpy.context.view_layer.objects.active
    bind(body, rig)
    face = face_fn(rig, v, P["height"] / 1.75)
    acts = actions(rig, creepy)
    body.name = "Body"
    return [rig, body, face], acts

def export_character(objs, filename):
    for o in bpy.context.selected_objects: o.select_set(False)
    for o in objs: o.select_set(True)
    path = os.path.join(OUTD, filename + ".fbx")
    bpy.ops.export_scene.fbx(filepath=path, use_selection=True, apply_scale_options='FBX_SCALE_ALL',
                             axis_forward='-Z', axis_up='Y', bake_space_transform=False, object_types={'MESH', 'ARMATURE'},
                             add_leaf_bones=False, bake_anim=True, bake_anim_use_all_actions=True, bake_anim_use_nla_strips=False,
                             bake_anim_force_startend_keying=True, bake_anim_simplify_factor=0.0, mesh_smooth_type='FACE',
                             armature_nodetype='NULL', primary_bone_axis='Y', secondary_bone_axis='X')
    print("exported", filename)

# ----------------------------------------------------------------------------- hands & phone
def hand_mesh(name, pose, side=1, material="Skin", sleeve="Hoodie"):
    """Hand from a branching skin skeleton. pose: grip | reach | flash. Hand points +Y, palm faces +Z (grip)."""
    bm = bmesh.new(); radii = {}
    def V(p, r):
        v = bm.verts.new(Vector(p)); radii[v] = r; return v
    def E(a, b): bm.edges.new((a, b))
    elbow = V((0, -0.2, -0.005), 0.04); wrist = V((0, -0.005, 0), 0.026); palm = V((0, 0.06, 0.0), 0.03)
    E(elbow, wrist); E(wrist, palm)
    for i, x in enumerate((-0.03, -0.01, 0.01, 0.029)):
        x *= side; ln = (0.85, 1.0, 0.95, 0.75)[i]
        k = V((x, 0.095, 0.0), 0.012); E(palm, k)
        if pose == "grip":   pts = [(x, 0.125, 0.012), (x * 1.02, 0.135, 0.04), (x, 0.125, 0.06)]
        elif pose == "flash": pts = [(x, 0.115, -0.022), (x, 0.098, -0.04), (x, 0.08, -0.03)]
        else:                 pts = [(x, 0.095 + 0.03 * ln, -0.004), (x, 0.095 + 0.055 * ln, -0.01), (x, 0.095 + 0.075 * ln, -0.016)]
        prev = k
        for j, pt in enumerate(pts):
            v = V(pt, 0.0105 - j * 0.0012); E(prev, v); prev = v
    tb = V((-0.028 * side, 0.03, -0.005), 0.014); E(wrist, tb)
    if pose == "grip": tp = [(-0.045 * side, 0.06, 0.015), (-0.05 * side, 0.085, 0.035)]
    elif pose == "flash": tp = [(-0.04 * side, 0.07, -0.02), (-0.025 * side, 0.09, -0.035)]
    else: tp = [(-0.05 * side, 0.06, -0.01), (-0.06 * side, 0.085, -0.015)]
    prev = tb
    for pt in tp:
        v = V(pt, 0.011); E(prev, v); prev = v
    order = list(bm.verts)
    me = bpy.data.meshes.new(name); bm.to_mesh(me)
    rlist = [radii[v] for v in order]; bm.free()
    ob = bpy.data.objects.new(name, me); bpy.context.scene.collection.objects.link(ob)
    ob.modifiers.new("skin", 'SKIN')
    for i, sv in enumerate(me.skin_vertices[0].data):
        r = rlist[i]; flat = 0.55 if me.vertices[i].co.y > 0.0 and me.vertices[i].co.y < 0.1 and r > 0.02 else 0.85
        sv.radius = (r, r * flat)
    me.skin_vertices[0].data[0].use_root = True
    ob.modifiers.new("sub", 'SUBSURF').levels = 1
    for o in bpy.context.selected_objects: o.select_set(False)
    ob.select_set(True); bpy.context.view_layer.objects.active = ob
    bpy.ops.object.modifier_apply(modifier="skin"); bpy.ops.object.modifier_apply(modifier="sub")
    ob.data.materials.append(mat(material)); ob.data.materials.append(mat(sleeve))
    for p in ob.data.polygons:
        p.use_smooth = True
        p.material_index = 1 if p.center.y < -0.04 else 0
    return ob

def build_hands():
    clear_scene_objects()
    out = []
    # right hand holding phone (phone held in front of palm, screen facing -Y toward camera... in hand space screen faces +Z)
    h = hand_mesh("PhoneHand", "grip", side=1)
    out.append(h)
    ph = boxmm("Phone", (-0.037, 0.08, 0.035), (0.037, 0.235, 0.046), "Phone", 0.006, 2)
    ph = finish([ph, box("cam", (0.012, 0.012, 0.004), (0.022, 0.215, 0.033), "Glass", 0.002)], "Phone")
    out.append(ph)
    scr = finish([box("s", (0.066, 0.143, 0.001), (0, 0.1575, 0.0468), "PhoneScreen", 0)], "PhoneScreen", uv=0.143)
    out.append(scr)
    reach = hand_mesh("ReachHand", "reach", side=1); reach.location = (0.3, 0, 0); out.append(reach)
    flash = hand_mesh("FlashHand", "flash", side=-1); flash.location = (-0.3, 0, 0)
    fl = cyl("flashlight", 0.02, 0.2, (-0.3, 0.12, -0.02), "PlasticDark", 12, rot=(math.radians(90), 0, 0))
    lens = cyl("lens", 0.026, 0.03, (-0.3, 0.23, -0.02), "Chrome", 12, rot=(math.radians(90), 0, 0))
    out.append(flash); out.append(finish([fl, lens], "Flashlight"))
    for o in bpy.context.selected_objects: o.select_set(False)
    for o in out: o.select_set(True)
    bpy.ops.export_scene.fbx(filepath=os.path.join(OUTD, "fp_hands.fbx"), use_selection=True, apply_scale_options='FBX_SCALE_ALL',
                             axis_forward='-Z', axis_up='Y', bake_space_transform=True, object_types={'MESH'}, mesh_smooth_type='FACE')
    print("exported fp_hands")

if __name__ == "__main__":
    os.makedirs(OUTD, exist_ok=True)
    ent_P = dict(height=2.02, leg=1.06, arm=1.3, neck=1.8, thin=0.84, head=0.92, headlong=1.15, finger=1.25, handsize=1.1, depth=0.75)
    ent_regions = {"head": "EntitySkin", "neck": "EntitySkin", "hand": "EntitySkin", "torso": "EntityCloth", "arm": "EntityCloth",
                   "pelvis": "EntityCloth", "leg": "EntitySkin", "foot": "EntitySkin"}
    objs, _ = build_character("Entity", ent_P, ent_regions, True, face_entity)
    export_character(objs, "entity")
    dbl_P = dict(height=1.74, depth=0.8)
    dbl_regions = {"head": "Skin", "neck": "Skin", "hand": "Skin", "torso": "Hoodie", "arm": "Hoodie", "pelvis": "Jeans", "leg": "Jeans", "foot": "PlasticDark"}
    objs, _ = build_character("Double", dbl_P, dbl_regions, False, face_double)
    export_character(objs, "double")
    build_hands()
