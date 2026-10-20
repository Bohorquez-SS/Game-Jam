"""Shared Blender helpers for GOOD NIGHT models.
Conventions: meters, Blender Z up, object FRONT faces -Y, origin at floor center (z=0) unless noted.
Materials are named GN_<Name>; Unity's builder remaps them by name to its own URP materials.
"""
import bpy, bmesh, math, os
from mathutils import Vector, Matrix

OUT = None

def reset():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    for c in (bpy.data.meshes, bpy.data.materials, bpy.data.objects):
        for x in list(c): c.remove(x)

_mats = {}
COLORS = {
    "WoodDark": (0.16, 0.09, 0.05), "WoodLight": (0.45, 0.30, 0.18), "WoodMid": (0.30, 0.18, 0.10),
    "Paint": (0.78, 0.76, 0.72), "Metal": (0.45, 0.45, 0.48), "Chrome": (0.8, 0.8, 0.82), "PlasticDark": (0.05, 0.05, 0.06),
    "PlasticBeige": (0.72, 0.68, 0.58), "Sheet": (0.75, 0.76, 0.80), "Blanket": (0.20, 0.25, 0.45), "Pillow": (0.85, 0.85, 0.82),
    "Fabric": (0.42, 0.18, 0.16), "FabricGreen": (0.18, 0.28, 0.22), "Ceramic": (0.92, 0.92, 0.9), "Glass": (0.6, 0.7, 0.8),
    "Screen": (0.02, 0.03, 0.04), "LampShade": (0.95, 0.85, 0.65), "Bulb": (1, 0.95, 0.8), "Paper": (0.9, 0.88, 0.8),
    "BookRed": (0.45, 0.1, 0.08), "BookBlue": (0.1, 0.16, 0.4), "BookGreen": (0.12, 0.3, 0.16), "BookCream": (0.7, 0.62, 0.5),
    "Rubber": (0.03, 0.03, 0.03), "Red": (0.6, 0.05, 0.04), "Counter": (0.62, 0.6, 0.56), "Fridge": (0.88, 0.88, 0.86),
    "Skin": (0.78, 0.58, 0.46), "Hoodie": (0.18, 0.24, 0.36), "Jeans": (0.12, 0.16, 0.28), "Hair": (0.05, 0.04, 0.03),
    "EntitySkin": (0.55, 0.52, 0.48), "EntityCloth": (0.05, 0.05, 0.05), "Eye": (0.9, 0.9, 0.85), "EyeDark": (0.0, 0.0, 0.0),
    "Mouth": (0.12, 0.02, 0.02), "Cardboard": (0.55, 0.4, 0.25), "Brass": (0.7, 0.55, 0.25), "Plant": (0.15, 0.35, 0.15),
    "Soil": (0.15, 0.1, 0.06), "Towel": (0.6, 0.75, 0.8), "Tape": (0.08, 0.08, 0.08), "Label": (0.9, 0.9, 0.85),
    "Phone": (0.02, 0.02, 0.025), "PhoneScreen": (0.05, 0.06, 0.08), "Coat": (0.25, 0.2, 0.15), "Carpet": (0.4, 0.15, 0.13),
}

def mat(name):
    full = "GN_" + name
    if full in _mats: return _mats[full]
    m = bpy.data.materials.new(full)
    c = COLORS.get(name, (0.5, 0.5, 0.5))
    m.diffuse_color = (c[0], c[1], c[2], 1)
    m.use_nodes = True
    bsdf = m.node_tree.nodes.get("Principled BSDF")
    if bsdf: bsdf.inputs["Base Color"].default_value = (c[0], c[1], c[2], 1); bsdf.inputs["Roughness"].default_value = 0.7
    _mats[full] = m
    return m

def _link(obj):
    bpy.context.scene.collection.objects.link(obj)
    return obj

def mesh_from_bm(bm, name, material):
    me = bpy.data.meshes.new(name)
    bm.to_mesh(me); bm.free()
    ob = _link(bpy.data.objects.new(name, me))
    ob.data.materials.append(mat(material))
    return ob

def box(name, size, loc, material, bevel=0.01, segs=1, rot=(0, 0, 0)):
    """Box by size (x,y,z) centered at loc. Bevel in meters (0 = none)."""
    bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=1.0)
    for v in bm.verts: v.co = Vector((v.co.x * size[0], v.co.y * size[1], v.co.z * size[2]))
    if bevel > 0:
        bmesh.ops.bevel(bm, geom=list(bm.edges), offset=min(bevel, min(size) * 0.45), segments=segs, affect='EDGES', profile=0.5)
    ob = mesh_from_bm(bm, name, material)
    ob.location = Vector(loc); ob.rotation_euler = rot
    return ob

