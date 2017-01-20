;;;
;;; The Excalibur domain!
;;;
(define (domain arth)
  (:requirements :adl :domain-axioms :intentionality)
  (:types character location - object
          ring book sword - item
		  thing - thing)
  (:constants )
  (:predicates (player ?character - character)
			   (at ?character - character ?location - location)
			   (at ?thing - item ?location - location)
			   (has ?character - character ?item - item)
			   (asleep ?character - character)
			   (connected ?location - location ?oldlocation - location)
			   (enchanted ?item - item)
			   (is ?item - item ?thing - thing))

  ;; A character moves from one place to another.
  (:action move-location
    :parameters   (?character - character ?to - location ?from - location)
	:precondition (and (connected ?to ?from)
                       (not (asleep ?character))
                       (at ?character ?from))
	:effect       (and (not (at ?character ?from))
                       (at ?character ?to))
    :agents       (?character))
	
  ;; A character wakes another.
  (:action wake-person
    :parameters   (?waker - character ?woken - character ?location - location)
	:precondition (and (at ?waker ?location)
					   (at ?woken ?location)
					   (asleep ?woken)
					   (not (asleep ?waker)))
	:effect       (not (asleep ?woken))
    :agents       (?waker))
	
  ;; A character takes something.
  (:action take-thing
    :parameters   (?taker - character ?thing - item ?location - location)
	:precondition (and (at ?taker ?location)
					   (at ?thing ?location)
					   (not (asleep ?taker))
					   (not (enchanted ?thing)))
	:effect       (and (not (at ?thing ?location))
					   (has ?taker ?thing))
    :agents       (?taker))
	
  ;; A character disenchants something.
  (:action disenchant-thing
    :parameters   (?caster - character ?thing - item ?book - book ?location - location)
	:precondition (and (at ?caster ?location)
					   (at ?thing ?location)
					   (not (asleep ?caster))
					   (has ?caster ?book)
					   (enchanted ?thing))
	:effect       (not (enchanted ?thing))
    :agents       (?caster)))