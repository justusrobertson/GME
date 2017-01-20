;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;;; ZOMBIE World
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

(define 
	(domain ZOMBIE)
	(:requirements :strips)
	(:predicates 
		(character ?x)
		(at ?x ?y)
	    (has ?x ?y)
	    (object ?x)
	    (location ?x)
		(color ?x ?y)
	)
	
	(:action posess-person
	    :parameters (?spirit ?person)
	    :precondition 
			(and 
				(spirit ?spirit) (alive ?person) (free ?spirit)
			)
	    :effect
			(and 
				(not (alive ?person))
				(zombie ?person)
			)
	)
	
	(:action read-book
	    :parameters (?person ?book ?location)
	    :precondition 
			(and 
				(character ?person) (book ?book) (evil ?book) (has ?person ?book) (at ?person ?location)
			)
	    :effect
			(and 
				(free evilspirit)
			)
	)

	(:action shoot-zombie
	    :parameters (?shooter ?zombie ?location ?gun)
	    :precondition 
			(and 
				(character ?shooter) (character ?zombie) (location ?location) (gun ?gun)
				(alive ?shooter) (zombie ?zombie) (at ?shooter ?location) (at ?zombie ?location) (has ?shooter ?gun)
			)
	    :effect
			(and 
				(not (zombie ?zombie))
			)
	)
	
	(:action chop-zombie
	    :parameters (?chopper ?zombie ?location ?axe)
	    :precondition 
			(and 
				(character ?chopper) (character ?zombie) (location ?location) (axe ?axe)
				(alive ?chopper) (zombie ?zombie) (at ?chopper ?location) (at ?zombie ?location) (has ?chopper ?axe)
			)
	    :effect
			(and 
				(not (zombie ?zombie))
			)
	)
	
	(:action chop-thing
	    :parameters (?chopper ?cabinet ?location ?axe)
	    :precondition 
			(and 
				(character ?chopper) (cabinet ?cabinet) (location ?location) (axe ?axe)
				(alive ?chopper) (at ?chopper ?location) (at ?cabinet ?location) (has ?chopper ?axe)
			)
	    :effect
			(and 
				(not (locked ?cabinet)) (not (closed ?cabinet)) (open ?cabinet)
			)
	)
	
	(:action scratch-person
	    :parameters (?zombie ?person ?location)
	    :precondition 
			(and 
				(zombie ?zombie) (location ?location)
				(character ?person) (alive ?person) (at ?zombie ?location) (at ?person ?location)
			)
	    :effect
			(and 
				(hurt ?person)
			)
	)
	
	(:action unlock-thing
	     :parameters (?unlocker ?thing ?key ?room)
	     :precondition 
			(and 
				(character ?unlocker) (at ?unlocker ?room) (at ?thing ?room) (locked ?thing) (alive ?unlocker)
				(has ?unlocker ?key) (key ?key) (unlocks ?key ?thing)
			)
	    :effect
			(and 
				(not (locked ?thing))
			)
	)

	(:action open-thing
	     :parameters (?opener ?thing ?room)
	     :precondition 
			(and 
				(character ?opener) (at ?opener ?room) (at ?thing ?room) (not (open ?thing)) (alive ?opener)
				(closed ?thing) (not (locked ?thing))
			)
	    :effect
			(and 
				(not (closed ?thing)) (open ?thing)
			)
	)
	
	(:action move-location
	    :parameters (?mover ?location ?oldlocation)
	    :precondition 
			(and 
				(character ?mover) (location ?location) (location ?oldlocation)
				(at ?mover ?oldlocation) (not (at ?mover ?location)) (alive ?mover)
				(connected ?location ?oldlocation)
			)
	    :effect
			(and 
				(not (at ?mover ?oldlocation))
				(at ?mover ?location)
			)
	)
	
	(:action move-zombie
	    :parameters (?mover ?location ?oldlocation)
	    :precondition 
			(and 
				(character ?mover) (location ?location) (location ?oldlocation)
				(at ?mover ?oldlocation) (not (at ?mover ?location)) (zombie ?mover)
				(connected ?location ?oldlocation)
			)
	    :effect
			(and 
				(not (at ?mover ?oldlocation))
				(at ?mover ?location)
			)
	)

	(:action take-from
	    :parameters (?taker ?thing ?in ?place)
	    :precondition 
			(and 
				(character ?taker) (alive ?taker) (open ?in) (at ?taker ?place) (in ?thing ?in)
				(at ?in ?place)
			)
	    :effect
			(and 
				(not (in ?thing ?in)) (has ?taker ?thing)
			)
	)
	
	(:action take-thing
	    :parameters (?taker ?thing ?place)
	    :precondition 
			(and 
				(character ?taker) (alive ?taker) (not (character ?thing)) (at ?taker ?place) (at ?thing ?place)
				(not (cabinet ?thing))
			)
	    :effect
			(and 
				(not (at ?thing ?place)) (has ?taker ?thing)
			)
	)
)
