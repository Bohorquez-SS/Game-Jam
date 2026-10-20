import sys, bpy, math
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=sys.argv[1])
sc=bpy.context.scene
cam=bpy.data.objects.new("c",bpy.data.cameras.new("c")); sc.collection.objects.link(cam); sc.camera=cam
cam.data.type='ORTHO'; cam.data.ortho_scale=0.9; cam.location=(0,-0.1,1.5); cam.rotation_euler=(0,0,0)
sc.render.engine='BLENDER_WORKBENCH'; sh=sc.display.shading; sh.light='STUDIO'; sh.color_type='MATERIAL'; sh.show_cavity=True
sc.render.resolution_x=1200; sc.render.resolution_y=700; sc.world=bpy.data.worlds.new("w"); sc.world.color=(0.3,0.3,0.33)
sc.render.filepath=sys.argv[2]; bpy.ops.render.render(write_still=True)
