namespace GodotUtils.World2D.TopDown;

/*
 * Attach this script to a child node of the Camera node
 */
public partial class CameraController : Node
{
    // Inspector
    [Export] 
    private float Speed { get; set; } = 100;

    [ExportGroup("Zoom")]
    [Export(PropertyHint.Range, "0.02, 0.16")] 
    private float ZoomIncrementDefault { get; set; } = 0.02f;

    [Export(PropertyHint.Range, "0.01, 1")] 
    private float MinZoom { get; set; } = 0.01f;

    [Export(PropertyHint.Range, "0.1, 10")] 
    private float MaxZoom { get; set; } = 1.0f;

    [Export(PropertyHint.Range, "0.01, 1")] 
    private float SmoothFactor { get; set; } = 0.25f;

    private float zoomIncrement = 0.02f;
    private float targetZoom;

    // Panning
    private Vector2 initialPanPosition;
    private bool panning;
    private Camera2D camera;

    public override void _Ready()
    {
        camera = GetParent<Camera2D>();

        // Set the initial target zoom value on game start
        targetZoom = camera.Zoom.X;
    }

    public override void _Process(double delta)
    {
        // Not sure if the below code should be in _PhysicsProcess or _Process

        // Arrow keys and WASD move camera around
        var dir = Vector2.Zero;

        if (GInput.IsMovingLeft())
            dir.X -= 1;

        if (GInput.IsMovingRight())
            dir.X += 1;

        if (GInput.IsMovingUp())
            dir.Y -= 1;

        if (GInput.IsMovingDown())
            dir.Y += 1;
        
        if (panning)
            camera.Position = initialPanPosition - (GetViewport().GetMousePosition() / camera.Zoom.X);

        // Arrow keys and WASD movement are added onto the panning position changes
        camera.Position += dir.Normalized() * Speed;
    }

    public override void _PhysicsProcess(double delta)
    {
        // Prevent zoom from becoming too fast when zooming out
        zoomIncrement = ZoomIncrementDefault * camera.Zoom.X;

        // Lerp to the target zoom for a smooth effect
        camera.Zoom = camera.Zoom.Lerp(new Vector2(targetZoom, targetZoom), SmoothFactor);
    }

    // Not sure if this should be done in _Input or _UnhandledInput
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
            InputEventMouseButton(mouseButton);
    }

    private void InputEventMouseButton(InputEventMouseButton @event)
    {
        HandlePan(@event);
        HandleZoom(@event);
    }

    private void HandlePan(InputEventMouseButton @event)
    {
        // Left click to start panning the camera
        if (@event.ButtonIndex != MouseButton.Left)
            return;
        
        // Is this the start of a left click or is this releasing a left click?
        if (@event.IsPressed())
        {
            // Save the intial position
            initialPanPosition = camera.Position + (GetViewport().GetMousePosition() / camera.Zoom.X);
            panning = true;
        }
        else
            // Only stop panning once left click has been released
            panning = false;
    }

    private void HandleZoom(InputEventMouseButton @event)
    {
        // Not sure why or if this is required
        if (!@event.IsPressed())
            return;

        // Zoom in
        if (@event.ButtonIndex == MouseButton.WheelUp)
            targetZoom += zoomIncrement;

        // Zoom out
        if (@event.ButtonIndex == MouseButton.WheelDown)
            targetZoom -= zoomIncrement;

        // Clamp the zoom
        targetZoom = Mathf.Clamp(targetZoom, MinZoom, MaxZoom);
    }
}
