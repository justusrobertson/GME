;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;;; ARTHUR World
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

(define 
	(domain ARTHUR)
	(:requirements :strips)
	(:predicates 
		(character ?x)
		(at ?x ?y)
	    (has ?x ?y)
	    (object ?x)
	    (location ?x)
		(color ?x ?y)
	)
	
	(:action move-location
	    :parameters (?mover ?location ?oldlocation)
	    :precondition 
			(and 
				(character ?mover) (at ?mover ?oldlocation) (not (at ?mover ?location)) (not (asleep ?mover))
				(location ?location) (connected ?location ?oldlocation)
				(location ?oldlocation)
			)
	    :effect
			(and 
				(not (at ?mover ?oldlocation))
				(at ?mover ?location)
			)
	)
	
	(:action wake-person
	    :parameters (?waker ?waked ?location)
	    :precondition 
			(and 
				(character ?waker) (at ?waker ?location) (not (asleep ?waker)) (at ?waker ?location)
				(character ?waked) (asleep ?waked) (at ?waked ?location)
				(location ?location)
			)
	    :effect
			(and 
				(not (asleep ?waked))
			)
	)
	
	(:action take-thing
	    :parameters (?taker ?thing ?location)
	    :precondition 
			(and 
				(character ?taker) (at ?taker ?location)
				(is ?thing thing) (at ?thing ?location) (not (enchanted ?thing))
				(location ?location)
			)
	    :effect
			(and 
				(not (at ?thing ?location))
				(has ?taker ?thing)
			)
	)
	
	(:action disenchant-sword
	    :parameters (?caster ?sword ?book ?location)
	    :precondition 
			(and 
				(character ?caster) (at ?caster ?location) (has ?caster ?book) (not (asleep ?caster))
				(sword ?sword) (at ?sword ?location) (enchanted ?sword)
				(book ?book)
				(location ?location)
			)
	    :effect
			(and 
				(not (enchanted ?sword))
			)
	)
)
