"""Render a contact sheet of exported FBX models (Workbench). python3 preview.py <fbx_dir> <out.png> [names,...]"""
import sys, os, math, bpy
from mathutils import Vector
fbx_dir, out = sys.argv[1], sys.argv[2]
names = sys.argv[3].split(",") if len(sys.argv) > 3 and sys.argv[3] else sorted(f[:-4] for f in os.listdir(fbx_dir) if f.endswith(".fbx"))
cam_dir = sys.argv[4] if len(sys.argv) > 4 else "front"
bpy.ops.wm.read_factory_settings(use_empty=True)
sc = bpy.context.scene
cols = math.ceil(math.sqrt(len(names) * 1.6))
x = y = 0; rowh = 0; spacing = 0.4
for i, n in enumerate(names):
    before = set(bpy.data.objects)
    bpy.ops.import_scene.fbx(filepath=os.path.join(fbx_dir, n + ".fbx"))
    new = [o for o in bpy.data.objects if o not in before]
    mn = Vector((1e9,) * 3); mx = Vector((-1e9,) * 3)
    for o in new:
        if o.type != 'MESH': continue
        for c in o.bound_box:
            w = o.matrix_world @ Vector(c); mn = Vector(map(min, mn, w)); mx = Vector(map(max, mx, w))
    size = mx - mn
    if i % cols == 0 and i > 0: x = 0; y -= rowh + spacing; rowh = 0
    off = Vector((x - mn.x, y - mn.y, -mn.z))
    roots = [o for o in new if o.parent is None]
    for o in roots: o.location += off
    t = bpy.data.objects.new("lbl", bpy.data.curves.new("t", 'FONT')); t.data.body = n; t.data.size = 0.12
    sc.collection.objects.link(t); t.location = (x, y - 0.25, 0); t.rotation_euler = (math.radians(90), 0, 0)
    x += size.x + spacing; rowh = max(rowh, size.y)
# camera framing
allmn = Vector((1e9,) * 3); allmx = Vector((-1e9,) * 3)
for o in bpy.data.objects:
    if o.type not in ('MESH',): continue
    for c in o.bound_box:
        w = o.matrix_world @ Vector(c); allmn = Vector(map(min, allmn, w)); allmx = Vector(map(max, allmx, w))
ctr = (allmn + allmx) / 2
cam = bpy.data.objects.new("cam", bpy.data.cameras.new("cam")); sc.collection.objects.link(cam); sc.camera = cam
cam.data.type = 'ORTHO'; cam.data.ortho_scale = max(allmx.x - allmn.x, (allmx.z - allmn.z + allmx.y - allmn.y)) * 1.05
cam.location = ctr + Vector((0, -30, 22)); cam.rotation_euler = (math.radians(55), 0, 0)
sc.render.engine = 'BLENDER_WORKBENCH'
sh = sc.display.shading
sh.light = 'STUDIO'; sh.color_type = 'MATERIAL'; sh.show_shadows = True; sh.show_cavity = True; sh.cavity_type = 'BOTH'
sc.render.resolution_x = 1800; sc.render.resolution_y = 1100
sc.world = bpy.data.worlds.new("w"); sc.world.color = (0.15, 0.15, 0.17)
sc.render.filepath = out
bpy.ops.render.render(write_still=True)
print("rendered", out)
