"""Author human-proportioned RIMBARA NPCs from MPFB CC0 assets.

Run with portable Blender, not Unity's importer. Source authoring tools and
downloaded asset libraries live outside the project; only selected generated
FBX, textures, .blend sources and provenance are exported into Assets.
"""
import argparse
import json
import os
import sys
import hashlib
from pathlib import Path

import bpy
import numpy as np
from mathutils import Vector

parser = argparse.ArgumentParser()
parser.add_argument("--mpfb", required=True)
parser.add_argument("--assets", required=True)
parser.add_argument("--output", required=True)
parser.add_argument("--probe", action="store_true")
parser.add_argument("--sources")
parser.add_argument("--preview")
parser.add_argument("--preview-only", action="store_true")
args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:])
sys.path.insert(0, str(Path(args.mpfb) / "src"))
user = Path(args.output).parent / ".mpfb-authoring-cache"
user.mkdir(parents=True, exist_ok=True)
# The addon is imported from its pinned source tree, not installed as a global
# Blender extension. Keep its local cache isolated and don't save user preferences.
bpy.utils.extension_path_user = lambda *a, **k: str(user)
bpy.ops.preferences.addon_enable(module="mpfb")
from mpfb.services.humanservice import HumanService
from mpfb.services.targetservice import TargetService
from mpfb.services.exportservice import ExportService
from mpfb.services.rigservice import RigService
from mpfb.services.objectservice import ObjectService
from mpfb.services.clothesservice import ClothesService
from mpfb.entities.clothes.mhclo import Mhclo
from mpfb.entities.material.mhmaterial import MhMaterial

output = Path(args.output)
output.mkdir(parents=True, exist_ok=True)
assets = Path(args.assets)
sources = Path(args.sources) if args.sources else output.parent / "sources"
sources.mkdir(parents=True, exist_ok=True)
textures = output / "Textures"
textures.mkdir(exist_ok=True)
provenance = {}
image_cache = {}


def find_file(subdir, name):
    matches = sorted((assets / subdir).rglob(name))
    if name.endswith(".mhclo"):
        binaries = sorted((assets / subdir).rglob(name.replace(".mhclo", ".mhpxy")))
        if binaries:
            matches = [unpack_system_proxy(binaries[0])]
    if not matches:
        raise FileNotFoundError(f"Required verified system asset missing: {subdir}/{name}")
    return matches[0]


def unpack_system_proxy(path):
    """Decode bundled CC0 binary graphical data into MPFB's text asset format.

    PPA packages ship geometry as NumPy arrays, not original OBJ/MHCLO files.
    This is a data-format conversion, not vendoring MakeHuman's program code.
    """
    remember(path)
    with np.load(path, allow_pickle=False) as proxy:
        text = proxy["lic_str"].tobytes().decode("utf-8")
        lengths = proxy["lic_idx"]
        values, position = [], 0
        for length in lengths:
            values.append(text[position:position+int(length)])
            position += int(length)
        license_info = dict(zip(values[::2], values[1::2]))
        if license_info.get("license") != "CC0":
            raise RuntimeError(f"Non-CC0 bundled asset: {path}: {license_info}")
        obj_path = path.parent / proxy["obj_file"].tobytes().decode("utf-8")
        binary_mesh = obj_path.with_suffix(".npz")
        remember(binary_mesh)
        with np.load(binary_mesh, allow_pickle=False) as mesh:
            lines = ["# CC0 MakeHuman bundled mesh; decoded from checksum-verified PPA data"]
            lines.extend("v " + " ".join(str(float(v)) for v in coord) for coord in mesh["coord"])
            lines.extend("vt " + " ".join(str(float(v)) for v in uv) for uv in mesh["texco"])
            for face_index, face in enumerate(mesh["fvert"]):
                face = list(face)
                if face[-1] == face[0]: face = face[:-1]
                if "fuvs" in mesh:
                    lines.append("f " + " ".join(f"{int(v)+1}/{int(mesh['fuvs'][face_index][i])+1}" for i,v in enumerate(face)))
                else:
                    lines.append("f " + " ".join(str(int(v)+1) for v in face))
            obj_path.write_text("\n".join(lines)+"\n", encoding="utf-8")
        material_name = proxy["material_file"].tobytes().decode()
        material_path = assets / material_name
        if not material_path.exists(): material_path = path.parent / material_name
        if not material_path.exists(): raise FileNotFoundError(material_path)
        lines = ["# license: CC0", "# author: MakeHuman Team", "name " + path.stem,
            "uuid " + proxy["uuid"].tobytes().decode(), "basemesh hm08", "obj_file " + obj_path.name,
            "material " + os.path.relpath(material_path, path.parent).replace("\\", "/"), "z_depth " + str(int(proxy["z_depth"]))]
        if "tmat_scale" in proxy:
            for i, axis in enumerate(("x", "y", "z")):
                lines.append(f"{axis}_scale {int(proxy['tmat_scale_idx'][2*i])} {int(proxy['tmat_scale_idx'][2*i+1])} {float(proxy['tmat_scale'][i])}")
        lines.append("verts 0")
        if int(proxy["num_refverts"]) == 1:
            lines.extend(str(int(v)) for v in proxy["ref_vIdxs"])
        else:
            for verts, weights, offsets in zip(proxy["ref_vIdxs"], proxy["weights"], proxy["offsets"]):
                lines.append(" ".join([str(int(v)) for v in verts]+[str(float(v)) for v in weights]+[str(float(v)) for v in offsets]))
        if "deleteVerts" in proxy:
            # MPFB consumes the first non-numeric line to exit the verts block.
            lines.append("end")
            lines.append("delete_verts")
            lines.extend(str(int(v)) for v in np.flatnonzero(proxy["deleteVerts"]))
    result = path.with_suffix(".mhclo")
    result.write_text("\n".join(lines)+"\n", encoding="utf-8")
    return result


