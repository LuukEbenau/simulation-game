using Godot;
using SacaSimulationGame.scripts.managers;
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

        private MapDataItem[,] TerrainGradients;
        private Vector3I CellSize;
        private Gradient ColorRamp;
        private float VisualHeight = 1.15f;
        private float Transparency = 0.25f;

        private WorldMapManager MapManager = null;

        public override void _Ready()
        {
            ColorRamp = CreateColorRamp();
        }

        public void SetGradients(MapDataItem[,] gradients, Vector3I cellSize, WorldMapManager mapManager)
        {
            this.MapManager = mapManager;
            TerrainGradients = gradients;
            CellSize = cellSize;
            CreateVisualization();
        }

        private void CreateVisualization()
        {
            var xLen = TerrainGradients.GetLength(0);
            var yLen = TerrainGradients.GetLength(1);
            for (int x = 0; x < xLen; x++) {
                for (int y = 0; y < yLen; y++)
                {
                    Color color;
                    var cellData = TerrainGradients[x, y];
                    if(cellData == null)
                    {
                        GD.Print($"content oc cell {x},{y} is null");
                        color = new Color(1, 1, 1);
                    }
                    else if (cellData.CellType.HasFlag(CellType.WATER))
                    {
                        color = new Color(0, 0, 1);
                    }
                    else
                    {
                        float slope = cellData.Slope;
                        color = GetColorForAngle(slope);
                    }

                    CreateCellVisual(x,y, color);
                }
            }
            //foreach (var cell in TerrainGradients.Keys)
            //{

            //}
            UpdateVisibility();
        }

        private void CreateCellVisual(int x, int y, Color color)
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

            //MapManager.CellToWorldInterpolated(new Vector2(x + 0.5f, y + 0.5f))
            //n.ToVec3World();
            meshInstance.Position = MapManager.CellToWorldInterpolated(new Vector2(x + 0.5f, y + 0.5f), VisualHeight);// new Vector3(x * CellSize.X, , y * CellSize.Z);
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