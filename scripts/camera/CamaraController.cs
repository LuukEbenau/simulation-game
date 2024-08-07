using Godot;
using SacaSimulationGame.scripts.managers;
using System;
using System.Threading.Tasks;



public partial class CamaraController : Node3D
{
    [Export] public float MoveSpeed { get; set; } = 75.0f;
    private float _moveSpeedDynamic = 75.0f;
    [Export] public float ZoomSpeed { get; set; } = 5.0f;
    [Export] public float MinZoom { get; set; } = 6.0f;
    [Export] public float MaxZoom { get; set; } = 140.0f;
    /// <summary>
    /// MouseDistanceFromBorder
    /// </summary>
    [Export] public float MouseDistanceFromScreenBorderThreshhold { get; set; } = 50.0f;
    [Export] public float MaxAngle { get; set; } = -85.0f;
    [Export] public float MinAngle { get; set; } = -12.0f;

    private Vector2 _viewportSize;
    private Camera3D _camera;
    private Node3D _map;
    private Vector3 _minWorldCoord;
    private Vector3 _maxWorldCoord;

    private bool _isCinematicZooming = false;

    private GameManager GameManager { get; set; }
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

        this.GameManager = GetParent().GetNode<GameManager>("GameManager");

        StartCinematicZoomout();
    }

    private void StartCinematicZoomout()
    {
        _isCinematicZooming = true;

        var spawnPoint = GameManager.SpawnLocation.GlobalPosition;
        GlobalPosition = new Vector3(spawnPoint.X + 5, MinZoom, spawnPoint.Z + 30);
        UpdateCameraAngle();

    }

    private float EaseOutQuad(float t)
    {
        return t == 1.0f ? 1.0f : 1 - Mathf.Pow(2, -10 * t);
        //return t * (2 - t);
    }
    private void TickCinematicZoomout(double delta)
    {
        var finalZoomY = MaxZoom * 0.25f;
        var startZoomY = MinZoom;
        var totalZoomDistance = finalZoomY - startZoomY;
        var currentZoomY = GlobalPosition.Y - startZoomY;
        var zoomProgress = Mathf.Clamp((float)(currentZoomY / totalZoomDistance), 0.0f, 1.0f);

        if (GlobalPosition.Y >= finalZoomY)
        {
            _isCinematicZooming = false;
        }
        else
        {
            var easedProgress = EaseOutQuad(zoomProgress) + 0.15f;
            var cinematicZoomSpeedCoefficient = 7.5f * easedProgress;
            ZoomCamera((float)delta * cinematicZoomSpeedCoefficient);
        }
    }


    public override void _Process(double delta)
    {
        if (_isCinematicZooming)
        {
            TickCinematicZoomout(delta);
        }
        else
        {

            if (_minWorldCoord == default)
            {
                var borderBoundarySize = 5;
                var minCell = new Vector2I(borderBoundarySize, borderBoundarySize);
                var maxCell = new Vector2I(this.GameManager.MapManager.MapWidth - borderBoundarySize, GameManager.MapManager.MapHeight - borderBoundarySize);
                _minWorldCoord = this.GameManager.MapManager.CellToWorld(minCell);
                _maxWorldCoord = this.GameManager.MapManager.CellToWorld(maxCell);
            }

            Vector2 mousePos = GetViewport().GetMousePosition();
            Vector3 moveDir = Vector3.Zero;

            //TODO: i want to prevent scrolling outside map borders. The borders of the map are defined as following:

            //var borderBoundarySize = 40;

            //var minCell = new Vector2I(this.GameManager.MapManager.MinX + borderBoundarySize, this.GameManager.MapManager.MinY + borderBoundarySize);
            //var maxCell = new Vector2I(this.GameManager.MapManager.MaxX - borderBoundarySize, GameManager.MapManager.MaxY - borderBoundarySize);
            //var minWorldCoord = this.GameManager.MapManager.CellToWorld(minCell);
            //var maxWorldCoord = this.GameManager.MapManager.CellToWorld(maxCell);

            // Check if mouse is near the borders
            if (mousePos.X < MouseDistanceFromScreenBorderThreshhold || Input.IsActionPressed("Move Camera Left"))
                moveDir.X -= 1;
            else if (mousePos.X > _viewportSize.X - MouseDistanceFromScreenBorderThreshhold || Input.IsActionPressed("Move Camera Right"))
                moveDir.X += 1;

            if (mousePos.Y < MouseDistanceFromScreenBorderThreshhold || Input.IsActionPressed("Move Camera Top"))
                moveDir.Z -= 1;
            else if (mousePos.Y > _viewportSize.Y - MouseDistanceFromScreenBorderThreshhold || Input.IsActionPressed("Move Camera Bottom"))
                moveDir.Z += 1;

            MoveScreen(moveDir, (float)delta);
        }
    }

    private void MoveScreen(Vector3 dir, float delta = 1.0f)
    {
        if (dir != Vector3.Zero)
        {
            dir = dir.Normalized();
            Vector3 newPosition = GlobalPosition + dir * _moveSpeedDynamic * delta;

            // Clamp the new position within the map boundaries
            newPosition.X = Mathf.Clamp(newPosition.X, _minWorldCoord.X, _maxWorldCoord.X);
            newPosition.Z = Mathf.Clamp(newPosition.Z, _minWorldCoord.Z, _maxWorldCoord.Z);

            // Only move if the new position is different
            if (newPosition != GlobalPosition)
            {
                GlobalPosition = newPosition;
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (_isCinematicZooming) return;

        if (@event.IsActionPressed("Zoom Out Camera"))
            ZoomCamera(-1);
        else if (@event.IsActionPressed("Zoom In Camera"))
            ZoomCamera(1);
    }

    private void ZoomCamera(float direction)
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

