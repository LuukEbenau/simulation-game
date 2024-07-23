using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.naturalResources;
using SacaSimulationGame.scripts.units;
using SacaSimulationGame.scripts.units.dataObjects;

namespace SacaSimulationGame.scripts.buildings
{
    public partial class BuildingResources : Node3D
    {
        public float PercentageResourcesAquired => (CurrentWood + CurrentStone + 1) / (RequiredWood + RequiredStone + 1);
        public bool RequiresResources => PercentageResourcesAquired < 1;
        
        [Export] public float RequiredWood { get; set; }
        public float CurrentWood { get; private set; }
        [Export] public float RequiredStone { get; set; }
        public float CurrentStone { get; private set; }

        public ResourceType TypesOfResourcesRequired { get; set; }

        private List<Node3D> _woodIndicatorNodes;
        private List<Node3D> _stoneIndicatorNodes;
        private Node3D _woodWrap;
        private Node3D _stoneWrap;

        public void ShowResources(bool visible)
        {
            if (_woodWrap != null)
            {
                _woodWrap.Visible = visible;
            }
            if (_stoneWrap != null)
            {
                _stoneWrap.Visible = visible;
            }
        }

        public override void _Ready()
        {
            base._Ready();
            if (this.RequiredWood > 0) TypesOfResourcesRequired |= ResourceType.Wood;
            if (this.RequiredStone > 0) TypesOfResourcesRequired |= ResourceType.Stone;
            
            _woodWrap = GetNodeOrNull<Node3D>("Wood");
            if (_woodWrap != null)
            {
                _woodIndicatorNodes = _woodWrap.GetChildren().Select(c => c as Node3D).ToList();
            }
            _stoneWrap = GetNodeOrNull<Node3D>("Stone");
            if (_stoneWrap != null)
            {
                _stoneIndicatorNodes = _stoneWrap.GetChildren().Select(c => c as Node3D).ToList();
            }
        }

        private void UpdateResourceVisual() {
            if (_woodIndicatorNodes != null && RequiredWood > 0) {
                
                var percentWood = CurrentWood / RequiredWood;
                var nrOfIndicators = Mathf.FloorToInt(percentWood * _woodIndicatorNodes.Count);

                for (int i = 0; i < _woodIndicatorNodes.Count; i++)
                {
                    var node = _woodIndicatorNodes[i];
                    node.Visible = i <= nrOfIndicators;
                }
            }

            if (_stoneIndicatorNodes != null && RequiredStone > 0)
            {

                var percent = CurrentStone / RequiredStone;
                var nrOfIndicators = Mathf.FloorToInt(percent * _stoneIndicatorNodes.Count);

                for (int i = 0; i < _stoneIndicatorNodes.Count; i++)
                {
                    var node = _stoneIndicatorNodes[i];
                    node.Visible = i <= nrOfIndicators;
                }
            }

        }

        public float RequiresOfResource(ResourceType resourceType)
        {
            if (resourceType == ResourceType.Wood)
            {
                return  RequiredWood - CurrentWood;
            }
            else if (resourceType == ResourceType.Stone)
            {
                return RequiredStone - CurrentStone;
            }
            else throw new Exception($"Resource type {resourceType} not implemented");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceType">1 single resource type</param>
        /// <param name="amount"></param>
        /// <returns>Amount of leftover of the resource</returns>
        public float AddResource(ResourceType resourceType, float amount)
        {
            if (resourceType == ResourceType.Wood)
            {
                var spaceLeft = RequiredWood - CurrentWood;
                if (amount < spaceLeft)
                {
                    CurrentWood += amount;
                }
                else
                {
                    var leftover = amount - spaceLeft;
                    CurrentWood += spaceLeft;

                    TypesOfResourcesRequired &= ~resourceType;

                    UpdateResourceVisual();
                    return leftover;
                }
            }

            else if (resourceType == ResourceType.Stone)
            {
                var spaceLeft = RequiredStone - CurrentStone;
                if (amount < spaceLeft)
                {
                    CurrentStone += amount;
                }
                else
                {
                    var leftover = amount - spaceLeft;
                    CurrentStone += spaceLeft;

                    TypesOfResourcesRequired &= ~resourceType;

                    UpdateResourceVisual();
                    return leftover;
                }
            }
            else
            {
                throw new Exception($"unknown resource type {resourceType}");
            }

            UpdateResourceVisual();
            return 0;
        }
    }
}
