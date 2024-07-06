class_name UnitFactory

enum UnitType {BUILDER, SERVANT}

static func new_unit(unit_type: UnitType, gameManager:GameManager):
	var unit
	if unit_type == UnitType.BUILDER:
		unit = preload("res://assets/units/builder.tscn")
	elif unit_type == UnitType.SERVANT:
		unit = preload("res://assets/units/servant.tscn")

	var instance = unit.instantiate()
	print("instance is %s" % instance)
	instance.setup(gameManager)
	return instance
