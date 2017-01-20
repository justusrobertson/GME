(define (problem 01)
	(:domain ZOMBIE)
	(:objects 
		ASH LINDA EVILSPIRIT
		LIVINGROOM BEDROOM OUTSIDE WOODSHED CELLAR
		CABINET 
		KEY NECRONOMICON
		BOOMSTICK AXE
	)
	(:init    
		(player ash) (character ash) (alive ash) (at ash livingroom)
		(character linda) (alive linda) (at linda livingroom)
		(character spirit) (spirit evilspirit)
		(location livingroom) (connected livingroom bedroom) (connected livingroom outside) (connected livingroom cellar)
		(location bedroom) (connected bedroom livingroom)
		(location outside) (connected outside livingroom) (connected outside woodshed)
		(location woodshed) (connected woodshed outside)
		(location cellar) (connected cellar livingroom)
		(cabinet cabinet) (closed cabinet) (locked cabinet) (at cabinet livingroom)
		(key key) (at key bedroom) (unlocks key cabinet) (old key)
		(book necronomicon) (evil necronomicon) (at necronomicon cellar)
		(gun boomstick) (in boomstick cabinet)
		(axe axe) (at axe woodshed)
	)
	(:goal 
		(and
			(not (zombie linda)) (not (alive linda)) (hurt ash)
		)
	)
)