def remember(path):
    path = Path(path).resolve()
    provenance[str(path.relative_to(assets.resolve()))] = hashlib.sha256(path.read_bytes()).hexdigest()


def material_from_file(path, name, tint=(1, 1, 1, 1), cutout=False):
    remember(path)
    data = MhMaterial()
    data.populate_from_mhmat(str(path))
    mat = bpy.data.materials.new(name)
    mat.use_nodes = True
    shader = mat.node_tree.nodes.get("Principled BSDF")
    shader.inputs["Roughness"].default_value = 0.82
    shader.inputs["Base Color"].default_value = tint
    mat.diffuse_color = tint
    tex = data.get_value("diffuseTexture")
    exported_texture = ""
    if tex:
        tex = Path(tex)
        remember(tex)
        digest = hashlib.sha256(tex.read_bytes()).hexdigest()[:12]
        exported_texture = f"{tex.stem}-{digest}.png"
        if exported_texture not in image_cache:
            image = bpy.data.images.load(str(tex), check_existing=True)
            if max(image.size) > 1024:
                ratio = 1024 / max(image.size)
                image.scale(int(image.size[0] * ratio), int(image.size[1] * ratio))
            image.filepath_raw = str(textures / exported_texture)
            image.file_format = "PNG"
            image.save()
            image_cache[exported_texture] = image
        node = mat.node_tree.nodes.new("ShaderNodeTexImage")
        node.image = image_cache[exported_texture]
        multiply = mat.node_tree.nodes.new("ShaderNodeMixRGB")
        multiply.blend_type = "MULTIPLY"
        multiply.inputs[0].default_value = 1
        multiply.inputs[2].default_value = tint
        mat.node_tree.links.new(node.outputs["Color"], multiply.inputs[1])
        mat.node_tree.links.new(multiply.outputs[0], shader.inputs["Base Color"])
        if cutout:
            mat.node_tree.links.new(node.outputs["Alpha"], shader.inputs["Alpha"])
            mat.surface_render_method = "DITHERED"
    return mat, {"name": name, "texture": exported_texture, "color": list(tint), "cutout": cutout}


