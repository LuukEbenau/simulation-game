using Godot;
using Godot.Collections;
using SacaSimulationGame.scripts.buildings;
using SacaSimulationGame.scripts.buildings.dataStructures.blueprints;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
public class BuildingMenuItem
{
    public string Name { get; set; }
    public Texture2D Icon { get; set; }
    public string Keybinding { get; set; }
    public string ActionName { get; set; }
    public BuildingType Type { get; set; }
}
public partial class BottomMenu : Control
{
    private ItemList MenuItems { get; set; }

    public bool IsHovered { get; private set; } = false;

    [Signal]
    public delegate void MenuSelectionChangedEventHandler(BuildingType buildingType);

    private List<BuildingMenuItem> AvailableItems { get; set; }
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.MenuItems = GetNode<ItemList>("MenuItems");
        this.AvailableItems = GenerateMenuItems();
        InitializeMenuItems(AvailableItems);
        //AddKeybindingsToTitles();
        MenuItems.ItemSelected += MenuItems_ItemSelected;
    }

    private void InitializeMenuItems(List<BuildingMenuItem> menuItems)
    {
        this.MenuItems.Clear();
        for(int i = 0; i < AvailableItems.Count; i++)
        {
            var item = AvailableItems[i];

            var placedIdx = MenuItems.AddItem(item.Name + $"\n{item.Keybinding}", item.Icon);

            GD.Print($"placing {i}th item at index {placedIdx}");
        }
    }

    private List<BuildingMenuItem> GenerateMenuItems()
    {
        GD.Print("Generating menu items");
        List<BuildingMenuItem> availableItems = new() {
            new BuildingMenuItem{
                Name = "House",
                Icon = (Texture2D) ResourceLoader.Load("res://assets/buildings/models/building/house/icon.png"),
                ActionName = "Build House",
                Keybinding = GetKeybindingsOfAction("Build House"),
                Type = BuildingType.House
            },
            new BuildingMenuItem
            {
                Name = "Road",
                Icon = (Texture2D)ResourceLoader.Load("res://assets/buildings/models/building/road/icon.png"),
                ActionName = "Build Road",
                Keybinding = GetKeybindingsOfAction("Build Road"),
                Type = BuildingType.Road
            },
            new BuildingMenuItem
            {
                Name = "Lumberjack",
                Icon = (Texture2D)ResourceLoader.Load("res://assets/buildings/models/building/lumberjack/icon.png"),
                ActionName = "Build Lumberjack",
                Keybinding = GetKeybindingsOfAction("Build Lumberjack"),
                Type = BuildingType.Lumberjack
            },
            new BuildingMenuItem
            {
                Name = "Stockpile",
                Icon = (Texture2D)ResourceLoader.Load("res://assets/buildings/models/building/stockpile/icon.png"),
                ActionName = "Build Stockpile",
                Keybinding = GetKeybindingsOfAction("Build Stockpile"),
                Type = BuildingType.Stockpile
            },
            new BuildingMenuItem
            {
                Name = "Fishing Post",
                Icon = (Texture2D)ResourceLoader.Load("res://assets/buildings/models/building/fishingpost/icon.png"),
                ActionName = "Build Fishing Post",
                Keybinding = GetKeybindingsOfAction("Build Fishing Post"),
                Type = BuildingType.FishingPost
            },
            new BuildingMenuItem
            {
                Name = "Bridge",
                Icon = (Texture2D)ResourceLoader.Load("res://assets/buildings/models/building/bridge/icon.png"),
                ActionName = "Build Bridge",
                Keybinding = GetKeybindingsOfAction("Build Bridge"),
                Type = BuildingType.Bridge
            },
            new BuildingMenuItem
            {
                Name = "Stone Mine",
                Icon = (Texture2D)ResourceLoader.Load("res://assets/buildings/models/building/stonemine/icon.png"),
                ActionName = "Build Stone Mine",
                Keybinding = GetKeybindingsOfAction("Build Stone Mine"),
                Type = BuildingType.StoneMine
            }
        };

        return availableItems;
    }

    private string GetKeybindingsOfAction(string action)
    {
        var events = InputMap.ActionGetEvents(action);

        

        var eventText = "";
        if (events != null)
        {
            foreach (var e in events)
            {
                eventText += $"{e.AsText()}, ";
            }
            eventText = eventText.Trim(',', ' ');
        }

        return eventText;
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

    private void ChangeSelection(int index)
    {
        if (index == -1)
        {
            this.EmitSignal(SignalName.MenuSelectionChanged, (int)BuildingType.None);
        }
        else
        {
            var name = MenuItems.GetItemText(index);


            BuildingType bt = this.AvailableItems[index].Type;

            this.EmitSignal(SignalName.MenuSelectionChanged, (int)bt);
        }
    }

    public override void _Input(InputEvent @event)
    {
        for (int i = 0; i < this.AvailableItems.Count; i++) {
            var itemData = this.AvailableItems[i];
            if(@event.IsActionPressed(itemData.ActionName))
            {
                MenuItems.Select(i);
                ChangeSelection(i);
            }
        }

        if (@event.IsActionPressed("Cancel Selection"))
        {
            MenuItems.DeselectAll();
            ChangeSelection(-1);
        }
    }
}




