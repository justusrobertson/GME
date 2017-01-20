;;;
;;; The Excalibur problem!
;;;
(define (problem 01)
  (:domain arth)
  (:objects arthur merlin - character
            clearing woods - location
            excalibur - sword
			spellbook merlinbook - book
			ring - ring
			thing - thing)
  (:init (player arthur)
		 (at arthur woods)
		 (intends arthur (has arthur excalibur))
		 (at merlin clearing)
		 (has merlin merlinbook)
		 (asleep merlin)
		 (intends merlin (has arthur excalibur))
		 (at excalibur clearing)
		 (enchanted excalibur)
		 (at ring woods)
		 (at spellbook woods)
		 (connected woods clearing)
		 (connected clearing woods))
  (:goal (has arthur excalibur)))