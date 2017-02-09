(define
	(domain CEDEL-TEST)
	(:requirements :adl :typing :universal-preconditions)
	(:types 
	)
	(:constants )
	(:predicates
	)

	(:action take-thing
		:parameters (?taker ?thing ?location )
		:precondition
			(and
				(character ?taker)
				(at ?taker ?location)
				(thing ?thing)
				(at ?thing ?location)
			)
		:effect
			(and
				(not (at ?thing ?location))
				(has ?taker ?thing)
				(when
					(book ?thing)
					(magical ?taker)
				)
			)
	)
)
