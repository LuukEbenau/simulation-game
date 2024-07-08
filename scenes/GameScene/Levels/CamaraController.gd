extends Node3D

@export var move_speed : float = 30.0
@export var zoom_speed : float = 5
@export var min_zoom : float = 9.0
@export var max_zoom : float = 150.0
@export var border_threshold : float = 50.0

@export var max_angle : float = -85.0  # Angle when fully zoomed out
@export var min_angle : float = -14.0  # Angle when fully zoomed in

var viewport_size : Vector2
var camera : Camera3D
var map : Node3D

func _ready():
    viewport_size = get_viewport().size
    camera = $Camera3D
    map = get_parent().get_node("Map")

    if not camera:
        push_error("Camera3D node not found in CameraWrap")
    if not map:
        push_error("Map node not found as sibling of CameraWrap")

    global_position.y = map.global_position.y + max_zoom
    update_camera_angle()

func _process(delta):
    var mouse_pos = get_viewport().get_mouse_position()
    var move_dir = Vector3.ZERO

    # Check if mouse is near the borders
    if mouse_pos.x < border_threshold:
        move_dir.x -= 1
    elif mouse_pos.x > viewport_size.x - border_threshold:
        move_dir.x += 1

    if mouse_pos.y < border_threshold:
        move_dir.z -= 1
    elif mouse_pos.y > viewport_size.y - border_threshold:
        move_dir.z += 1

    # Normalize and apply movement
    if move_dir != Vector3.ZERO:
        move_dir = move_dir.normalized()
        global_translate(move_dir * move_speed * delta)

func _input(event):
    if event is InputEventMouseButton:
        if event.button_index == MOUSE_BUTTON_WHEEL_UP:
            zoom_camera(-1)
        elif event.button_index == MOUSE_BUTTON_WHEEL_DOWN:
            zoom_camera(1)

func zoom_camera(direction):
    if not camera or not map:
        return

    var new_position = global_position + global_transform.basis.y * direction * zoom_speed

    var mapPosY = map.global_position.y


    #var distance_to_origin = new_position.distance_to(global_transform.origin)

    # Check if zooming in would go below the map's y-coordinate
    if direction < 0 and new_position.y <= mapPosY + min_zoom:
        return

    var distance_to_origin = abs(new_position.y - mapPosY)
    # Check if zooming out would exceed the max zoom distance
    if direction > 0 and distance_to_origin > max_zoom:
        return

    global_translate(global_transform.basis.y * direction * zoom_speed)
    update_camera_angle()

func update_camera_angle():
    var current_zoom = global_position.y - (map.global_position.y + min_zoom)
    var zoom_range = max_zoom - min_zoom

    if zoom_range == 0:
        return

    var percent_zoom = current_zoom / zoom_range
    percent_zoom = clamp(percent_zoom, 0, 1)

    var eased_percent = ease(1 - percent_zoom, 3)
    var target_angle = lerp(min_angle, max_angle, 1 - eased_percent)

    camera.rotation_degrees.x = target_angle

func ease(x: float, curve: float) -> float:
    return 1.0 - pow(1.0 - x, curve)