def add_asset(human, subdir, filename, atype, prefix, tint=(1, 1, 1, 1), cutout=False):
    path = find_file(subdir, filename)
    remember(path)
    asset = Mhclo()
    asset.load(str(path))
    remember(asset.obj_file if hasattr(asset, "obj_file") else path.with_suffix(".obj"))
    obj = HumanService.add_mhclo_asset(str(path), human, asset_type="Clothes" if atype == "Shoes" else atype, subdiv_levels=0, material_type="MAKESKIN")
    ClothesService.update_delete_group(asset, human, delete_group_name=prefix + "_Hide_" + atype)
    obj.name = f"{prefix}_{atype}"
    mat, entry = material_from_file(asset.material, f"{prefix}_{atype}", tint, cutout)
    obj.data.materials.clear()
    obj.data.materials.append(mat)
    if atype == "Hair" and prefix in ("Elder", "VillagerSenior"):
        shader = mat.node_tree.nodes.get("Principled BSDF")
        for link in list(mat.node_tree.links):
            if link.to_socket == shader.inputs["Base Color"]: mat.node_tree.links.remove(link)
        shader.inputs["Base Color"].default_value = tint
        entry["texture"] = ""  # Grey opaque hair cards; no dark diffuse multiplying grey.
        entry["cutout"] = False
    if atype == "Clothes" and (filename.startswith("female_casual") or filename == "male_casualsuit06.mhclo"):
        # Plain original fabric colours: avoid the bundled MakeHuman logo on T-shirts.
        shader = mat.node_tree.nodes.get("Principled BSDF")
        for link in list(mat.node_tree.links):
            if link.to_socket == shader.inputs["Base Color"]: mat.node_tree.links.remove(link)
        shader.inputs["Base Color"].default_value = tint
        entry["texture"] = ""
        jeans = mat.copy()
        jeans.name = prefix + "_Trousers"
        jeans.node_tree.nodes.get("Principled BSDF").inputs["Base Color"].default_value = (.065,.12,.18,1)
        jeans.diffuse_color = (.065,.12,.18,1)
        obj.data.materials.append(jeans)
        waist = max(v.co.z for v in obj.data.vertices) * .57
        for poly in obj.data.polygons:
            if sum(obj.data.vertices[v].co.z for v in poly.vertices) / len(poly.vertices) < waist:
                poly.material_index = 1
        entry["extra"] = {"name": jeans.name, "texture": "", "color": [.065,.12,.18,1], "cutout": False}
    if atype == "Clothes":
        # A few millimetres of fit clearance prevents retargeted skin intersecting
        # the garment's boundary after four-weight Unity skinning.
        obj.data.update()
        for vertex in obj.data.vertices: vertex.co += vertex.normal * .003
    return obj, entry


# Body phenotype is deliberately varied; no chibi targets or stylized head scaling.
# Colours are role identifiers, not any institution's uniform or logo.
characters = [
    dict(id="Coordinator", gender=1, age=.53, height=.57, muscle=.58, weight=.53, clothes="male_worksuit01", hair="short01", tint=(.95,.72,.34,1)),
    dict(id="Ranger", gender=1, age=.48, height=.65, muscle=.69, weight=.43, clothes="male_casualsuit05", hair="short02", tint=(.53,.68,.40,1)),
    dict(id="Elder", gender=1, age=.94, height=.44, muscle=.35, weight=.48, clothes="male_casualsuit01", hair="short01", tint=(.86,.76,.57,1)),
    dict(id="Volunteer", gender=0, age=.32, height=.53, muscle=.55, weight=.44, clothes="female_casualsuit01", hair="bob01", tint=(1,.57,.26,1)),
    dict(id="ResidentWoman", gender=0, age=.62, height=.42, muscle=.38, weight=.63, clothes="female_casualsuit01", hair="long01", tint=(.67,.73,.96,1)),
    dict(id="VillagerMan", gender=1, age=.38, height=.59, muscle=.52, weight=.59, clothes="male_casualsuit03", hair="short02", tint=(.64,.85,.92,1)),
    dict(id="VillagerWoman", gender=0, age=.43, height=.49, muscle=.42, weight=.49, clothes="female_casualsuit02", hair="bob02", tint=(.86,.53,.67,1)),
    dict(id="VillagerSenior", gender=1, age=.83, height=.47, muscle=.36, weight=.59, clothes="male_casualsuit05", hair="short01", tint=(.64,.73,.87,1)),
]

bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)
if args.probe:
    macro = TargetService.get_default_macro_info_dict()
    macro.update({"gender": 1.0, "age": .9, "height": .48})
    human = HumanService.create_human(macro_detail_dict=macro)
    rig = HumanService.add_builtin_rig(human, "mixamo_unity")
    print("RIMBARA_PROBE", human.name, len(human.data.vertices), len(rig.data.bones))
    bpy.ops.wm.save_as_mainfile(filepath=str(output / "probe.blend"))
    sys.exit(0)

manifest = {"generator": "Tools/Generate-HumanNPCs.py", "mpfb": "v2.0.18", "characters": []}
completed = []
for index, spec in enumerate([] if args.preview_only else characters):
    # Hide other source characters while saving each independently editable file.
    for obj in list(bpy.data.objects):
        bpy.data.objects.remove(obj, do_unlink=True)
    macro = TargetService.get_default_macro_info_dict()
    macro.update({key: spec[key] for key in ("gender", "age", "height", "muscle", "weight")})
    macro["race"] = {"asian": .8, "caucasian": .12, "african": .08}
    macro["proportions"] = .5
    human = HumanService.create_human(macro_detail_dict=macro)
    human.name = spec["id"] + "_Body"
    rig = HumanService.add_builtin_rig(human, "mixamo_unity")
    rig.name = spec["id"]
    sex = "male" if spec["gender"] else "female"
    age = "old" if spec["age"] > .8 else "middleage" if spec["age"] > .5 else "young"
    skin = find_file("skins", f"{age}_asian_{sex}.mhmat")
    mat, entry = material_from_file(skin, spec["id"] + "_Skin")
    human.data.materials.clear()
    human.data.materials.append(mat)
    entries = [entry]
    for subdir, fname, atype, tint, alpha in [
        ("eyes", "low-poly.mhclo", "Eyes", (1,1,1,1), False),
        ("eyebrows", "eyebrow001.mhclo", "Eyebrows", (.3,.28,.24,1) if age == "old" else (.12,.10,.09,1), True),
        ("hair", spec["hair"] + ".mhclo", "Hair", (.83,.81,.76,1) if age == "old" else (.18,.15,.12,1), True),
        ("clothes", spec["clothes"] + ".mhclo", "Clothes", spec["tint"], False),
        ("clothes", "shoes01.mhclo", "Shoes", (.55,.48,.4,1), False),
    ]:
        _, asset_entry = add_asset(human, subdir, fname, atype, spec["id"], tint, alpha)
        entries.append(asset_entry)
        if "extra" in asset_entry: entries.append(asset_entry.pop("extra"))
    # Clothes' deletion masks hide covered skin, preserving body under source clothing
    # while avoiding skin bleed-through on the exported animated character.
    bpy.ops.file.pack_all()
    bpy.ops.wm.save_as_mainfile(filepath=str(sources / (spec["id"] + ".blend")))
    export_root = ExportService.create_character_copy(human, name_suffix="_export")
    export_human = ObjectService.find_object_of_type_amongst_nearest_relatives(export_root, "Basemesh")
    TargetService.bake_targets(export_human)
    ExportService.bake_modifiers_remove_helpers(export_human, bake_masks=True, bake_subdiv=False, remove_helpers=True, also_proxy=False)
    hierarchy = [export_root] + ObjectService.get_list_of_children(export_root)
    for obj in hierarchy:
        if obj.type == "MESH":
            for modifier in list(obj.modifiers):
                if modifier.type == "SUBSURF": obj.modifiers.remove(modifier)
    pose = json.loads((Path(args.mpfb) / "src/mpfb/data/poses/mixamo_unity_fk/t-pose.json").read_text())
    bpy.ops.object.select_all(action="DESELECT")
    export_root.select_set(True)
    bpy.context.view_layer.objects.active = export_root
    bpy.ops.object.mode_set(mode="POSE")
    RigService.set_pose_from_dict(export_root, pose)
    bpy.ops.object.mode_set(mode="OBJECT")
    RigService.apply_pose_as_rest_pose(export_root)
    # Normalize height including footwear, using mesh data (not armature bounds).
    bpy.context.view_layer.update()
    points = [obj.matrix_world @ Vector(c) for obj in hierarchy if obj.type == "MESH" for c in obj.bound_box]
    height = max(p.z for p in points) - min(p.z for p in points)
    if not 1.2 < height < 2.2:
        raise RuntimeError(f"Unexpected human size {spec['id']}: {height}")
    desired_height = [1.75, 1.82, 1.68, 1.66, 1.60, 1.76, 1.64, 1.69][index]
    export_root.scale *= desired_height / height
    height = desired_height
    bpy.ops.object.select_all(action="DESELECT")
    for obj in hierarchy: obj.select_set(True)
    bpy.context.view_layer.objects.active = export_root
    bpy.ops.export_scene.fbx(filepath=str(output / (spec["id"] + ".fbx")), use_selection=True,
        object_types={"MESH", "ARMATURE"}, add_leaf_bones=False, bake_anim=False,
        axis_forward="-Z", axis_up="Y", apply_unit_scale=True, path_mode="STRIP")
    triangles = sum(sum(len(p.vertices)-2 for p in obj.data.polygons) for obj in hierarchy if obj.type == "MESH")
    manifest["characters"].append({"id": spec["id"], "height": height, "triangles": triangles,
        "phenotype": macro, "clothes": spec["clothes"], "hair": spec["hair"], "materials": entries})
    # Keep only the baked clothed game mesh in the preview library.
    for obj in list(bpy.data.objects):
        if obj not in hierarchy: bpy.data.objects.remove(obj, do_unlink=True)
    bpy.ops.wm.save_as_mainfile(filepath=str(output.parent / (spec["id"] + "-preview.blend")))
    completed.append(output.parent / (spec["id"] + "-preview.blend"))
    print("RIMBARA_CHARACTER", spec["id"], "height", round(height,3), "triangles", triangles, flush=True)

