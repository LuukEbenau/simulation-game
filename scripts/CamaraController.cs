using Godot;
using System;

public partial class CamaraController : Node3D
{
    [Export] public float MoveSpeed { get; set; } = 75.0f;
    private float _moveSpeedDynamic = 75.0f;
    [Export] public float ZoomSpeed { get; set; } = 5.0f;
    [Export] public float MinZoom { get; set; } = 6.0f;
    [Export] public float MaxZoom { get; set; } = 190.0f;
    [Export] public float BorderThreshold { get; set; } = 50.0f;
    [Export] public float MaxAngle { get; set; } = -85.0f;  // Angle when fully zoomed out
    [Export] public float MinAngle { get; set; } = -12.0f;  // Angle when fully zoomed in

    private Vector2 _viewportSize;
    private Camera3D _camera;
    private Node3D _map;

    public override void _Ready()
    {
        GD.Print("loading camera");
        _viewportSize = GetViewport().GetVisibleRect().Size;
        _camera = GetNode<Camera3D>("Camera3D");
        _map = GetParent().GetNode<Node3D>("Map");

        if (_camera == null)
            GD.PushError("Camera3D node not found in CameraWrap");
        if (_map == null)
            GD.PushError("Map node not found as sibling of CameraWrap");

        GlobalPosition = new Vector3(GlobalPosition.X, _map.GlobalPosition.Y + MaxZoom, GlobalPosition.Z);
        UpdateCameraAngle();
    }

    public override void _Process(double delta)
    {
        Vector2 mousePos = GetViewport().GetMousePosition();
        Vector3 moveDir = Vector3.Zero;

        // Check if mouse is near the borders
        if (mousePos.X < BorderThreshold || Input.IsActionPressed("Move Camera Left"))
            moveDir.X -= 1;
        else if (mousePos.X > _viewportSize.X - BorderThreshold || Input.IsActionPressed("Move Camera Right"))
            moveDir.X += 1;

        if (mousePos.Y < BorderThreshold || Input.IsActionPressed("Move Camera Top"))
            moveDir.Z -= 1;
        else if (mousePos.Y > _viewportSize.Y - BorderThreshold || Input.IsActionPressed("Move Camera Bottom"))
            moveDir.Z += 1;

        MoveScreen(moveDir, (float)delta);
    }

    private void MoveScreen(Vector3 dir, float delta = 1.0f)
    {
        if (dir != Vector3.Zero)
        {
            dir = dir.Normalized();
            GlobalTranslate(dir * _moveSpeedDynamic * delta);
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("Zoom Out Camera"))
            ZoomCamera(-1);
        else if (@event.IsActionPressed("Zoom In Camera"))
            ZoomCamera(1);
    }

    private void ZoomCamera(int direction)
    {
        if (_camera == null || _map == null)
            return;

        Vector3 newPosition = GlobalPosition + GlobalTransform.Basis.Y * direction * ZoomSpeed;
        float mapPosY = _map.GlobalPosition.Y;

        // Check if zooming in would go below the map's y-coordinate
        if (direction < 0 && newPosition.Y <= mapPosY + MinZoom)
            return;

        float distanceToOrigin = Math.Abs(newPosition.Y - mapPosY);

        // Check if zooming out would exceed the max zoom distance
        if (direction > 0 && distanceToOrigin > MaxZoom)
            return;

        GlobalTranslate(GlobalTransform.Basis.Y * direction * ZoomSpeed);
        UpdateCameraAngle();
    }

    private void UpdateCameraAngle()
    {
        float currentZoom = GlobalPosition.Y - (_map.GlobalPosition.Y + MinZoom);
        float zoomRange = MaxZoom - MinZoom;

        if (zoomRange == 0)
            return;

        float percentZoom = currentZoom / zoomRange;
        percentZoom = Mathf.Clamp(percentZoom, 0, 1);
        _moveSpeedDynamic = 5 + MoveSpeed * percentZoom;

        float easedPercent = Ease(percentZoom, 2.2f);
        float targetAngle = Mathf.Lerp(MinAngle, MaxAngle, easedPercent);
        _camera.RotationDegrees = new Vector3(targetAngle, _camera.RotationDegrees.Y, _camera.RotationDegrees.Z);
    }

    private float Ease(float x, float curve)
    {
        return 1.0f - Mathf.Pow(1.0f - x, curve);
    }
}

