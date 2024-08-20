using Godot;
using Godot.Collections;
using SacaSimulationGame.scripts.buildings;
using SacaSimulationGame.scripts.buildings.dataStructures.blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public partial class BottomMenu : CanvasLayer
{
    private ItemList MenuItems { get; set; }

    public bool IsHovered { get; private set; } = false;

    [Signal]
    public delegate void MenuSelectionChangedEventHandler(BuildingType buildingType);

    //private List<BuildingMenuItem> AvailableItems { get; set; }
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.MenuItems = GetNode<ItemList>("MenuItems");
        MenuItems.ItemSelected += MenuItems_ItemSelected;
    }

    private void MenuItems_ItemSelected(long index)
    {
        //var selectedItem = MenuItems.GetSelectedItems().FirstOrDefault();
        GD.Print($"changing selection to index {index}");
        ChangeSelection((int)index);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (GetViewport().GuiGetHoveredControl() != null)
        {
            IsHovered = true;
        }
        else IsHovered = false;
    }
    //public override void _UnhandledInput(InputEvent @event)
    //{
    //    // If the mouse is over any GUI element, return early and do not process 3D input
    //    if (((object)GetViewport().GuiGetDragData()) != null || )
    //    {
    //        // This ensures no input events propagate to the 3D world while GUI is interacted with
    //        GD.Print($"capturing event bc of hover over gui");
    //        return;
    //    }

    //}
    private void ChangeSelection(int index)
    {
        if (index == -1)
        {
            this.EmitSignal(SignalName.MenuSelectionChanged, (int)BuildingType.None);
        }
        else
        {
            var name = MenuItems.GetItemText(index);

            BuildingType bt = name switch
            {
                "House" => BuildingType.House,
                "Road" => BuildingType.Road,
                "Lumberjack" => BuildingType.Lumberjack,
                "Stockpile" => BuildingType.Stockpile,
                "Fishing Post" => BuildingType.FishingPost,
                "Bridge" => BuildingType.Bridge,
                "Stone Mine" => BuildingType.StoneMine,
                _ => throw new ArgumentOutOfRangeException(nameof(name), $"not implemented value {name} in menuitems")
            };
            
            this.EmitSignal(SignalName.MenuSelectionChanged, (int)bt);
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("Action Slot 1"))
        {
            MenuItems.Select(0);
            ChangeSelection(0);
        }
        else if (@event.IsActionPressed("Action Slot 2"))
        {
            MenuItems.Select(1);
            ChangeSelection(1);
        }
        else if (@event.IsActionPressed("Action Slot 3"))
        {
            MenuItems.Select(2);
            ChangeSelection(2);
        }
        else if (@event.IsActionPressed("Action Slot 4"))
        {
            MenuItems.Select(3);
            ChangeSelection(3);
        }
        else if (@event.IsActionPressed("Action Slot 5"))
        {
            MenuItems.Select(4);
            ChangeSelection(4);
        }
        else if (@event.IsActionPressed("Action Slot 6"))
        {
            MenuItems.Select(5);
            ChangeSelection(5);
        }
        else if (@event.IsActionPressed("Cancel Selection"))
        {
            MenuItems.DeselectAll();
            ChangeSelection(-1);
        }
    }
}




//public class BuildingMenuItem
//{
//    public string Name { get; set; }
//    public Texture2D Icon { get; set; }
//    public BuildingBlueprintBase blueprint { get; set; }
//    //AvailableItems = [
//    //    new BuildingMenuItem{
//    //        Name = "House",
//    //        Icon = (Texture2D)ResourceLoader.Load("res://assets/buildings/models/building/house/icon.png"),
//    //        blueprint = new HouseBlueprint()
//    //    },
//    //    new BuildingMenuItem{
//    //        Name = "Road",
//    //        Icon = (Texture2D)ResourceLoader.Load("res://assets/buildings/models/building/road/icon.png"),
//    //        blueprint = new HouseBlueprint()
//    //    },
//    //    new BuildingMenuItem{
//    //        Name = "Lumberjack",
//    //        Icon = (Texture2D)ResourceLoader.Load("res://assets/buildings/models/building/lumberjack/icon.png"),
//    //        blueprint = new LumberjackBlueprint()
//    //    },
//    //    new BuildingMenuItem{
//    //        Name = "Stockpile",
//    //        Icon = (Texture2D)ResourceLoader.Load("res://assets/buildings/models/building/stockpile/icon.png"),
//    //        blueprint = new StockpileBlueprint()
//    //    },
//    //    new BuildingMenuItem{
//    //        Name = "Fishingpost",
//    //        Icon = (Texture2D)ResourceLoader.Load("res://assets/buildings/models/building/fishingpost/icon.png"),
//    //        blueprint = new FishingPostBlueprint()
//    //    },
//    //    new BuildingMenuItem{
//    //        Name = "Bridge",
//    //        Icon = (Texture2D)ResourceLoader.Load("res://assets/buildings/models/building/bridge/icon.png"),
//    //        blueprint = new BridgeBlueprint()
//    //    },
//    //    ];

//}