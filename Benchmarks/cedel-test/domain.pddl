;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;;; ALIEN world
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

(define 
	(domain CEDEL-TEST)
	(:requirements :strips :universal-preconditions)
	(:predicates 
		(character ?x)
		(at ?x ?y)
	    (has ?x ?y)
	    (object ?x)
	    (location ?x)
		(color ?x ?y)
	)
	
	(:action take-thing
	    :parameters (?taker ?thing ?location)
	    :precondition 
			(and 
				(character ?taker) (at ?taker ?location)
				(thing ?thing) (at ?thing ?location)
			)
	    :effect
			(and 
				(not (at ?thing ?location))
				(has ?taker ?thing)
				(when 
					(ring ?thing)
					(magical ?taker)
				)
				(when 
					(book ?thing)
					(magical ?taker)
				)
			)
	)
)
