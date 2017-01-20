(define (problem 01)
	(:domain ALIEN)
	(:objects 
		RIPLEY CAT ALIEN PIPE
		SLEEPCHAMBER
		BRIDGE KITCHEN SOUTHPASSAGE EASTPASSAGE AUTODOCK SLEEPVAULT SHUTTLE MOTHER STORAGE COCKPIT SPACE
		DOOR
		FLIGHTCONTROL DOORCONTROL
		SPACESUIT GRAPPLE
		CHAIR
	)
	(:init    
		(player ripley) (character ripley) (alive ripley) (human ripley) (at ripley bridge)
		(character alien) (alien alien) (alive alien) (at alien eastpassage)
		(cat cat) (at cat bridge) (alive cat)
		(sleepchamber sleepchamber) (at sleepchamber shuttle)
		(location bridge) (connected bridge kitchen) (connected bridge mother) (connected bridge southpassage)
		(location kitchen) (connected kitchen bridge) (connected kitchen southpassage) (connected kitchen eastpassage)
		(location southpassage) (connected southpassage bridge) (connected southpassage kitchen) 
			(connected southpassage autodock)
		(location eastpassage) (connected eastpassage sleepvault) (connected eastpassage kitchen)
			(connected eastpassage autodock)
		(location autodock) (connected autodock eastpassage) (connected autodock southpassage)
			(connected autodock shuttle)
		(location sleepvault) (connected sleepvault eastpassage)
		(location shuttle) (connected shuttle autodock) (connected shuttle storage) (connected shuttle cockpit) 
			(ship shuttle)
		(door door) (between door shuttle autodock) (between door autodock shuttle) (open door)
		(chair chair) (at chair shuttle)
		(location cockpit) (connected cockpit shuttle)
		(controlpanel flightcontrol) (at flightcontrol cockpit)
		(location storage) (connected storage shuttle)
		(spacesuit spacesuit) (at spacesuit storage)
		(gun grapple) (at grapple storage) (loaded grapple)
		(location mother) (connected mother bridge)
		(location space)
	)
	(:goal 
		(and
			(stalking alien) (alive ripley) (alive cat) (closed door) (not (alive alien))
		)
	)
)