if not args.preview_only:
    (output / "manifest.json").write_text(json.dumps(manifest, indent=2), encoding="utf-8")
    (output / "source-hashes.json").write_text(json.dumps(provenance, indent=2), encoding="utf-8")
else:
    completed = [output.parent / (spec["id"] + "-preview.blend") for spec in characters]
if args.preview:
    bpy.ops.wm.save_as_mainfile(filepath=str(sources / "NPC-Lineup.blend"))
    for obj in list(bpy.data.objects): bpy.data.objects.remove(obj, do_unlink=True)
    for index, path in enumerate(completed):
        with bpy.data.libraries.load(str(path), link=False) as (data_from, data_to):
            data_to.objects = list(data_from.objects)
        for obj in data_to.objects:
            if obj: bpy.context.collection.objects.link(obj)
        root = next(obj for obj in data_to.objects if obj and obj.type == "ARMATURE")
        root.location.x = index * 1.65
        # Relax arms only for the contact sheet, not exported bind pose.
        for side, angle in (("Left", -1.15), ("Right", 1.15)):
            bone = root.pose.bones["mixamorig:" + side + "Arm"]
            bone.rotation_mode = "XYZ"
            bone.rotation_euler.z = angle
    bpy.ops.object.camera_add(location=(5.8,-16,4.3))
    camera = bpy.context.object
    camera.rotation_euler = (Vector((5.8,0,1))-camera.location).to_track_quat("-Z", "Y").to_euler()
    camera.data.type = "ORTHO"
    camera.data.ortho_scale = 13.7
    bpy.context.scene.camera = camera
    for location, energy, size in [((3,-6,7),1800,8),((9,2,6),1500,6)]:
        bpy.ops.object.light_add(type="AREA", location=location)
        light = bpy.context.object
        light.data.energy = energy
        light.data.shape = "DISK"
        light.data.size = size
        light.rotation_euler = (Vector((5.8,0,1))-light.location).to_track_quat("-Z", "Y").to_euler()
    scene = bpy.context.scene
    scene.render.engine = "CYCLES"
    scene.cycles.samples = 24
    scene.world.color = (.25,.25,.25)
    scene.render.resolution_x = 1920
    scene.render.resolution_y = 700
    scene.render.resolution_percentage = 100
    scene.render.filepath = args.preview
    bpy.ops.wm.save_as_mainfile(filepath=str(sources / "NPC-Lineup.blend"))
    bpy.ops.render.render(write_still=True)
print("RIMBARA_PREVIEW_COMPLETE" if args.preview_only else "RIMBARA_EXPORT_COMPLETE", len(characters) if args.preview_only else len(manifest["characters"]), flush=True)
