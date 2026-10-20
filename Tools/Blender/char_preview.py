import sys, os, math, bpy
from mathutils import Vector
fbx, out = sys.argv[1], sys.argv[2]
poses = sys.argv[3].split(",")  # e.g. Idle:0,Walk:8,Run:5,Stare:80,Attack:20
cam_side = sys.argv[4] if len(sys.argv) > 4 else "front"
bpy.ops.wm.read_factory_settings(use_empty=True)
sc = bpy.context.scene
for i, p in enumerate(poses):
    name, frame = p.split(":")
    before = set(bpy.data.objects)
    bpy.ops.import_scene.fbx(filepath=fbx, automatic_bone_orientation=False)
    new = [o for o in bpy.data.objects if o not in before]
    rig = [o for o in new if o.type == 'ARMATURE'][0]
    acts = [a for a in bpy.data.actions if a.name.endswith("|" + name) or a.name == name]
    if rig.animation_data is None: rig.animation_data_create()
    if acts:
        rig.animation_data.action = acts[0]
        try: rig.animation_data.action_slot = acts[0].slots[0]
        except Exception: pass
    holder = bpy.data.objects.new("holder%d" % i, None); sc.collection.objects.link(holder)
    holder.location.x = i * 1.1
    for o in new:
        if o.parent is None: o.parent = holder
    t = bpy.data.objects.new("lbl", bpy.data.curves.new("t", 'FONT')); t.data.body = p; t.data.size = 0.12
    sc.collection.objects.link(t); t.location = (i * 1.1 - 0.3, -0.5, 0.02); t.rotation_euler = (math.radians(90), 0, 0)
sc.frame_set(0)
for i, p in enumerate(poses):
    pass
# frames differ per copy: evaluate by setting scene frame per object is impossible; use the max requested frame for all
frames = [int(p.split(":")[1]) for p in poses]
sc.frame_set(frames[0])
cam = bpy.data.objects.new("cam", bpy.data.cameras.new("cam")); sc.collection.objects.link(cam); sc.camera = cam
cam.data.type = 'ORTHO'; cam.data.ortho_scale = max(2.6, len(poses) * 1.15)
mid = (len(poses) - 1) * 1.1 / 2
if cam_side == "side":
    cam.location = (mid + 0.0, -12, 1.1); cam.rotation_euler = (math.radians(90), 0, math.radians(0))
else:
    cam.location = (mid, -12, 3.0); cam.rotation_euler = (math.radians(80), 0, 0)
sc.render.engine = 'BLENDER_WORKBENCH'
sh = sc.display.shading; sh.light = 'STUDIO'; sh.color_type = 'MATERIAL'; sh.show_shadows = True; sh.show_cavity = True
sc.render.resolution_x = 1600; sc.render.resolution_y = 900
sc.world = bpy.data.worlds.new("w"); sc.world.color = (0.3, 0.3, 0.33)
sc.render.filepath = out
bpy.ops.render.render(write_still=True)
print("acts:", [a.name for a in bpy.data.actions][:12])
