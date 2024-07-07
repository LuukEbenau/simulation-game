extends "res://addons/gd-plug/plug.gd"

# Misc: useful tools?
# https://github.com/gdquest-demos/godot-shaders <-- seems like a popular shader library
# godot-orchestrator <-- seems insanely useful! but not sure how easy installation is gonna be


func _plugging():
		# Resource generation
		#plug("Tshmofen/waterways-net")

		# Behaviour trees, etc.
		plug("limbonaut/limboai")

		# Level builders
		plug("blackears/cyclopsLevelBuilder")
		# Or use tiled? https://www.mapeditor.org/ https://github.com/vnen/godot-tiled-importer

		# Testing
		#plug("MikeSchulze/gdUnit4")

		# Misc
		#plug("Ericdowney/SignalVisualizer")



		# If we ever need multiplayer functionality
		# plug("AndreMicheletti/godot-agones-sdk")
