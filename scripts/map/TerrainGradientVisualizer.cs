using Godot;
using System;
using System.Collections.Generic;
namespace SacaSimulationGame.scripts.map
{
    public partial class TerrainGradientVisualizer : Node3D
    {
        [Export]
        public bool ShowSlopeGradients
        {
            get => _showSlopeGradients;
            set
            {
                _showSlopeGradients = value;
                UpdateVisibility();
            }
        }
        private bool _showSlopeGradients = true;

        private Dictionary<Vector2I, MapDataItem> TerrainGradients = new Dictionary<Vector2I, MapDataItem>();
        private Vector3I CellSize;
        private Gradient ColorRamp;
        private float VisualHeight = 1.15f;
        private float Transparency = 0.25f;

        public override void _Ready()
        {
            ColorRamp = CreateColorRamp();
        }

        public void SetGradients(Dictionary<Vector2I, MapDataItem> gradients, Vector3I cellSize)
        {
            TerrainGradients = gradients;
            CellSize = cellSize;
            CreateVisualization();
        }

        private void CreateVisualization()
        {
            foreach (var cell in TerrainGradients.Keys)
            {
                Color color;
                if (TerrainGradients[cell].CellType.HasFlag(CellType.WATER))
                {
                    color = new Color(0, 0, 1);
                }
                else
                {
                    float slope = TerrainGradients[cell].Slope;
                    color = GetColorForAngle(slope);
                }

                CreateCellVisual(cell, color);
            }
            UpdateVisibility();
        }

        private void CreateCellVisual(Vector2I cell, Color color)
        {
            var meshInstance = new MeshInstance3D();
            var planeMesh = new PlaneMesh();
            planeMesh.Size = new Vector2(CellSize.X, CellSize.Z);
            meshInstance.Mesh = planeMesh;

            var material = new StandardMaterial3D();
            material.AlbedoColor = color;
            material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
            //material.TransparentEnabled = true;
            material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
            material.CullMode = BaseMaterial3D.CullModeEnum.Disabled;
            meshInstance.SetSurfaceOverrideMaterial(0, material);

            meshInstance.Position = new Vector3(cell.X * CellSize.X, VisualHeight, cell.Y * CellSize.Z);
            AddChild(meshInstance);
        }

        private Color GetColorForAngle(float angle)
        {
            angle = Mathf.Clamp(angle, 0, 90);
            float t = angle / 90.0f;
            Color color = ColorRamp.Sample(t);
            color.A = Transparency;
            return color;
        }

        private Gradient CreateColorRamp()
        {
            var gradient = new Gradient();
            gradient.SetColor(0, new Color(0, 1, 0, 1));  // Bright green
            gradient.AddPoint(0.5f, new Color(1, 1, 0, 1));  // Bright yellow
            gradient.SetColor(1, new Color(1, 0, 0, 1));  // Bright red
            return gradient;
        }

        private void UpdateVisibility()
        {
            foreach (var child in GetChildren())
            {
                if (child is MeshInstance3D meshInstance)
                {
                    meshInstance.Visible = ShowSlopeGradients;
                }
            }
        }
    }
}