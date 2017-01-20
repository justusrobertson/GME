;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;;; Bank World
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

(define 
	(domain BANK)
	(:requirements :strips)
	(:predicates (character ?x)
		(at ?x ?y)
	    (has ?x ?y)
	    (object ?x)
	    (location ?x)
		(color ?x ?y)
	)

	(:action shoot-person
	    :parameters (?shooter ?victim ?location ?gun)
	    :precondition 
			(and 
				(character ?shooter) (character ?victim) (location ?location) (gun ?gun)
				(alive ?shooter) (alive ?victim) (at ?shooter ?location) (at ?victim ?location) (has ?shooter ?gun)
				(not (has ?victim ?gun))
			)
	    :effect
			(and 
				(not (alive ?victim))
			)
	)

	(:action open-vault
	     :parameters (?opener ?thing ?room)
	     :precondition 
			(and 
				(character ?opener) (at ?opener ?room) (at ?thing ?room) (not (open ?thing)) (alive ?opener)
				(closed ?thing)
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

	(:action take-thing
	    :parameters (?taker ?thing)
	    :precondition 
			(and 
				(character ?taker) (thing ?thing) (alive ?taker) (open vault) (at ?taker vault-room) (in ?thing vault)
			)
	    :effect
			(and 
				(not (in ?thing vault)) (has ?taker ?thing)
			)
	)
)