def boxmm(name, mn, mx, material, bevel=0.01, segs=1):
    size = (mx[0] - mn[0], mx[1] - mn[1], mx[2] - mn[2])
    loc = ((mx[0] + mn[0]) / 2, (mx[1] + mn[1]) / 2, (mx[2] + mn[2]) / 2)
    return box(name, size, loc, material, bevel, segs)

def cyl(name, r, h, loc, material, verts=16, rot=(0, 0, 0), r2=None, bevel=0.0):
    bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=False, segments=verts, radius1=r, radius2=r if r2 is None else r2, depth=h)
    if bevel > 0:
        edges = [e for e in bm.edges if abs(e.verts[0].co.z - e.verts[1].co.z) < 1e-5]
        bmesh.ops.bevel(bm, geom=edges, offset=bevel, segments=1, affect='EDGES')
    ob = mesh_from_bm(bm, name, material)
    ob.location = Vector(loc); ob.rotation_euler = rot
    return ob

def sphere(name, r, loc, material, scale=(1, 1, 1), segs=12, rings=8):
    bm = bmesh.new()
    bmesh.ops.create_uvsphere(bm, u_segments=segs, v_segments=rings, radius=r)
    for v in bm.verts: v.co = Vector((v.co.x * scale[0], v.co.y * scale[1], v.co.z * scale[2]))
    ob = mesh_from_bm(bm, name, material)
    ob.location = Vector(loc)
    return ob

def cushion(name, size, loc, material, puff=0.35, rot=(0, 0, 0)):
    """Soft rounded box (pillow/cushion/mattress)."""
    bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=1.0)
    bmesh.ops.subdivide_edges(bm, edges=list(bm.edges), cuts=3, use_grid_fill=True)
    for v in bm.verts:
        p = v.co.copy()
        # superellipsoid-ish rounding
        n = Vector((p.x * 2, p.y * 2, p.z * 2))
        k = 1 - puff * (n.x ** 2 * 0.5 + n.y ** 2 * 0.5) * 0.0
        v.co = Vector((p.x * size[0], p.y * size[1], p.z * size[2] * (1 - puff * (n.x ** 4 + n.y ** 4) * 0.5)))
    ob = mesh_from_bm(bm, name, material)
    mod = ob.modifiers.new("sub", 'SUBSURF'); mod.levels = 1; mod.render_levels = 1
    ob.location = Vector(loc); ob.rotation_euler = rot
    return ob

def join(objs, name):
    for o in bpy.context.selected_objects: o.select_set(False)
    for o in objs: o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]
    for o in objs:
        for m in list(o.modifiers):
            bpy.context.view_layer.objects.active = o
            bpy.ops.object.modifier_apply(modifier=m.name)
    bpy.context.view_layer.objects.active = objs[0]
    bpy.ops.object.join()
    ob = bpy.context.view_layer.objects.active
    ob.name = name; ob.data.name = name
    return ob

def uv_cube(ob, size=1.0):
    for o in bpy.context.selected_objects: o.select_set(False)
    ob.select_set(True); bpy.context.view_layer.objects.active = ob
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.uv.cube_project(cube_size=size, correct_aspect=False, scale_to_bounds=False)
    bpy.ops.object.mode_set(mode='OBJECT')

def finish(objs, name, uv=1.0, shade_smooth=False):
    ob = join(objs, name) if len(objs) > 1 else objs[0]
    ob.name = name
    bpy.context.view_layer.objects.active = ob
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    uv_cube(ob, uv)
    if shade_smooth:
        for p in ob.data.polygons: p.use_smooth = True
    return ob

def export(objs, filename, out_dir):
    os.makedirs(out_dir, exist_ok=True)
    for o in bpy.context.selected_objects: o.select_set(False)
    for o in objs: o.select_set(True)
    path = os.path.join(out_dir, filename + ".fbx")
    bpy.ops.export_scene.fbx(filepath=path, use_selection=True, apply_scale_options='FBX_SCALE_ALL',
                             axis_forward='-Z', axis_up='Y', bake_space_transform=True, object_types={'MESH', 'ARMATURE', 'EMPTY'},
                             use_mesh_modifiers=True, mesh_smooth_type='FACE', add_leaf_bones=False, bake_anim=False)
    return path

def clear_scene_objects():
    for o in list(bpy.data.objects): bpy.data.objects.remove(o, do_unlink=True)
    for m in list(bpy.data.meshes): bpy.data.meshes.remove(m)
